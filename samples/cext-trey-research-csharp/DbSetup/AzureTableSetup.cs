using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace Trey.Research.Declarative.Agent.DbSetup
{
    public class AzureTableSetup
    {
        private readonly string _connectionString;
        private readonly string[] TableNames = { "Project", "Consultant", "Assignment" };

        public AzureTableSetup(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SetupTablesAndDataAsync(bool reset = true)
        {
            var tableServiceClient = new TableServiceClient(_connectionString);

            if (reset)
            {
                await DeleteTablesAsync(tableServiceClient);
            }

            foreach (var tableName in TableNames)
            {
                await CreateAndPopulateTableAsync(tableServiceClient, tableName);
            }
        }

        private async Task DeleteTablesAsync(TableServiceClient tableServiceClient)
        {
            foreach (var tableName in TableNames)
            {
                try
                {
                    var tableClient = tableServiceClient.GetTableClient(tableName);
                    Console.WriteLine($"Deleting table: {tableName}");
                    await tableClient.DeleteAsync();
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    Console.WriteLine($"Table {tableName} does not exist.");
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"Error deleting table {tableName}: {ex.Message}");
                }
            }

            Console.WriteLine("Waiting for tables to be deleted...");
            await Task.Delay(1000); // Allow some time for table deletion
        }

        private async Task CreateAndPopulateTableAsync(TableServiceClient tableServiceClient, string tableName)
        {
            try
            {
                var tableClient = tableServiceClient.GetTableClient(tableName);
                Console.WriteLine($"Creating table: {tableName}");

                // Create table if it does not exist
                try
                {
                    await tableClient.CreateAsync();
                }
                catch (RequestFailedException ex) when (ex.Status == 409)
                {
                    Console.WriteLine($"Table {tableName} already exists.");
                }
                // Populate the table
                var jsonString = File.ReadAllText($"DbSetup/db/{tableName}.json");
                var entities = JsonSerializer.Deserialize<JsonDocument>(jsonString);

                if (entities?.RootElement.TryGetProperty("Rows", out var rows) == true)
                {
                    foreach (var row in rows.EnumerateArray())
                    {
                        var rowKey = row.GetProperty("Id").GetString() ?? Guid.NewGuid().ToString();
                        var entity = new Dictionary<string, string>();

                        foreach (var prop in row.EnumerateObject())
                        {
                            // Store complex types as JSON strings
                            if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                            {
                                entity[prop.Name+"Json"] = prop.Value.GetRawText(); // Store as JSON string
                            }
                            else
                            {
                                entity[prop.Name] = ConvertToString(prop.Value);
                            }
                        }

                        var tableEntity = new TableEntity(tableName, rowKey)
                        {
                            { "partitionKey", tableName },
                            { "rowKey", rowKey }
                        };

                        foreach (var kvp in entity)
                        {
                            tableEntity[kvp.Key] = kvp.Value;
                        }

                        await tableClient.AddEntityAsync(tableEntity);
                        Console.WriteLine($"Added entity to {tableName} with key {rowKey}");
                    }
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error creating or populating table {tableName}: {ex.Message}");
            }
        }

        private string ConvertToString(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetDecimal().ToString(),
                JsonValueKind.True => "True",
                JsonValueKind.False => "False",
                JsonValueKind.Object or JsonValueKind.Array => element.ToString(),
                _ => throw new InvalidOperationException("Unsupported JSON value kind.")
            };
        }
    }
}