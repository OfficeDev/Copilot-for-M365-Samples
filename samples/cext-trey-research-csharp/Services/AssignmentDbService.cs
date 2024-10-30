using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trey.Research.Declarative.Agent.Models;
using Microsoft.Extensions.Configuration;

namespace Trey.Research.Declarative.Agent.Services
{
    public class AssignmentDbService
    {
        private const string TableName = "Assignment";
        private readonly DbService<DbAssignment> _dbService;

        // Constructor to inject IConfiguration and initialize DbService
        public AssignmentDbService(IConfiguration configuration)
        {
            // Pass both the IConfiguration and caching flag to DbService
            _dbService = new DbService<DbAssignment>(configuration, true); // Disable local caching
        }

        public async Task<List<Assignment>> GetAssignmentsAsync()
        {
            var assignments = (await _dbService.GetEntitiesAsync(TableName)).Cast<DbAssignment>();
            return assignments.Select(ConvertDbAssignment).ToList();
        }

        public async Task<int> ChargeHoursToProjectAsync(string projectId, string consultantId, int month, int year, double hours)
        {
            try
            {
                var dbAssignment = await _dbService.GetEntityByRowKeyAsync(TableName, $"{projectId},{consultantId}") as DbAssignment;
                if (dbAssignment == null)
                {
                    throw new Utilities.HttpError(404, "Assignment not found");
                }

                // Add the hours delivered
                if (dbAssignment.Delivered.Count == 0)
                {
                    dbAssignment.Delivered = new List<HoursEntry> { new HoursEntry { Month = month, Year = year, Hours = hours } };
                }
                else
                {
                    var existingDelivery = dbAssignment.Delivered.FirstOrDefault(d => d.Month == month && d.Year == year);
                    if (existingDelivery != null)
                    {
                        existingDelivery.Hours += hours;
                    }
                    else
                    {
                        dbAssignment.Delivered.Add(new HoursEntry { Month = month, Year = year, Hours = hours });
                    }
                }
                dbAssignment.Delivered = dbAssignment.Delivered.OrderBy(d => d.Year).ThenBy(d => d.Month).ToList();

                // Subtract the hours from the forecast
                double remainingForecast = -hours; // Use double if necessary
                if (dbAssignment.Forecast == null)
                {
                    dbAssignment.Forecast = new List<HoursEntry> { new HoursEntry { Month = month, Year = year, Hours = -hours } };
                }
                else
                {
                    var existingForecast = dbAssignment.Forecast.FirstOrDefault(f => f.Month == month && f.Year == year);
                    if (existingForecast != null)
                    {
                        existingForecast.Hours -= hours;
                        remainingForecast = existingForecast.Hours;
                    }
                    else
                    {
                        dbAssignment.Forecast.Add(new HoursEntry { Month = month, Year = year, Hours = -hours });
                    }
                }
                dbAssignment.Forecast = dbAssignment.Forecast.OrderBy(f => f.Year).ThenBy(f => f.Month).ToList();

                await _dbService.UpdateEntityAsync(TableName, dbAssignment);

                return (int)remainingForecast; // Cast to int if needed
            }
            catch (Exception)
            {
                throw new Utilities.HttpError(404, "Assignment not found");
            }
        }

        public async Task<int> AddConsultantToProjectAsync(string projectId, string consultantId, string role, int hours)
        {
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;

            DbAssignment dbAssignment;
            try
            {
                dbAssignment = await _dbService.GetEntityByRowKeyAsync(TableName, $"{projectId},{consultantId}") as DbAssignment;
            }
            catch
            {
                dbAssignment = null;
            }

            if (dbAssignment != null)
            {
                throw new Utilities.HttpError(403, "Assignment already exists");
            }

            try
            {
                var newAssignment = new DbAssignment
                {
                    Etag = "",
                    PartitionKey = TableName,
                    RowKey = $"{projectId},{consultantId}",
                    Timestamp = DateTime.Now,
                    Id = $"{projectId},{consultantId}",
                    ProjectId = projectId,
                    ConsultantId = consultantId,
                    Role = role,
                    Billable = "true",
                    Rate = 100,
                    Forecast = new List<HoursEntry> { new HoursEntry { Month = month, Year = year, Hours = hours } },
                    Delivered = new List<HoursEntry>()
                };

                await _dbService.CreateEntityAsync(TableName, newAssignment.Id, newAssignment);

                return hours;
            }
            catch (Exception)
            {
                throw new Utilities.HttpError(500, "Unable to add assignment");
            }
        }

        private Assignment ConvertDbAssignment(DbAssignment dbAssignment)
        {
            return new Assignment
            {
                Id = dbAssignment.Id,
                ProjectId = dbAssignment.ProjectId,
                ConsultantId = dbAssignment.ConsultantId,
                Role = dbAssignment.Role,
                Billable = dbAssignment.Billable,
                Rate = dbAssignment.Rate,
                Forecast = dbAssignment.Forecast,
                Delivered = dbAssignment.Delivered
            };
        }
    }
}