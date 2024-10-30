using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trey.Research.Declarative.Agent.Models;
using Trey.Research.Declarative.Agent.Services;
using Trey.Research.Declarative.Agent.Utilities;
using Microsoft.Extensions.Configuration; // Import for IConfiguration

namespace Trey.Research.Declarative.Agent.Services
{
    public class ConsultantApiService
    {
        private const int AVAILABLE_HOURS_PER_MONTH = 160;
        private readonly IConfiguration _configuration;

        // Constructor to accept IConfiguration
        public ConsultantApiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ApiConsultant> GetApiConsultantById(string consultantId)
        {
            // Pass the configuration object to the constructor
            var consultant = await new ConsultantDbService(_configuration).GetConsultantByIdAsync(consultantId);
            var assignments = await new AssignmentDbService(_configuration).GetAssignmentsAsync();

            var result = await GetApiConsultantForBaseConsultant(consultant, assignments);
            return result;
        }

        public async Task<List<ApiConsultant>> GetApiConsultants(
            string consultantName,
            string projectName,
            string skill,
            string certification,
            string role,
            string hoursAvailable)
        {
            var consultants = await new ConsultantDbService(_configuration).GetConsultantsAsync();
            var assignments = await new AssignmentDbService(_configuration).GetAssignmentsAsync();

            // Filter on base properties
            if (!string.IsNullOrEmpty(consultantName))
            {
                consultants = consultants.Where(c => c.Name.Contains(consultantName, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(skill))
            {
                consultants = consultants.Where(c => c.Skills.Any(s => s.Contains(skill, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            if (!string.IsNullOrEmpty(certification))
            {
                consultants = consultants.Where(c => c.Certifications.Any(s => s.Contains(certification, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            if (!string.IsNullOrEmpty(role))
            {
                consultants = consultants.Where(c => c.Roles.Any(r => r.Contains(role, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            // Augment the base properties with assignment information
            var result = new List<ApiConsultant>();
            foreach (var consultant in consultants)
            {
                result.Add(await GetApiConsultantForBaseConsultant(consultant, assignments));
            }

            // Filter on project name
            if (!string.IsNullOrEmpty(projectName))
            {
                result = result.Where(c =>
                    c.Projects.Any(p =>
                        (p.ProjectName + p.ClientName).Contains(projectName, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Filter on available hours
            if (!string.IsNullOrEmpty(hoursAvailable) && int.TryParse(hoursAvailable, out int hours))
            {
                result = result.Where(c =>
                {
                    int availableHours = (int)(AVAILABLE_HOURS_PER_MONTH * 2 - c.ForecastThisMonth - c.ForecastNextMonth);
                    return availableHours >= hours;
                }).ToList();
            }

            return result;
        }

        public async Task<ApiConsultant> CreateApiConsultant(Consultant consultant)
        {
            // Create consultant in the database
            await new ConsultantDbService(_configuration).CreateConsultantAsync(consultant);
            var assignments = await new AssignmentDbService(_configuration).GetAssignmentsAsync();

            // Map the base consultant and assignments to an ApiConsultant object
            var newApiConsultant = await GetApiConsultantForBaseConsultant(consultant, assignments);

            return newApiConsultant;
        }

        private async Task<ApiConsultant> GetApiConsultantForBaseConsultant(Consultant consultant, List<Assignment> assignments)
        {
            var result = new ApiConsultant
            {
                Id = consultant.Id,
                Name = consultant.Name,
                Email = consultant.Email,
                Phone = consultant.Phone,
                ConsultantPhotoUrl = consultant.ConsultantPhotoUrl,                
                Location = consultant.Location,
                Skills = consultant.Skills,
                Certifications = consultant.Certifications,
                Roles = consultant.Roles,
                Projects = new List<ApiConsultantAssignment>(),
                ForecastThisMonth = 0,
                ForecastNextMonth = 0,
                DeliveredLastMonth = 0,
                DeliveredThisMonth = 0
            };

            assignments = assignments.Where(a => a.ConsultantId == consultant.Id).ToList();

            foreach (var assignment in assignments)
            {
                var project = await new ProjectDbService(_configuration).GetProjectByIdAsync(assignment.ProjectId);
                var forecastHours = FindHours(assignment.Forecast);
                var deliveredHours = FindHours(assignment.Delivered);

                result.Projects.Add(new ApiConsultantAssignment
                {
                    ProjectName = project.Name,
                    ProjectDescription = project.Description,
                    ProjectLocation = project.Location,
                    MapUrl = project.MapUrl,
                    ClientName = project.ClientName,
                    ClientContact = project.ClientContact,
                    ClientEmail = project.ClientEmail,
                    Role = assignment.Role,
                    ForecastThisMonth = forecastHours.ThisMonthHours,
                    ForecastNextMonth = forecastHours.NextMonthHours,
                    DeliveredLastMonth = deliveredHours.LastMonthHours,
                    DeliveredThisMonth = deliveredHours.ThisMonthHours
                });

                result.ForecastThisMonth += forecastHours.ThisMonthHours;
                result.ForecastNextMonth += forecastHours.NextMonthHours;
                result.DeliveredLastMonth += deliveredHours.LastMonthHours;
                result.DeliveredThisMonth += deliveredHours.ThisMonthHours;
            }

            return result;
        }

        private HoursSummary FindHours(List<HoursEntry> hours)
        {
            var now = DateTime.Now;
            var thisMonth = now.Month;
            var thisYear = now.Year;

            var lastMonth = thisMonth == 1 ? 12 : thisMonth - 1;
            var lastYear = thisMonth == 1 ? thisYear - 1 : thisYear;

            var nextMonth = thisMonth == 12 ? 1 : thisMonth + 1;
            var nextYear = thisMonth == 12 ? thisYear + 1 : thisYear;

            return new HoursSummary
            {
                LastMonthHours = (int)(hours.FirstOrDefault(h => h.Month == lastMonth && h.Year == lastYear)?.Hours ?? 0),
                ThisMonthHours = (int)(hours.FirstOrDefault(h => h.Month == thisMonth && h.Year == thisYear)?.Hours ?? 0),
                NextMonthHours = (int)(hours.FirstOrDefault(h => h.Month == nextMonth && h.Year == nextYear)?.Hours ?? 0)
            };
        }

        public async Task<ApiChargeTimeResponse> ChargeTimeToProject(string projectName, string consultantId, double hours)
        {
            var projects = await new ProjectApiService(_configuration).GetApiProjects(projectName, "");
            if (!projects.Any())
            {
                throw new Utilities.HttpError(404, $"Project not found: {projectName}");
            }
            else if (projects.Count > 1)
            {
                throw new Utilities.HttpError(406, $"Multiple projects found with the name: {projectName}");
            }
            else
            {
                var project = projects.First();
                var month = DateTime.Now.Month;
                var year = DateTime.Now.Year;
                var remainingForecast = await new AssignmentDbService(_configuration).ChargeHoursToProjectAsync(project.Id, consultantId, month, year, hours);
                string message;

                if (remainingForecast < 0)
                {
                    message = $"Charged {hours} hours to {project.ClientName} on project \"{project.Name}\". You are {-remainingForecast} hours over your forecast this month.";
                }
                else
                {
                    message = $"Charged {hours} hours to {project.ClientName} on project \"{project.Name}\". You have {remainingForecast} hours remaining this month.";
                }

                return new ApiChargeTimeResponse
                {
                    ClientName = project.ClientName,
                    ProjectName = project.Name,
                    RemainingForecast = remainingForecast,
                    Message = message
                };
            }
        }
    }
}