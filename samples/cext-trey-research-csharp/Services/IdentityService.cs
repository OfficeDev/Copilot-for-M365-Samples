using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;

namespace cext_trey_research_csharp.Services
{
    public class IdentityService
    {
        private const string DEFAULT_CONSULTANT_ID = "1";
        private const string DEFAULT_CONSULTANT_NAME = "Avery Howard";
        private const string DEFAULT_CONSULTANT_EMAIL = "avery@treyresearch.com";

        private static int requestCounter = 0; // Singleton to identify each request

        public int RequestNumber { get; }
        public string Id { get; }
        public string Name { get; private set; }
        public string Email { get; private set; }

        public IdentityService()
        {
            RequestNumber = ++requestCounter;
            Id = DEFAULT_CONSULTANT_ID;
            SetDefaultConsultantValues();
        }

        public void InitializeFromRequest(HttpRequest req)
        {
            // Check for auth header from Easy Auth
            var clientPrincipal = req.Headers["x-ms-client-principal"].FirstOrDefault();

            if (!string.IsNullOrEmpty(clientPrincipal))
            {
                var clientPrincipalObj = JsonSerializer.Deserialize<ClientPrincipal>(DecodeBase64(clientPrincipal));
                if (clientPrincipalObj != null)
                {
                    Name = clientPrincipalObj.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                    Email = clientPrincipalObj.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value;
                    Console.WriteLine($"🔐 Request {RequestNumber} authenticated as {Name} ({Email})");
                }
                else
                {
                    SetDefaultConsultantValues();
                }
            }
            else
            {
                SetDefaultConsultantValues();
            }
        }

        private void SetDefaultConsultantValues()
        {
            Name = DEFAULT_CONSULTANT_NAME;
            Email = DEFAULT_CONSULTANT_EMAIL;
            Console.WriteLine($"⭐ Request {RequestNumber}");
        }

        private static string DecodeBase64(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public string GetDbConsultantName(string apiConsultantName)
        {
            return Name.IndexOf(apiConsultantName, StringComparison.OrdinalIgnoreCase) < 0 ? apiConsultantName : DEFAULT_CONSULTANT_NAME;
        }

        private class ClientPrincipal
        {
            public Claim[] Claims { get; set; }
        }

        private class Claim
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}