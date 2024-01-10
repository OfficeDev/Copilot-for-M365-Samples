using msgext_doc_search_csharp;
using msgext_doc_search_csharp.Search;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

// Create the Bot Framework Authentication to be used with the Bot Adapter.
var config = builder.Configuration.Get<ConfigOptions>();
builder.Configuration["MicrosoftAppType"] = "MultiTenant";
builder.Configuration["MicrosoftAppId"] = config.BOT_ID;
builder.Configuration["MicrosoftAppPassword"] = config.BOT_PASSWORD;
builder.Configuration["AZURE_OPENAI_SERVICE_NAME"] = config.AZURE_OPENAI_SERVICE_NAME;
builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] = config.AZURE_OPENAI_DEPLOYMENT_NAME;
builder.Configuration["AZURE_OPENAI_API_VERSION"] = config.AZURE_OPENAI_API_VERSION;
builder.Configuration["AZURE_OPENAI_API_KEY"] = config.AZURE_OPENAI_API_KEY;
builder.Configuration["AZURE_SEARCH_ENDPOINT"] = config.AZURE_SEARCH_ENDPOINT;
builder.Configuration["AZURE_SEARCH_ADMIN_KEY"] = config.AZURE_SEARCH_ADMIN_KEY;
builder.Configuration["AZURE_SEARCH_INDEX_NAME"] = config.AZURE_SEARCH_INDEX_NAME;
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

builder.Services.AddTransient<AISearch>();
// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, SearchApp>();

var app = builder.Build();

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