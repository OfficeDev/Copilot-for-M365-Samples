using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Trey.Research.Declarative.Agent.Models
{
    public class DbEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }  
    }

    public class DbProject : DbEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientName { get; set; }
        public string ClientContact { get; set; }
        public string ClientEmail { get; set; }
        public string LocationJson { get; set; } // Store JSON string
        public Location Location
        {
            get => LocationJson == null ? null : JsonSerializer.Deserialize<Location>(LocationJson);
            set => LocationJson = JsonSerializer.Serialize(value);
        }
    }

    public class DbConsultant : DbEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ConsultantPhotoUrl { get; set; }

        public string LocationJson { get; set; } // Store JSON string
        public Location Location
        {
            get => LocationJson == null ? null : JsonSerializer.Deserialize<Location>(LocationJson);
            set => LocationJson = JsonSerializer.Serialize(value);
        }

        public string SkillsJson { get; set; } // Store JSON string
        public List<string> Skills
        {
            get => SkillsJson == null ? null : JsonSerializer.Deserialize<List<string>>(SkillsJson);
            set => SkillsJson = JsonSerializer.Serialize(value);
        }

        public string CertificationsJson { get; set; } // Store JSON string
        public List<string> Certifications
        {
            get => CertificationsJson == null ? null : JsonSerializer.Deserialize<List<string>>(CertificationsJson);
            set => CertificationsJson = JsonSerializer.Serialize(value);
        }

        public string RolesJson { get; set; } // Store JSON string
        public List<string> Roles
        {
            get => RolesJson == null ? null : JsonSerializer.Deserialize<List<string>>(RolesJson);
            set => RolesJson = JsonSerializer.Serialize(value);
        }
    }
    public class DbAssignment : DbEntity
    {
        public string Etag { get; set; }
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string ConsultantId { get; set; }
        public string Role { get; set; }
        public string Billable { get; set; }
        public double Rate { get; set; }

        public string ForecastJson { get; set; } // Store JSON string
        public List<HoursEntry> Forecast
        {
            get => ForecastJson == null ? null : JsonSerializer.Deserialize<List<HoursEntry>>(ForecastJson);
            set => ForecastJson = JsonSerializer.Serialize(value);
        }

        public string DeliveredJson { get; set; } // Store JSON string
        public List<HoursEntry> Delivered
        {
            get => DeliveredJson == null ? null : JsonSerializer.Deserialize<List<HoursEntry>>(DeliveredJson);
            set => DeliveredJson = JsonSerializer.Serialize(value);
        }
    }
}
