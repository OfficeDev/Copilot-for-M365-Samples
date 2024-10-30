using System.Collections.Generic;

namespace Trey.Research.Declarative.Agent.Models
{
    // Represents a project assignment for a consultant
    public class ApiProjectAssignment
    {
        public string ConsultantName { get; set; }
        public Location ConsultantLocation { get; set; }
        public string Role { get; set; }
        public double ForecastThisMonth { get; set; }
        public double ForecastNextMonth { get; set; }
        public double DeliveredLastMonth { get; set; }
        public double DeliveredThisMonth { get; set; }
    }

    // Represents a project returned by all /api/projects GET requests
    public class ApiProject : Project
    {
        public List<ApiProjectAssignment> Consultants { get; set; }
        public double ForecastThisMonth { get; set; }
        public double ForecastNextMonth { get; set; }
        public double DeliveredLastMonth { get; set; }
        public double DeliveredThisMonth { get; set; }
    }

    // Represents a consultant assignment in a project
    public class ApiConsultantAssignment
    {
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public Location ProjectLocation { get; set; }
        public string MapUrl { get; set; }
        public string ClientName { get; set; }
        public string ClientContact { get; set; }
        public string ClientEmail { get; set; }
        public string Role { get; set; }
        public double ForecastThisMonth { get; set; }
        public double ForecastNextMonth { get; set; }
        public double DeliveredLastMonth { get; set; }
        public double DeliveredThisMonth { get; set; }
    }

    // Represents a consultant returned by all /api/consultants GET requests
    public class ApiConsultant : Consultant
    {
        public List<ApiConsultantAssignment> Projects { get; set; }
        public double ForecastThisMonth { get; set; }
        public double ForecastNextMonth { get; set; }
        public double DeliveredLastMonth { get; set; }
        public double DeliveredThisMonth { get; set; }
    }

    // Represents a request to charge time to a project
    public class ApiChargeTimeRequest
    {
        public string ProjectName { get; set; }
        public double Hours { get; set; }
    }

    // Represents a response from charging time to a project
    public class ApiChargeTimeResponse
    {
        public string ClientName { get; set; }
        public string ProjectName { get; set; }
        public double RemainingForecast { get; set; }
        public string Message { get; set; }
    }

    // Represents a request to assign a consultant to a project
    public class ApiAddConsultantToProjectRequest
    {
        public string ProjectName { get; set; }
        public string ConsultantName { get; set; }
        public string Role { get; set; }
        public double Hours { get; set; }
    }

    // Represents a response from assigning a consultant to a project
    public class ApiAddConsultantToProjectResponse
    {
        public string ClientName { get; set; }
        public string ProjectName { get; set; }
        public string ConsultantName { get; set; }
        public double RemainingForecast { get; set; }
        public string Message { get; set; }
    }

    // Represents an error result
    public class ErrorResult
    {
        public int Status { get; set; }
        public string Message { get; set; }
    }
}
