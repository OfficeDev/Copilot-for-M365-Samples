using System.Collections.Generic;
using System.Threading.Tasks;
using Trey.Research.Declarative.Agent.Services;  
using Trey.Research.Declarative.Agent.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace Trey.Research.Declarative.Agent.Services
{
    public class ConsultantDbService
    {
        private const string TableName = "Consultant";
        private readonly DbService<DbConsultant> _dbService;

        // Constructor to initialize DbService
        public ConsultantDbService(IConfiguration configuration)
        {
            _dbService = new DbService<DbConsultant>(configuration, true);
        }

        public async Task<Consultant> GetConsultantByIdAsync(string id)
        {
            var consultant = await _dbService.GetEntityByRowKeyAsync(TableName, id);
            return MapToConsultant(consultant as DbConsultant);
        }

        public async Task<List<Consultant>> GetConsultantsAsync()
        {
            var dbConsultants = await _dbService.GetEntitiesAsync(TableName);
            var consultants = new List<Consultant>();
            foreach (var dbConsultant in dbConsultants)
            {
                consultants.Add(MapToConsultant(dbConsultant as DbConsultant));
            }
            return consultants;
        }

        public async Task<Consultant> CreateConsultantAsync(Consultant consultant)
        {
            // Create a new DbConsultant entity with the necessary fields
            var newDbConsultant = new DbConsultant
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
                ETag = new Azure.ETag("*"),
                PartitionKey = TableName,
                RowKey = consultant.Id,
                Timestamp = DateTime.UtcNow
            };

            // Add the new DbConsultant to the table
            await _dbService.CreateEntityAsync(TableName, newDbConsultant.RowKey, newDbConsultant);

            Console.WriteLine($"Added new consultant {newDbConsultant.Name} ({newDbConsultant.Id}) to the Consultant table");

            return null; // You can change this to return the created consultant if needed
        }

        private Consultant MapToConsultant(DbConsultant dbConsultant)
        {
            return new Consultant
            {
                Id = dbConsultant.Id,
                Name = dbConsultant.Name,
                Email = dbConsultant.Email,
                Phone = dbConsultant.Phone,
                ConsultantPhotoUrl = dbConsultant.ConsultantPhotoUrl,
                Location = dbConsultant.Location,
                Skills = dbConsultant.Skills,
                Certifications = dbConsultant.Certifications,
                Roles = dbConsultant.Roles
            };
        }

    }
}