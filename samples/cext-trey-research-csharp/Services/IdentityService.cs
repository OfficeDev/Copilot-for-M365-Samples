using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Azure;
using Trey.Research.Declarative.Agent.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Trey.Research.Declarative.Agent.Services
{
    public class IdentityService
    {
        private int requestNumber = 1;  // Number the requests for logging purposes
        private readonly IConfiguration _configuration;

        public IdentityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ApiConsultant> ValidateRequest(HttpRequest req)
        {
            // Default user used for unauthenticated testing
            string userId = "1";
            string userName = "Avery Howard";
            string userEmail = "avery@treyresearch.com";

            // ** INSERT REQUEST VALIDATION HERE (see Lab E6) **

            // Get the consultant record for this user; create one if necessary
            ApiConsultant consultant = null;

            try
            {
                consultant = await new ConsultantApiService(_configuration).GetApiConsultantById(userId);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status != 404)
                {
                    throw ex;
                }
                // Consultant was not found, so we'll create one below
                consultant = null;
            }

            if (consultant == null)
            {
                consultant = await CreateConsultantForUser(userId, userName, userEmail);
            }

            return consultant;
        }

        private async Task<ApiConsultant> CreateConsultantForUser(string userId, string userName, string userEmail)
        {
            // Create a new consultant record for this user with default values
            Consultant consultant = new Consultant
            {
                Id = userId,
                Name = userName,
                Email = userEmail,
                Phone = "1-555-123-4567",
                ConsultantPhotoUrl = "https://microsoft.github.io/copilot-camp/demo-assets/images/consultants/Unknown.jpg",
                Location = new Location
                {
                    Street = "One Memorial Drive",
                    City = "Cambridge",
                    State = "MA",
                    Country = "USA",
                    PostalCode = "02142",
                    Latitude = 42.361366,
                    Longitude = -71.081257
                },
                Skills = new List<string> { "JavaScript", "TypeScript" },
                Certifications = new List<string> { "Azure Development" },
                Roles = new List<string> { "Architect", "Project Lead" }
            };

            ApiConsultant result = await new ConsultantApiService(_configuration).CreateApiConsultant(consultant);
            return result;
        }
    }
}