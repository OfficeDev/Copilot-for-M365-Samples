using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using NorthwindInventory.Models;

namespace NorthwindInventory.DbSetup
{
    public class AzureTableSetup(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        public async Task SetupTablesAndDataAsync(bool reset = true) // Adjust as needed based on your requirements
        {
            var tableServiceClient = new TableServiceClient(_connectionString);

            if (reset)
            {
                await DeleteAllTablesAsync(tableServiceClient);
            }

            await CreateTablesWithDataAsync(tableServiceClient);
        }

        private async Task DeleteAllTablesAsync(TableServiceClient tableServiceClient)
        {
            var tables = await GetAllTablesAsync(tableServiceClient);

            foreach (var table in tables)
            {
                try
                {
                    await tableServiceClient.DeleteTableAsync(table);
                    Console.WriteLine($"Deleted table: {table}");
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    Console.WriteLine($"Table {table} not found, already deleted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting table {table}: {ex.Message}");
                }
            }

            while (true)
            {
                tables = await GetAllTablesAsync(tableServiceClient);
                if (tables.Count == 0)
                {
                    Console.WriteLine("All tables deleted.");
                    break;
                }
                Console.WriteLine("Waiting for tables to be deleted...");
                await Task.Delay(1000);
            }
        }

        private async Task<List<string>> GetAllTablesAsync(TableServiceClient tableServiceClient)
        {
            var tables = new List<string>();
            await foreach (var table in tableServiceClient.QueryAsync())
            {
                tables.Add(table.Name);
            }
            return tables;
        }

        private async Task CreateTablesWithDataAsync(TableServiceClient tableServiceClient)
        {
            var tables = new List<string>
            {
                "Categories", "Customers", "Employees", "Orders", "OrderDetails", "Products", "Suppliers"
            };

            var rowKeyColumnNames = new List<string>
            {
                "CategoryID", "CustomerID", "EmployeeID", "OrderID", null, "ProductID", "SupplierID"
            };

            var generateImage = new List<bool>
            {
                false, true, false, false, false, true, true
            };

            var generateFlag = new List<bool>
            {
                false, true, false, false, false, false, true
            };

            for (int index = 0; index < tables.Count; index++)
            {
                var table = tables[index];
                var tableClient = tableServiceClient.GetTableClient(table);

                if (await TableExistsAsync(tableClient))
                {
                    Console.WriteLine($"Table {table} already exists, skipping creation.");
                    continue;
                }

                try
                {
                    await tableClient.CreateIfNotExistsAsync();
                    Console.WriteLine($"Created table: {table}");

                    var jsonString = File.ReadAllText($"DbSetup/db/{table}.json");

                    switch (table)
                    {
                        case "Categories":
                            var categories = JsonSerializer.Deserialize<List<Category>>(jsonString);
                            await AddEntitiesAsync(tableClient, categories, table, rowKeyColumnNames[index], generateImage[index], generateFlag[index]);
                            break;
                        case "Customers":
                            var customers = JsonSerializer.Deserialize<List<Customer>>(jsonString);
                            await AddEntitiesAsync(tableClient, customers, table, rowKeyColumnNames[index], generateImage[index], generateFlag[index]);
                            break;
                        case "Employees":
                            var employees = JsonSerializer.Deserialize<List<Employee>>(jsonString);
                            await AddEntitiesAsync(tableClient, employees, table, rowKeyColumnNames[index], generateImage[index], generateFlag[index]);
                            break;
                        case "Orders":
                            var orders = JsonSerializer.Deserialize<List<Order>>(jsonString);
                            await AddEntitiesAsync(tableClient, orders, table, rowKeyColumnNames[index], generateImage[index], generateFlag[index]);
                            break;
                        case "OrderDetails":
                            var orderDetails = JsonSerializer.Deserialize<List<OrderDetail>>(jsonString);
                            await AddEntitiesAsync(tableClient, orderDetails, table, rowKeyColumnNames[index], generateImage[index], generateFlag[index]);
                            break;
                        case "Products":
                            var products = JsonSerializer.Deserialize<List<Product>>(jsonString);
                            await AddEntitiesAsync(tableClient, products, table, rowKeyColumnNames[index], generateImage[index], generateFlag[index]);
                            break;
                        case "Suppliers":
                            var suppliers = JsonSerializer.Deserialize<List<Supplier>>(jsonString);
                            await AddEntitiesAsync(tableClient, suppliers, table, rowKeyColumnNames[index], generateImage[index], generateFlag[index]);
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown table name: {table}");
                    }
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"Failed to create table {table}: {ex.Message}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error reading JSON file for {table}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating table {table}: {ex.Message}");
                }
            }
        }

        private async Task AddEntitiesAsync<T>(TableClient tableClient, List<T> entities, string tableName, string rowKeyColumnName, bool generateImage, bool generateFlag) where T : class, IRow
        {
            foreach (var entity in entities)
            {
                entity.PartitionKey = tableName;
                entity.RowKey = rowKeyColumnName != null ? GetRowKey(entity, rowKeyColumnName) : Guid.NewGuid().ToString();

                var imageUrlProperty = typeof(T).GetProperty("ImageUrl");
                if (generateImage && (imageUrlProperty == null || string.IsNullOrEmpty(imageUrlProperty.GetValue(entity, null)?.ToString())))
                {
                    imageUrlProperty?.SetValue(entity, $"https://picsum.photos/seed/{entity.RowKey}/200/300");
                }

                var countryProperty = typeof(T).GetProperty("Country");
                var flagUrlProperty = typeof(T).GetProperty("FlagUrl");
                if (generateFlag && countryProperty != null)
                {
                    var country = countryProperty.GetValue(entity, null)?.ToString();
                    if (flagUrlProperty != null && !string.IsNullOrEmpty(country))
                    {
                        flagUrlProperty.SetValue(entity, GetFlagUrl(country));
                    }
                }

                var tableEntity = new TableEntity(entity.PartitionKey, entity.RowKey);

                foreach (var property in typeof(T).GetProperties())
                {
                    var value = property.GetValue(entity);
                    if (value != null)
                    {
                        tableEntity[property.Name] = value;
                    }
                }

                await tableClient.AddEntityAsync(tableEntity);
                Console.WriteLine($"Added entity to {tableName} with key {entity.RowKey}");
            }
        }

        private string GetRowKey<T>(T entity, string rowKeyColumnName) where T : class
        {
            var property = typeof(T).GetProperty(rowKeyColumnName);
            if (property == null)
            {
                throw new InvalidOperationException($"Row key property {rowKeyColumnName} not found on type {typeof(T).Name}");
            }

            var value = property.GetValue(entity);
            if (value == null)
            {
                throw new InvalidOperationException($"Row key value for property {rowKeyColumnName} is null on type {typeof(T).Name}");
            }

            return value.ToString();
        }

        private string GetFlagUrl(string country)
        {
            var countryCodes = new Dictionary<string, string>
            {
                { "australia", "au" },
                { "brazil", "br" },
                { "canada", "ca" },
                { "denmark", "dk" },
                { "france", "fr" },
                { "germany", "de" },
                { "finland", "fi" },
                { "italy", "it" },
                { "japan", "jp" },
                { "netherlands", "nl" },
                { "norway", "no" },
                { "singapore", "sg" },
                { "spain", "es" },
                { "sweden", "se" },
                { "uk", "gb" },
                { "usa", "us" }
            };

            return countryCodes.TryGetValue(country.ToLower(), out var code) ? $"https://flagcdn.com/32x24/{code}.png" : null;
        }

        private async Task<bool> TableExistsAsync(TableClient tableClient)
        {
            try
            {
                await tableClient.GetEntityAsync<TableEntity>("dummyPartitionKey", "dummyRowKey");
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }
    }
}
