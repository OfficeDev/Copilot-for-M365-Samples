using NorthwindInventory.Models; 

namespace NorthwindInventory.NorthwindDB
{
    public static class SupplierService
    {
        private static readonly string TableNameSupplier = "Suppliers";

        public static async Task<string> CreateSupplierAsync(Supplier supplier, IConfiguration configuration)
        {
            var dbService = new DbService<Supplier>(false, configuration);

            var nextId = await dbService.GetNextIdAsync(TableNameSupplier);

            var newSupplier = new Supplier
            {
                ETag = "",
                PartitionKey = TableNameSupplier,
                RowKey = nextId.ToString(),
                Timestamp = DateTimeOffset.UtcNow,
                SupplierID = nextId,
                CompanyName = supplier.CompanyName,
                ContactName = supplier.ContactName,
                ContactTitle = supplier.ContactTitle,
                Address = supplier.Address,
                City = supplier.City,
                Region = supplier.Region,
                PostalCode = supplier.PostalCode,
                Country = supplier.Country,
                Phone = supplier.Phone,
                Fax = supplier.Fax,
                HomePage = supplier.HomePage
            };

            await dbService.CreateEntityAsync(TableNameSupplier, newSupplier.RowKey, newSupplier);

            return newSupplier.SupplierID.ToString();
        }
    }
}