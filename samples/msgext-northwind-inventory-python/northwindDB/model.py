from typing import List, Optional, Dict
from datetime import datetime

TABLE_NAME = {
    "CATEGORY": "Categories",
    "CUSTOMER": "Customers",
    "EMPLOYEE": "Employees",
    "ORDER": "Orders",
    "ORDER_DETAIL": "OrderDetails",
    "PRODUCT": "Products",
    "SUPPLIER": "Suppliers"
}

class Row:
    def __init__(self, etag: str, partition_key: str, row_key: str, timestamp: datetime):
        self.etag = etag
        self.partitionKey = partition_key
        self.rowKey = row_key
        self.timestamp = timestamp

class Category(Row):
    def __init__(self, etag: str, partition_key: str, row_key: str, timestamp: datetime, category_id: str, category_name: str, description: str, picture: str):
        super().__init__(etag, partition_key, row_key, timestamp)
        self.CategoryID = category_id
        self.CategoryName = category_name
        self.Description = description
        self.Picture = picture

class Customer(Row):
    def __init__(self, etag: str, partition_key: str, row_key: str, timestamp: datetime, customer_id: str, company_name: str, contact_name: str, contact_title: str, address: str, city: str, region: str, postal_code: str, country: str, phone: str, fax: str, image_url: str, flag_url: str):
        super().__init__(etag, partition_key, row_key, timestamp)
        self.CustomerID = customer_id
        self.CompanyName = company_name
        self.ContactName = contact_name
        self.ContactTitle = contact_title
        self.Address = address
        self.City = city
        self.Region = region
        self.PostalCode = postal_code
        self.Country = country
        self.Phone = phone
        self.Fax = fax
        self.ImageUrl = image_url
        self.FlagUrl = flag_url

class Employee(Row):
    def __init__(self, etag: str, partition_key: str, row_key: str, timestamp: datetime, employee_id: int, last_name: str, first_name: str, title: str, title_of_courtesy: str, birth_date: datetime, hire_date: datetime, address: str, city: str, region: str, postal_code: str, country: str, home_phone: str, extension: str, photo: str, notes: str, reports_to: int, photo_path: str, image_url: str, flag_url: str):
        super().__init__(etag, partition_key, row_key, timestamp)
        self.EmployeeID = employee_id
        self.LastName = last_name
        self.FirstName = first_name
        self.Title = title
        self.TitleOfCourtesy = title_of_courtesy
        self.BirthDate = birth_date
        self.HireDate = hire_date
        self.Address = address
        self.City = city
        self.Region = region
        self.PostalCode = postal_code
        self.Country = country
        self.HomePhone = home_phone
        self.Extension = extension
        self.Photo = photo
        self.Notes = notes
        self.ReportsTo = reports_to
        self.PhotoPath = photo_path
        self.ImageUrl = image_url
        self.FlagUrl = flag_url

class OrderDetail(Row):
    def __init__(self, etag: str, partition_key: str, row_key: str, timestamp: datetime, order_id: int, product_id: str, unit_price: float, quantity: int, discount: float):
        super().__init__(etag, partition_key, row_key, timestamp)
        self.OrderID = order_id
        self.ProductID = product_id
        self.UnitPrice = unit_price
        self.Quantity = quantity
        self.Discount = discount

class Order(Row):
    def __init__(self, etag: str, partition_key: str, row_key: str, timestamp: datetime, order_id: int, customer_id: str, employee_id: int, order_date: str, required_date: Optional[str], shipped_date: Optional[str], order_details: List[OrderDetail], ship_via: str, freight: float, ship_name: str, ship_address: str, ship_city: str, ship_region: Optional[str], ship_postal_code: str, ship_country: str):
        super().__init__(etag, partition_key, row_key, timestamp)
        self.OrderID = order_id
        self.CustomerID = customer_id
        self.EmployeeID = employee_id
        self.OrderDate = order_date
        self.RequiredDate = required_date
        self.ShippedDate = shipped_date
        self.OrderDetails = order_details
        self.ShipVia = ship_via
        self.Freight = freight
        self.ShipName = ship_name
        self.ShipAddress = ship_address
        self.ShipCity = ship_city
        self.ShipRegion = ship_region
        self.ShipPostalCode = ship_postal_code
        self.ShipCountry = ship_country

class Product(Row):
    def __init__(self, etag: str, partition_key: str, row_key: str, timestamp: datetime, product_id: str, product_name: str, supplier_id: str, category_id: str, quantity_per_unit: str, unit_price: float, units_in_stock: int, units_on_order: int, reorder_level: int, discontinued: bool, image_url: str):
        super().__init__(etag, partition_key, row_key, timestamp)
        self.ProductID = product_id
        self.ProductName = product_name
        self.SupplierID = supplier_id
        self.CategoryID = category_id
        self.QuantityPerUnit = quantity_per_unit
        self.UnitPrice = unit_price
        self.UnitsInStock = units_in_stock
        self.UnitsOnOrder = units_on_order
        self.ReorderLevel = reorder_level
        self.Discontinued = discontinued
        self.ImageUrl = image_url

class ProductEx(Product):
    def __init__(self, etag: str, partition_key: str, row_key: str, timestamp: datetime, product_id: str, product_name: str, supplier_id: str, category_id: str, quantity_per_unit: str, unit_price: float, units_in_stock: int, units_on_order: int, reorder_level: int, discontinued: bool, image_url: str, category_name: str, supplier_name: str, supplier_city: str, inventory_status: str, inventory_value: float, unit_sales: int, revenue: float, average_discount: float):
        super().__init__(etag, partition_key, row_key, timestamp, product_id, product_name, supplier_id, category_id, quantity_per_unit, unit_price, units_in_stock, units_on_order, reorder_level, discontinued, image_url)
        self.CategoryName = category_name
        self.SupplierName = supplier_name
        self.SupplierCity = supplier_city
        self.InventoryStatus = inventory_status
        self.InventoryValue = inventory_value
        self.UnitSales = unit_sales
        self.Revenue = revenue
        self.AverageDiscount = average_discount

class Supplier(Row):
    def __init__(self, etag: str, partition_key: str, row_key: str, timestamp: datetime, supplier_id: str, company_name: str, contact_name: str, contact_title: str, address: str, city: str, region: str, postal_code: str, country: str, phone: str, fax: str, home_page: str):
        super().__init__(etag, partition_key, row_key, timestamp)
        self.SupplierID = supplier_id
        self.CompanyName = company_name
        self.ContactName = contact_name
        self.ContactTitle = contact_title
        self.Address = address
        self.City = city
        self.Region = region
        self.PostalCode = postal_code
        self.Country = country
        self.Phone = phone
        self.Fax = fax
        self.HomePage = home_page
