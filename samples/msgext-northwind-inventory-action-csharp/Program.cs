using NorthwindInventory;
using NorthwindInventory.Bots;
using NorthwindInventory.DbSetup;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Register configuration in the service collection
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Retrieve the storage connection string from the configuration
var storageConnectionString = builder.Configuration["StorageConnectionString"];

if (string.IsNullOrEmpty(storageConnectionString))
{
    throw new ArgumentNullException(nameof(storageConnectionString), "Storage connection string cannot be null or empty.");
}

// Set up services
builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

// Configure Bot Framework Authentication
builder.Services.AddSingleton(new ConfigurationBotFrameworkAuthentication(builder.Configuration));
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, SearchBot>();

// Register the AzureTableSetup service
builder.Services.AddSingleton(sp => new AzureTableSetup(storageConnectionString));

// Build the application
var app = builder.Build();

// Run the SetupTablesAndDataAsync method on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var azureTableSetup = services.GetRequiredService<AzureTableSetup>();
        await azureTableSetup.SetupTablesAndDataAsync();
    }
    catch (Exception ex)
    {
        // Handle exceptions
        Console.WriteLine($"An error occurred during table setup: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
