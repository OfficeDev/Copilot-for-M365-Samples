using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Trey.Research.Declarative.Agent.Models;
using Trey.Research.Declarative.Agent.Services;
using Trey.Research.Declarative.Agent.Utilities;
using Microsoft.Extensions.Configuration;

namespace Trey.Research.Declarative.Agent.Services
{
    public class ProjectApiService
    {
        private readonly IConfiguration _configuration;

        // Constructor to accept IConfiguration
        public ProjectApiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ApiProject> GetApiProjectById(string projectId)
        {
            var project = await new ProjectDbService(_configuration).GetProjectByIdAsync(projectId);
            var assignments = await new AssignmentDbService(_configuration).GetAssignmentsAsync();

            var result = await GetApiProject(project, assignments);
            return result;
        }

        public async Task<List<ApiProject>> GetApiProjects(
            string projectOrClientName,
            string consultantName)
        {
            var projects = await new ProjectDbService(_configuration).GetProjectsAsync();
            var assignments = await new AssignmentDbService(_configuration).GetAssignmentsAsync();

            // Filter on base properties
            if (!string.IsNullOrEmpty(projectOrClientName))
            {
                projects = projects.Where(
                    p => (p.Name?.ToLower().Contains(projectOrClientName.ToLower()) ?? false) ||
                         (p.ClientName?.ToLower().Contains(projectOrClientName.ToLower()) ?? false)).ToList();
            }

            // Remove duplicates
            projects = projects.GroupBy(p => p.Id)
                               .Select(g => g.First())
                               .ToList();

            // Augment the base properties with assignment information
            var result = (await Task.WhenAll(projects.Select(p => GetApiProject(p, assignments)))).ToList();

            // Filter on augmented properties
            if (!string.IsNullOrEmpty(consultantName))
            {
                result = result.Where(p =>
                    p.Consultants.Any(c =>
                        c.ConsultantName.ToLower().Contains(consultantName.ToLower()))
                ).ToList();
            }

            return result;
        }

        // Augment a project to get an ApiProject
        private async Task<ApiProject> GetApiProject(Project project, List<Assignment> assignments)
        {
            var result = new ApiProject
            {
                Id = project.Id,
                Name = project.Name,
                Location = project.Location,
                Description = project.Description,
                ClientName = project.ClientName,
                ClientEmail = project.ClientEmail,
                ClientContact = project.ClientContact,
                Consultants = new List<ApiProjectAssignment>(),
                ForecastThisMonth = 0,
                ForecastNextMonth = 0,
                DeliveredLastMonth = 0,
                DeliveredThisMonth = 0
            };

            assignments = assignments.Where(a => a.ProjectId == project.Id).ToList();

            foreach (var assignment in assignments)
            {
                var consultant = await new ConsultantDbService(_configuration).GetConsultantByIdAsync(assignment.ConsultantId);
                var forecastHours = FindHours(assignment.Forecast);
                var deliveredHours = FindHours(assignment.Delivered);

                result.Consultants.Add(new ApiProjectAssignment
                {
                    ConsultantName = consultant.Name,
                    ConsultantLocation = consultant.Location,
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

        // Extract this and next month's hours from an array of HoursEntry
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

        public async Task<ApiAddConsultantToProjectResponse> AddConsultantToProject(
            string projectName,
            string consultantName,
            string role,
            int hours)
        {
            var projects = await GetApiProjects(projectName, string.Empty);
            var consultants = await new ConsultantApiService(_configuration).GetApiConsultants(consultantName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            if (!projects.Any())
            {
                throw new HttpError(404, $"Project not found: {projectName}");
            }
            else if (projects.Count > 1)
            {
                throw new HttpError(406, $"Multiple projects found with the name: {projectName}");
            }
            else if (!consultants.Any())
            {
                throw new HttpError(404, $"Consultant not found: {consultantName}");
            }
            else if (consultants.Count > 1)
            {
                throw new HttpError(406, $"Multiple consultants found with the name: {consultantName}");
            }

            var project = projects.First();
            var consultant = consultants.First();

            // Always charge to the current month
            var remainingForecast = await new AssignmentDbService(_configuration).AddConsultantToProjectAsync(project.Id, consultant.Id, role, hours);
            var message = $"Added consultant {consultant.Name} to {project.ClientName} on project \"{project.Name}\" with {remainingForecast} hours forecast this month.";

            return new ApiAddConsultantToProjectResponse
            {
                ClientName = project.ClientName,
                ProjectName = project.Name,
                ConsultantName = consultant.Name,
                RemainingForecast = remainingForecast,
                Message = message
            };
        }
    }
}