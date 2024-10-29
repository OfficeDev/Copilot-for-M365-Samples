using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Trey.Research.Declarative.Agent.Models;
using Trey.Research.Declarative.Agent.Services;
using Trey.Research.Declarative.Agent.Utilities;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace Trey.Research.Declarative.Agent
{
    public class ProjectsFunction
    {
        private readonly ProjectApiService _projectApiService;
        private readonly IdentityService _identityService;
        private readonly IConfiguration _configuration;

        public ProjectsFunction(ProjectApiService projectApiService, IdentityService identityService, IConfiguration configuration)
        {
            _projectApiService = projectApiService;
            _identityService = identityService;
            _configuration = configuration;
        }

        [FunctionName("Projects")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "projects/{id?}")] HttpRequest req,
            string id,
            ILogger log)
        {
            log.LogInformation("HTTP trigger function projects processed a request.");

            var jsonResponse = new { results = new List<object>() };

            try
            {
                var userInfo = await new IdentityService(_configuration).ValidateRequest(req);

                switch (req.Method.ToUpper())
                {
                    case "GET":
                        var projectName = req.Query["projectName"].ToString().ToLower() ?? "";
                        var consultantName = req.Query["consultantName"].ToString().ToLower() ?? "";

                        log.LogInformation($"➡️ GET /api/projects: request for projectName={projectName}, consultantName={consultantName}, id={id}");

                        projectName = Utility.CleanUpParameter("projectName", projectName);
                        consultantName = Utility.CleanUpParameter("consultantName", consultantName);

                        if (!string.IsNullOrEmpty(id))
                        {
                            var result = await _projectApiService.GetApiProjectById(id.ToLower());
                            jsonResponse = new { results = new List<object> { result } };  // Correct type usage
                            log.LogInformation($"   ✅ GET /api/projects: response status 200; 1 project returned");
                            return new OkObjectResult(jsonResponse);
                        }

                        // Use current user if the project name is user_profile
                        if (projectName.Contains("user_profile"))
                        {
                            var result = await _projectApiService.GetApiProjects("", userInfo.Name);
                            jsonResponse = new { results = result.Cast<object>().ToList() };  // Cast to List<object>
                            log.LogInformation($"   ✅ GET /api/projects for current user response status 200; {result.Count} projects returned");
                            return new OkObjectResult(jsonResponse);
                        }

                        var projectsResult = await _projectApiService.GetApiProjects(projectName, consultantName);
                        jsonResponse = new { results = projectsResult.Cast<object>().ToList() };  // Cast to List<object>
                        log.LogInformation($"   ✅ GET /api/projects: response status 200; {projectsResult.Count} projects returned");
                        return new OkObjectResult(jsonResponse);

                    case "POST":
                        if (string.IsNullOrEmpty(id))
                        {
                            throw new HttpError((int)HttpStatusCode.BadRequest, "Invalid command");
                        }

                        string requestBody;
                        using (var reader = new StreamReader(req.Body))
                        {
                            requestBody = await reader.ReadToEndAsync();
                        }

                        if (string.IsNullOrWhiteSpace(requestBody))
                        {
                            throw new HttpError((int)HttpStatusCode.BadRequest, "No body to process this request.");
                        }

                        dynamic body = JsonConvert.DeserializeObject(requestBody);

                        if (id.ToLower() == "assignconsultant")
                        {
                            var projectNamePost = Utility.CleanUpParameter("projectName", (string)body.projectName);
                            if (string.IsNullOrEmpty(projectNamePost))
                            {
                                throw new HttpError((int)HttpStatusCode.BadRequest, "Missing project name");
                            }

                            var consultantNamePost = Utility.CleanUpParameter("consultantName", (string)body.consultantName);
                            if (string.IsNullOrEmpty(consultantNamePost))
                            {
                                throw new HttpError((int)HttpStatusCode.BadRequest, "Missing consultant name");
                            }

                            var role = Utility.CleanUpParameter("Role", (string)body.role);
                            if (string.IsNullOrEmpty(role))
                            {
                                throw new HttpError((int)HttpStatusCode.BadRequest, "Missing role");
                            }

                            var forecast = (int?)body.forecast ?? 0;

                            log.LogInformation($"➡️ POST /api/projects: assignconsultant request, projectName={projectNamePost}, consultantName={consultantNamePost}, role={role}, forecast={forecast}");
                            var result = await _projectApiService.AddConsultantToProject(projectNamePost, consultantNamePost, role, forecast);

                            jsonResponse = new
                            {
                                results = new List<object>
                                {
                                    new
                                    {
                                        status = 200,
                                        clientName = result.ClientName,
                                        projectName = result.ProjectName,
                                        consultantName = result.ConsultantName,
                                        remainingForecast = result.RemainingForecast,
                                        message = result.Message
                                    }
                                }
                            };

                            log.LogInformation($"   ✅ POST /api/projects: response status 200 - {result.Message}");
                            return new OkObjectResult(jsonResponse);
                        }
                        else
                        {
                            throw new HttpError((int)HttpStatusCode.BadRequest, $"Invalid command: {id}");
                        }

                    default:
                        throw new HttpError((int)HttpStatusCode.MethodNotAllowed, $"Method not allowed: {req.Method}");
                }
            }
            catch (HttpError httpError)
            {
                log.LogError($"   ⛔ Returning error status code {(int)httpError.Status}: {httpError.Message}");
                return new ObjectResult(new { error = httpError.Message }) { StatusCode = (int)httpError.Status };
            }
            catch (Exception ex)
            {
                log.LogError($"   ⛔ Returning error status code 500: {ex.Message}");
                return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
            }
        }
    }
}