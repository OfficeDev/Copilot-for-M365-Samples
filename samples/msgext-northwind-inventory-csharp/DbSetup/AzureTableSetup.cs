using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using msgext_northwind_inventory_csharp.Models;

namespace msgext_northwind_inventory_csharp.DbSetup
{
    public class AzureTableSetup
    {
        private readonly string _connectionString;

        public AzureTableSetup(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SetupTablesAndDataAsync()
        {
            var reset = true; // Adjust as needed based on your requirements

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
                    // Table not found, already deleted
                    Console.WriteLine($"Table {table} not found, already deleted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting table {table}: {ex.Message}");
                    // Handle or log the exception
                }
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
                TableNameConstants.CATEGORY,
                TableNameConstants.CUSTOMER,
                TableNameConstants.EMPLOYEE,
                TableNameConstants.ORDER,
                TableNameConstants.ORDER_DETAIL,
                TableNameConstants.PRODUCT,
                TableNameConstants.SUPPLIER
            };

            foreach (var table in tables)
            {
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
                        case TableNameConstants.CATEGORY:
                            var categories = JsonSerializer.Deserialize<List<Category>>(jsonString);
                            await AddEntitiesAsync(tableClient, categories, table);
                            break;
                        case TableNameConstants.CUSTOMER:
                            var customers = JsonSerializer.Deserialize<List<Customer>>(jsonString);
                            await AddEntitiesAsync(tableClient, customers, table);
                            break;
                        case TableNameConstants.EMPLOYEE:
                            var employees = JsonSerializer.Deserialize<List<Employee>>(jsonString);
                            await AddEntitiesAsync(tableClient, employees, table);
                            break;
                        case TableNameConstants.ORDER:
                            var orders = JsonSerializer.Deserialize<List<Order>>(jsonString);
                            await AddEntitiesAsync(tableClient, orders, table);
                            break;
                        case TableNameConstants.ORDER_DETAIL:
                            var orderDetails = JsonSerializer.Deserialize<List<OrderDetail>>(jsonString);
                            await AddEntitiesAsync(tableClient, orderDetails, table);
                            break;
                        case TableNameConstants.PRODUCT:
                            var products = JsonSerializer.Deserialize<List<Product>>(jsonString);
                            await AddEntitiesAsync(tableClient, products, table);
                            break;
                        case TableNameConstants.SUPPLIER:
                            var suppliers = JsonSerializer.Deserialize<List<Supplier>>(jsonString);
                            await AddEntitiesAsync(tableClient, suppliers, table);
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown table name: {table}");
                    }
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"Failed to create table {table}: {ex.Message}");
                    // Handle or log the exception
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error reading JSON file for {table}: {ex.Message}");
                    // Handle or log the exception
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating table {table}: {ex.Message}");
                    // Handle or log the exception
                }
            }
        }

        private async Task AddEntitiesAsync<T>(TableClient tableClient, List<T> entities, string tableName) where T : class, IRow
        {
            foreach (var entity in entities)
            {
                entity.PartitionKey = tableName;
                entity.RowKey = GetRowKey(entity);

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

        private string GetRowKey(IRow entity)
        {
            return entity switch
            {
                ICategory category => category.CategoryID.ToString(),
                ICustomer customer => customer.CustomerID,
                IEmployee employee => employee.EmployeeID.ToString(),
                IOrder order => order.OrderID.ToString(),
                IOrderDetail orderDetail => $"{orderDetail.OrderID}_{orderDetail.ProductID}",
                IProduct product => product.ProductID.ToString(),
                ISupplier supplier => supplier.SupplierID.ToString(),
                _ => throw new InvalidOperationException("Unknown entity type")
            };
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
