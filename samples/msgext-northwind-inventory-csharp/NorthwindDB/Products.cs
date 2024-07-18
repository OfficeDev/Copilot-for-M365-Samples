using Azure.Data.Tables;
using NorthwindInventory.Models;
using NorthwindInventory.Handlers;
using Azure.Core;

namespace NorthwindInventory.NorthwindDB
{
    public class ProductService
    {
        private readonly TableClient _productTableClient;
        private readonly TableClient _categoryTableClient;
        private readonly TableClient _supplierTableClient;
        private readonly TableClient _orderDetailTableClient;

        private static Dictionary<string, Category> _categories;
        private static Dictionary<string, Supplier> _suppliers;
        private static Dictionary<string, OrderTotals> _orderTotals;

        public ProductService(IConfiguration configuration)
        {
            var connectionString = configuration["StorageConnectionString"]; 
            var clientOptions = new TableClientOptions
            {
                Retry =
                {
                    MaxRetries = 5,
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(10),
                    Mode = RetryMode.Exponential
                }
            };

            _productTableClient = new TableClient(connectionString, TableNames.Product, clientOptions);
            _categoryTableClient = new TableClient(connectionString, TableNames.Category, clientOptions);
            _supplierTableClient = new TableClient(connectionString, TableNames.Supplier, clientOptions);
            _orderDetailTableClient = new TableClient(connectionString, TableNames.OrderDetail, clientOptions);

            // Ensure tables are created
            CreateTablesIfNotExists();
        }

        private void CreateTablesIfNotExists()
        {
            _productTableClient.CreateIfNotExists();
            _categoryTableClient.CreateIfNotExists();
            _supplierTableClient.CreateIfNotExists();
            _orderDetailTableClient.CreateIfNotExists();
        }

        public async Task<List<IProductEx>> SearchProducts(string productName, string categoryName, string inventoryStatus, string supplierCity, string stockLevel)
        {
            var result = await GetAllProductsEx();

            if (!string.IsNullOrEmpty(productName))
            {
                result = result.Where(p => p.ProductName.StartsWith(productName, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(categoryName))
            {
                result = result.Where(p => p.CategoryName.StartsWith(categoryName, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(inventoryStatus))
            {
                result = result.Where(p => IsMatchingStatus(inventoryStatus, p)).ToList();
            }
            if (!string.IsNullOrEmpty(supplierCity))
            {
                result = result.Where(p => p.SupplierCity.StartsWith(supplierCity, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(stockLevel))
            {
                result = result.Where(p => IsInRange(stockLevel, p.UnitsInStock)).ToList();
            }

            return result;
        }

        public async Task<List<IProductEx>> GetDiscountedProductsByCategory(string categoryName)
        {
            var result = await GetAllProductsEx();
            result = result.Where(p => p.AverageDiscount > 5).ToList();

            if (!string.IsNullOrEmpty(categoryName))
            {
                result = result.Where(p => p.CategoryName.StartsWith(categoryName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return result;
        }

        public async Task<List<IProductEx>> GetProductsByRevenueRange(string revenueRange)
        {
            var result = await GetAllProductsEx();

            if (!string.IsNullOrEmpty(revenueRange))
            {
                var range = revenueRange;
                if (revenueRange.StartsWith("l", StringComparison.OrdinalIgnoreCase)) range = "0-5000";
                if (revenueRange.StartsWith("h", StringComparison.OrdinalIgnoreCase)) range = "50000-";

                result = result.Where(p => IsInRange(range, p.Revenue)).ToList();
            }

            return result;
        }

        private bool IsMatchingStatus(string inventoryStatusQuery, IProductEx product)
        {
            var query = inventoryStatusQuery.ToLower();

            return query switch
            {
                var q when q.StartsWith("out") => product.UnitsInStock == 0,
                var q when q.StartsWith("low") => product.UnitsInStock <= product.ReorderLevel,
                var q when q.StartsWith("on") => product.UnitsOnOrder > 0,
                _ => product.UnitsInStock > 0,
            };
        }

        private bool IsInRange(string rangeExpression, decimal value)
        {
            var result = false;
            if (!rangeExpression.Contains('-'))
            {
                if (decimal.TryParse(rangeExpression, out var val))
                {
                    result = value == val;
                }
            }
            else if (rangeExpression.EndsWith('-'))
            {
                if (decimal.TryParse(rangeExpression[..^1], out var lowerBound))
                {
                    result = value >= lowerBound;
                }
            }
            else
            {
                var bounds = rangeExpression.Split('-');
                if (decimal.TryParse(bounds[0], out var lowerBound) && decimal.TryParse(bounds[1], out var upperBound))
                {
                    result = lowerBound <= value && upperBound >= value;
                }
            }
            return result;
        }

        private async Task<Dictionary<string, T>> LoadReferenceData<T>(TableClient tableClient) where T : class, new()
        {
            var result = new Dictionary<string, T>();
            try
            {
                await foreach (var entity in tableClient.QueryAsync<TableEntity>())
                {
                    var mappedEntity = MapEntity<T>(entity);
                    result[entity.RowKey] = mappedEntity;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
        }

        private T MapEntity<T>(TableEntity entity) where T : class, new()
        {
            var mappedEntity = new T();

            // Example mapping for a Product entity
            if (mappedEntity is IProduct product)
            {
                product.ProductID = int.Parse(entity["ProductID"].ToString());
                product.ProductName = entity["ProductName"].ToString();
                product.SupplierID = int.Parse(entity["SupplierID"].ToString());
                product.CategoryID = int.Parse(entity["CategoryID"].ToString());
                product.QuantityPerUnit = entity["QuantityPerUnit"].ToString();
                product.UnitPrice = (decimal)entity["UnitPrice"];
                product.UnitsInStock = (int)entity["UnitsInStock"];
                product.UnitsOnOrder = (int)entity["UnitsOnOrder"];
                product.ReorderLevel = (int)entity["ReorderLevel"];
                product.Discontinued = (bool)entity["Discontinued"];
                product.ImageUrl = entity["ImageUrl"].ToString();
            }

            else if (mappedEntity is ICategory category)
            {
                category.CategoryID = int.Parse(entity["CategoryID"].ToString());
                category.CategoryName = entity["CategoryName"].ToString();
                category.Description = entity["Description"].ToString();
                category.Picture = entity["Picture"].ToString();
            }

            else if (mappedEntity is ISupplier supplier)
            {
                supplier.SupplierID = int.Parse(entity["SupplierID"].ToString());
                supplier.CompanyName = entity["CompanyName"].ToString();
                supplier.ContactName = entity["ContactName"].ToString();
                supplier.ContactTitle = entity["ContactTitle"].ToString();
                supplier.Address = entity["Address"].ToString();
                supplier.City = entity["City"].ToString();
                supplier.Region = entity["Region"]?.ToString();
                supplier.PostalCode = entity["PostalCode"].ToString();
                supplier.Country = entity["Country"].ToString();
                supplier.Phone = entity["Phone"].ToString();
                supplier.Fax = entity["Fax"]?.ToString();
                supplier.HomePage = entity["HomePage"]?.ToString();
            }

            // Add mappings for other types as needed

            return mappedEntity;
        }

        private async Task<Dictionary<string, OrderTotals>> LoadOrderTotals()
        {
            var totals = new Dictionary<string, OrderTotals>();
            await foreach (var entity in _orderDetailTableClient.QueryAsync<TableEntity>())
            {
                var productId = entity["ProductID"].ToString();
                try
                {
                    if (!totals.ContainsKey(productId))
                    {
                        totals[productId] = new OrderTotals
                        {
                            TotalQuantity = Convert.ToInt32(entity["Quantity"]),
                            TotalRevenue = Convert.ToDecimal(entity["Quantity"]) * Convert.ToDecimal(entity["UnitPrice"]) * (1 - Convert.ToDecimal(entity["Discount"])),
                            TotalDiscount = Convert.ToDecimal(entity["Quantity"]) * Convert.ToDecimal(entity["UnitPrice"]) * Convert.ToDecimal(entity["Discount"])
                        };
                    }
                    else
                    {
                        totals[productId].TotalQuantity += Convert.ToInt32(entity["Quantity"]);
                        totals[productId].TotalRevenue += Convert.ToDecimal(entity["Quantity"]) * Convert.ToDecimal(entity["UnitPrice"]) * (1 - Convert.ToDecimal(entity["Discount"]));
                        totals[productId].TotalDiscount += Convert.ToDecimal(entity["Quantity"]) * Convert.ToDecimal(entity["UnitPrice"]) * Convert.ToDecimal(entity["Discount"]);
                    }
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return totals;
        }

        public async Task<List<IProductEx>> GetAllProductsEx()
        {
            _categories ??= await LoadReferenceData<Category>(_categoryTableClient);
            _suppliers ??= await LoadReferenceData<Supplier>(_supplierTableClient);
            _orderTotals ??= await LoadOrderTotals();

            var result = new List<IProductEx>();

            await foreach (var entity in _productTableClient.QueryAsync<TableEntity>())
            {
                var productEx = GetProductExForEntity(entity);
                result.Add(productEx);
            }

            return result;
        }

        private IProductEx GetProductExForEntity(TableEntity entity)
        {
            try
            {
                var productEx = new ProductEx
                {
                    ETag = entity.ETag.ToString(),
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Timestamp = entity.Timestamp.HasValue ? entity.Timestamp.Value.DateTime : DateTime.MinValue,
                    ProductID = entity.ContainsKey("ProductID") ? int.Parse(entity["ProductID"].ToString()) : 0,
                    ProductName = entity.ContainsKey("ProductName") ? entity["ProductName"].ToString() : string.Empty,
                    SupplierID = entity.ContainsKey("SupplierID") ? int.Parse(entity["SupplierID"].ToString()) : 0,
                    CategoryID = entity.ContainsKey("CategoryID") ? int.Parse(entity["CategoryID"].ToString()) : 0,
                    QuantityPerUnit = entity.ContainsKey("QuantityPerUnit") ? entity["QuantityPerUnit"].ToString() : string.Empty,
                    UnitPrice = entity.ContainsKey("UnitPrice") ? Convert.ToDecimal(entity["UnitPrice"]) : 0,
                    UnitsInStock = entity.ContainsKey("UnitsInStock") ? Convert.ToInt32(entity["UnitsInStock"]) : 0,
                    UnitsOnOrder = entity.ContainsKey("UnitsOnOrder") ? Convert.ToInt32(entity["UnitsOnOrder"]) : 0,
                    ReorderLevel = entity.ContainsKey("ReorderLevel") ? Convert.ToInt32(entity["ReorderLevel"]) : 0,
                    Discontinued = entity.ContainsKey("Discontinued") && Convert.ToBoolean(entity["Discontinued"]),
                    ImageUrl = entity.ContainsKey("ImageUrl") ? entity["ImageUrl"].ToString() : string.Empty
                };

                productEx.CategoryName = _categories[productEx.CategoryID.ToString()].CategoryName;
                productEx.SupplierName = _suppliers[productEx.SupplierID.ToString()].CompanyName;
                productEx.SupplierCity = _suppliers[productEx.SupplierID.ToString()].City;
                productEx.UnitSales = _orderTotals[productEx.ProductID.ToString()].TotalQuantity;
                productEx.InventoryValue = productEx.UnitsInStock * productEx.UnitPrice;
                productEx.Revenue = _orderTotals[productEx.ProductID.ToString()].TotalRevenue;
                productEx.AverageDiscount = Math.Round(productEx.Revenue / _orderTotals[productEx.ProductID.ToString()].TotalDiscount, 1);
                productEx.InventoryStatus = Utils.GetInventoryStatus(productEx);

                return productEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IProductEx> GetProductExAsync(int productId)
        {
            var entity = await _productTableClient.GetEntityAsync<TableEntity>(TableNames.Product, productId.ToString());
            return GetProductExForEntity(entity.Value);
        }

        public async Task UpdateProductAsync(IProductEx updatedProduct)
        {
            var entity = await _productTableClient.GetEntityAsync<TableEntity>(TableNames.Product, updatedProduct.ProductID.ToString());
            if (entity == null)
            {
                throw new Exception("Product not found");
            }
            var mergedEntity = entity.Value;
            mergedEntity["ProductName"] = updatedProduct.ProductName;
            mergedEntity["SupplierID"] = updatedProduct.SupplierID;
            mergedEntity["CategoryID"] = updatedProduct.CategoryID;
            mergedEntity["QuantityPerUnit"] = updatedProduct.QuantityPerUnit;
            mergedEntity["UnitPrice"] = updatedProduct.UnitPrice;
            mergedEntity["UnitsInStock"] = updatedProduct.UnitsInStock;
            mergedEntity["UnitsOnOrder"] = updatedProduct.UnitsOnOrder;
            mergedEntity["ReorderLevel"] = updatedProduct.ReorderLevel;
            mergedEntity["Discontinued"] = updatedProduct.Discontinued;
            mergedEntity["ImageUrl"] = updatedProduct.ImageUrl;

            await _productTableClient.UpdateEntityAsync(mergedEntity, mergedEntity.ETag, TableUpdateMode.Merge);
        }
    }

    internal class OrderTotals
    {
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
    }
}
