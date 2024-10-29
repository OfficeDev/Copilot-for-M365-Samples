using System.Collections.Generic;

namespace Trey.Research.Declarative.Agent.Models
{
    public class Location
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class HoursEntry
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public double Hours { get; set; }
    }

    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientName { get; set; }
        public string ClientContact { get; set; }
        public string ClientEmail { get; set; }
        public Location Location { get; set; }
        public string MapUrl { get; set; }
    }

    public class Consultant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ConsultantPhotoUrl { get; set; }
        public Location Location { get; set; }
        public List<string> Skills { get; set; }
        public List<string> Certifications { get; set; }
        public List<string> Roles { get; set; }
    }

    public class Assignment
    {
        public string Id { get; set; }  // The assignment ID is "projectid,consultantid"
        public string ProjectId { get; set; }
        public string ConsultantId { get; set; }
        public string Role { get; set; }
        public string Billable { get; set; }
        public double Rate { get; set; }
        public List<HoursEntry> Forecast { get; set; }
        public List<HoursEntry> Delivered { get; set; }
    }
    public class HoursSummary
    {
        public int LastMonthHours { get; set; }
        public int ThisMonthHours { get; set; }
        public int NextMonthHours { get; set; }
    }
}