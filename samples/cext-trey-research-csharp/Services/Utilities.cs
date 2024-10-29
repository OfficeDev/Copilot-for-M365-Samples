using Trey.Research.Declarative.Agent.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace Trey.Research.Declarative.Agent.Utilities
{
    // Custom exception to represent an HTTP error
    public class HttpError : Exception
    {
        public int Status { get; }

        public HttpError(int status, string message) : base(message)
        {
            Status = status;
        }
    }

    public static class Utility
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Clean up common issues with parameters
        public static string CleanUpParameter(string name, string value)
        {
            string val = value.ToLower();

            if (val.Contains("trey") || val.Contains("research"))
            {
                string newVal = val.Replace("trey", "").Replace("research", "").Trim();
                Console.WriteLine($"   ❗ Plugin name detected in the {name} parameter '{val}'; replacing with '{newVal}'.");
                val = newVal;
            }
            if (val == "<user_name>")
            {
                Console.WriteLine($"   ❗ Invalid name '{val}'; replacing with 'avery'.");
                val = "avery";
            }
            if (name == "role" && val == "consultant")
            {
                Console.WriteLine($"   ❗ Invalid role name '{val}'; replacing with ''.");
                val = "";
            }
            if (val == "null")
            {
                Console.WriteLine($"   ❗ Invalid value '{val}'; replacing with ''.");
                val = "";
            }

            return val;
        }
    }
}
