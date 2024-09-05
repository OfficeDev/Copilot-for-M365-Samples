using cext_trey_research_csharp.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace cext_trey_research_csharp.Utilities
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

        // Augment a location with a map URL
        public static Location GetLocationWithMap(Location location)
        {
            var result = location;
            var bingKey = _configuration["SECRET_BING_MAPS_KEY"];
            var coord = $"{location.Latitude},{location.Longitude}";
            result.MapUrl = $"https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/?{coord}mapSize=450,600&pp={coord}&key={bingKey}";

            return result;
        }
    }
}
