using System.Collections.Generic;
using System.Threading.Tasks;
using cext_trey_research_csharp.Services;  
using cext_trey_research_csharp.Models;
using Microsoft.Extensions.Configuration;

namespace cext_trey_research_csharp.Services
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

        public async Task<Consultant> GetConsultantByIdAsync(IdentityService identity, string id)
        {
            var consultant = await _dbService.GetEntityByRowKeyAsync(TableName, id);
            return ConvertDbConsultant(identity, consultant as DbConsultant);
        }

        public async Task<List<Consultant>> GetConsultantsAsync(IdentityService identity)
        {
            var consultants = await _dbService.GetEntitiesAsync(TableName);
            var result = new List<Consultant>();
            foreach (var c in consultants)
            {
                result.Add(ConvertDbConsultant(identity, c as DbConsultant));
            }
            return result;
        }

        private Consultant ConvertDbConsultant(IdentityService identity, DbConsultant dbConsultant)
        {
            var result = new Consultant
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

            if (dbConsultant.Id == identity.Id)
            {
                // If this is the current user, return the actual name and email
                result.Name = identity.Name;
                result.Email = identity.Email;
            }

            return result;
        }
    }
}