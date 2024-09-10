using Azure;
using Azure.Data.Tables;
using NorthwindInventory.NorthwindDB;

namespace NorthwindInventory.Models
{
    public static class TableNameConstants
    {
        public const string CATEGORY = "Categories";
        public const string CUSTOMER = "Customers";
        public const string EMPLOYEE = "Employees";
        public const string ORDER = "Orders";
        public const string ORDER_DETAIL = "OrderDetails";
        public const string PRODUCT = "Products";
        public const string SUPPLIER = "Suppliers";
    }

    public static class TableNames
    {
        public const string Product = "Products";
        public const string Category = "Categories";
        public const string Supplier = "Suppliers";
        public const string OrderDetail = "OrderDetails";
    }

    public interface IRow
    {
        string ETag { get; set; }
        string PartitionKey { get; set; }
        string RowKey { get; set; }
        DateTimeOffset? Timestamp { get; set; }
    }

    public interface ICategory : IRow
    {
        int CategoryID { get; set; }
        string CategoryName { get; set; }
        string Description { get; set; }
        string Picture { get; set; }
    }

    public interface ICustomer : IRow
    {
        string CustomerID { get; set; }
        string CompanyName { get; set; }
        string ContactName { get; set; }
        string ContactTitle { get; set; }
        string Address { get; set; }
        string City { get; set; }
        string Region { get; set; }
        string PostalCode { get; set; }
        string Country { get; set; }
        string Phone { get; set; }
        string Fax { get; set; }
        string ImageUrl { get; set; }
        string FlagUrl { get; set; }
    }

    public interface IEmployee : IRow
    {
        int EmployeeID { get; set; }
        string LastName { get; set; }
        string FirstName { get; set; }
        string Title { get; set; }
        string TitleOfCourtesy { get; set; }
        DateTime BirthDate { get; set; }
        DateTime HireDate { get; set; }
        string Address { get; set; }
        string City { get; set; }
        string Region { get; set; }
        string PostalCode { get; set; }
        string Country { get; set; }
        string HomePhone { get; set; }
        string Extension { get; set; }
        string Notes { get; set; }
        int? ReportsTo { get; set; }
    }

    public interface IOrderDetail : IRow
    {
        int OrderID { get; set; }
        int ProductID { get; set; }
        decimal UnitPrice { get; set; }
        int Quantity { get; set; }
        decimal Discount { get; set; }
    }

    public interface IOrder : IRow
    {
        int OrderID { get; set; }
        string CustomerID { get; set; }
        int EmployeeID { get; set; }
        string OrderDate { get; set; }
        string RequiredDate { get; set; }
        string ShippedDate { get; set; }
        List<IOrderDetail> OrderDetails { get; set; }
        string ShipVia { get; set; }
        decimal Freight { get; set; }
        string ShipName { get; set; }
        string ShipAddress { get; set; }
        string ShipCity { get; set; }
        string ShipRegion { get; set; }
        string ShipPostalCode { get; set; }
        string ShipCountry { get; set; }
    }

    public interface IProduct : IRow
    {
        int? ProductID { get; set; }
        string ProductName { get; set; }
        int SupplierID { get; set; }
        int CategoryID { get; set; }
        string QuantityPerUnit { get; set; }
        decimal UnitPrice { get; set; }
        int UnitsInStock { get; set; }
        int UnitsOnOrder { get; set; }
        int ReorderLevel { get; set; }
        bool Discontinued { get; set; }
        string ImageUrl { get; set; }
    }

    public interface IProductEx : IProduct
    {
        string CategoryName { get; set; }
        string SupplierName { get; set; }
        string SupplierCity { get; set; }
        string InventoryStatus { get; set; }
        decimal InventoryValue { get; set; }
        int UnitSales { get; set; }
        decimal Revenue { get; set; }
        decimal AverageDiscount { get; set; }
    }

    public interface ISupplier : IRow
    {
        int? SupplierID { get; set; }
        string CompanyName { get; set; }
        string ContactName { get; set; }
        string ContactTitle { get; set; }
        string Address { get; set; }
        string City { get; set; }
        string Region { get; set; }
        string PostalCode { get; set; }
        string Country { get; set; }
        string Phone { get; set; }
        string Fax { get; set; }
        string HomePage { get; set; }
    }

    public class Category : ICategory
    {
        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } 

        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; }
    }

    public class Supplier : IDbEntity, ISupplier
    {
        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } 

        public int? SupplierID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string HomePage { get; set; }

        ETag ITableEntity.ETag
        {
            get => new ETag(ETag);
            set => ETag = value.ToString();
        }
    }

    public class Customer : ICustomer
    {
        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } 

        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string ImageUrl { get; set; }
        public string FlagUrl { get; set; }
    }

    public class Employee : IEmployee
    {
        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } 

        public int EmployeeID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string TitleOfCourtesy { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string HomePhone { get; set; }
        public string Extension { get; set; }
        public string Notes { get; set; }
        public int? ReportsTo { get; set; }
        public string PhotoPath { get; set; }
        public string ImageUrl { get; set; }
        public string FlagUrl { get; set; }
    }

    public class OrderDetail : IOrderDetail
    {
        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } 

        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
    }

    public class Order : IOrder
    {
        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } 

        public int OrderID { get; set; }
        public string CustomerID { get; set; }
        public int EmployeeID { get; set; }
        public string OrderDate { get; set; }
        public string RequiredDate { get; set; }
        public string ShippedDate { get; set; }
        public List<IOrderDetail> OrderDetails { get; set; }
        public string ShipVia { get; set; }
        public decimal Freight { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipRegion { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipCountry { get; set; }
    }

    public class Product : IProduct
    {
        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } 

        public int ? ProductID { get; set; }
        public string ProductName { get; set; }
        public int SupplierID { get; set; }
        public int CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public int UnitsOnOrder { get; set; }
        public int ReorderLevel { get; set; }
        public bool Discontinued { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ProductEx : Product, IProductEx
    {
        public string CategoryName { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCity { get; set; }
        public string InventoryStatus { get; set; }
        public decimal InventoryValue { get; set; }
        public int UnitSales { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageDiscount { get; set; }
    }

    public class DbProject : IDbProject, IProduct
    {
        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public int? ProductID { get; set; }
        public string ProductName { get; set; }
        public int SupplierID { get; set; }
        public int CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public int UnitsOnOrder { get; set; }
        public int ReorderLevel { get; set; }
        public bool Discontinued { get; set; }
        public string ImageUrl { get; set; }

        ETag ITableEntity.ETag
        {
            get => new ETag(ETag);
            set => ETag = value.ToString();
        }
    }

    public class OrderTotals
    {
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
    }

    public class ProductActionData
    {
        public string action { get; set; }
        public string productName { get; set; }
        public int categoryID { get; set; }
        public int supplierID { get; set; }
        public string qtyPerUnit { get; set; }
        public decimal unitPrice { get; set; }
        public int unitsInStock { get; set; }
        public int unitsOnOrder { get; set; }
        public int reorderLevel { get; set; }
        public bool discontinued { get; set; }
    }
}