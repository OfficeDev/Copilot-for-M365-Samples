using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using cext_trey_research_csharp.DbSetup;
using cext_trey_research_csharp.Services;
using System;
using cext_trey_research_csharp.Models;

[assembly: FunctionsStartup(typeof(cext_trey_research_csharp.Startup))]

namespace cext_trey_research_csharp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            var contentRootPath = builder.GetContext().ApplicationRootPath;

            if (configuration == null)
            {
                throw new InvalidOperationException("Configuration is not available.");
            }

            var storageConnectionString = configuration["AzureWebJobsStorage"];
            Utilities.Utility.Initialize(configuration);

            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new ArgumentNullException(nameof(storageConnectionString), "Storage connection string cannot be null or empty.");
            }

            // Pass the content root path to the AzureTableSetup instance
            var azureTableSetup = new AzureTableSetup(storageConnectionString);
            builder.Services.AddSingleton(azureTableSetup);

            // Register your services with the DI container
            builder.Services.AddSingleton<ConsultantApiService>();
            builder.Services.AddSingleton<IdentityService>();
            builder.Services.AddSingleton<ProjectApiService>();

            // Register DbService with the configuration and caching option
            builder.Services.AddSingleton<DbService<DbEntity>>(provider =>
                    new DbService<DbEntity>(configuration, okToCacheLocally: true));

            // Call the setup method to initialize the tables
            azureTableSetup.SetupTablesAndDataAsync().GetAwaiter().GetResult();
        }
    }
}