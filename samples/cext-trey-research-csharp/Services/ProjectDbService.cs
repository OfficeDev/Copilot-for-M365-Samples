using Trey.Research.Declarative.Agent.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trey.Research.Declarative.Agent.Services
{
    public class ProjectDbService
    {
        private const string TableName = "Project";
        private readonly IConfiguration _configuration;
        private readonly DbService<DbProject> _dbService;

        // Constructor to accept IConfiguration
        public ProjectDbService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Initialize _dbService using _configuration
            _dbService = new DbService<DbProject>(_configuration, true);
        }

        // NOTE: Projects are READ ONLY in this demo app, so we are free to cache them in memory.

        public async Task<Project> GetProjectByIdAsync(string id)
        {
            var dbProject = await _dbService.GetEntityByRowKeyAsync(TableName, id);
            return ConvertDbProject(dbProject as DbProject);
        }

        public async Task<List<Project>> GetProjectsAsync()
        {
            var dbProjects = await _dbService.GetEntitiesAsync(TableName);
            // Ensure all entities are of type DbProject
            var dbProjectList = dbProjects.OfType<DbProject>().ToList();
            // Convert each DbProject to Project
            return dbProjectList.Select(ConvertDbProject).ToList();
        }

        private Project ConvertDbProject(DbProject dbProject)
        {
            if (dbProject == null)
            {
                return null;
            }

            return new Project
            {
                Id = dbProject.Id,
                Name = dbProject.Name,
                Description = dbProject.Description,
                ClientName = dbProject.ClientName,
                ClientContact = dbProject.ClientContact,
                ClientEmail = dbProject.ClientEmail,
                Location = dbProject.Location,
                MapUrl = GetMapUrl(dbProject)
            };
        }

        private string GetMapUrl(DbProject project)
        {
            // Convert the client name to lower case and replace spaces with hyphens
            string companyNameKabobCase = project.ClientName.ToLower().Replace(" ", "-");

            // Return the formatted URL with the company name in kebab-case
            return $"https://microsoft.github.io/copilot-camp/demo-assets/images/maps/{companyNameKabobCase}.jpg";
        }
    }
}