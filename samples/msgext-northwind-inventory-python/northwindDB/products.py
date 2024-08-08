from azure.data.tables import TableClient, UpdateMode
from azure.core.exceptions import ResourceNotFoundError
from typing import List, Dict
from datetime import datetime
from .model import Product, ProductEx, Supplier, Category, OrderDetail
from adaptive_cards.utils import get_inventory_status
from config import Settings

categories: Dict[str, Category] = None
suppliers: Dict[str, Supplier] = None
order_totals: Dict[str, OrderDetail] = None

async def search_products(product_name: str, category_name: str, inventory_status: str, supplier_city: str, stock_level: str) -> List[ProductEx]:
    result = await get_all_products_ex()

    if product_name:
        result = [p for p in result if p['ProductName'].lower().startswith(product_name.lower())]
    if category_name:
        result = [p for p in result if p['CategoryName'].lower().startswith(category_name.lower())]
    if inventory_status:
        result = [p for p in result if is_matching_status(inventory_status, p)]
    if supplier_city:
        result = [p for p in result if p['SupplierCity'].lower().startswith(supplier_city.lower())]
    if stock_level:
        result = [p for p in result if is_in_range(stock_level, p['UnitsInStock'])]

    return result

async def get_discounted_products_by_category(category_name: str) -> List[ProductEx]:
    result = await get_all_products_ex()
    result = [p for p in result if p['AverageDiscount'] > 5]
    if category_name:
        result = [p for p in result if p['CategoryName'].lower().startswith(category_name.lower())]

    return result

async def get_products_by_revenue_range(revenue_range: str) -> List[ProductEx]:
    result = await get_all_products_ex()
    if revenue_range:
        range_value = revenue_range
        if revenue_range.lower().startswith('l'):
            range_value = "0-5000"
        elif revenue_range.lower().startswith('h'):
            range_value = "50000-"
        result = [p for p in result if is_in_range(range_value, p['Revenue'])]

    return result

def is_matching_status(inventory_status_query: str, product: ProductEx) -> bool:
    query = inventory_status_query.lower()
    if query.startswith("out"):
        return product['UnitsInStock'] == 0
    elif query.startswith("low"):
        return product['UnitsInStock'] <= product['ReorderLevel']
    elif query.startswith("on"):
        return product['UnitsOnOrder'] > 0
    else:
        return product['UnitsInStock'] > 0

def is_in_range(range_expression: str, value: int) -> bool:
    result = False
    if '-' not in range_expression:
        val = int(range_expression)
        result = value == val
    elif range_expression.endswith('-'):
        lower_bound = int(range_expression[:-1])
        result = value >= lower_bound
    else:
        bounds = range_expression.split('-')
        lower_bound = int(bounds[0])
        upper_bound = int(bounds[1])
        result = lower_bound <= value <= upper_bound

    return result

async def load_reference_data(table_name: str) -> Dict[str, Dict]:
    table_client = TableClient.from_connection_string(Settings.STORAGE_ACCOUNT_CONNECTION_STRING, table_name)
    entities = table_client.list_entities()
    result = {}
    for entity in entities:
        result[entity.get("RowKey")] = entity  

    return result

async def load_order_totals() -> Dict[str, Dict[str, float]]:
    table_client = TableClient.from_connection_string(Settings.STORAGE_ACCOUNT_CONNECTION_STRING, Settings.TABLE_NAME['ORDER_DETAIL'])
    entities = table_client.list_entities()
    totals = {}
    for entity in entities:
        p = entity.get('ProductID')
        if p not in totals:
            totals[p] = {
                'totalQuantity': float(entity.get('Quantity', 0)),
                'totalRevenue': float(entity.get('Quantity', 0)) * float(entity.get('UnitPrice', 0)) * (1 - float(entity.get('Discount', 0))),
                'totalDiscount': float(entity.get('Quantity', 0)) * float(entity.get('UnitPrice', 0)) * float(entity.get('Discount', 0)),
            }
        else:
            totals[p]['totalQuantity'] += float(entity.get('Quantity', 0))
            totals[p]['totalRevenue'] += float(entity.get('Quantity', 0)) * float(entity.get('UnitPrice', 0)) * (1 - float(entity.get('Discount', 0)))
            totals[p]['totalDiscount'] += float(entity.get('Quantity', 0)) * float(entity.get('UnitPrice', 0)) * float(entity.get('Discount', 0))

    return totals

async def get_all_products_ex() -> List[ProductEx]:
    global categories, suppliers, order_totals
    categories = categories or await load_reference_data(Settings.TABLE_NAME['CATEGORY'])
    suppliers = suppliers or await load_reference_data(Settings.TABLE_NAME['SUPPLIER'])
    order_totals = order_totals or await load_order_totals()
    result = []
    table_client = TableClient.from_connection_string(Settings.STORAGE_ACCOUNT_CONNECTION_STRING, Settings.TABLE_NAME['PRODUCT'])
    entities = table_client.list_entities()

    for entity in entities:
        p = get_product_ex_for_entity(entity)
        result.append(p)
       
    return result

def get_product_ex_for_entity(entity: dict) -> ProductEx:
    p: ProductEx = {
        'etag': entity.metadata.get("etag"),
        'partitionKey': entity.get("PartitionKey"),  
        'rowKey': entity.get("RowKey"),  
        'timestamp': entity.metadata.get("timestamp"), 
        'ProductID': entity.get("ProductID"),
        'ProductName': entity.get("ProductName"),
        'SupplierID': entity.get("SupplierID"),
        'CategoryID': entity.get("CategoryID"),
        'QuantityPerUnit': entity.get("QuantityPerUnit"),
        'UnitPrice': float(entity.get("UnitPrice", 0)),
        'UnitsInStock': float(entity.get("UnitsInStock", 0)),
        'UnitsOnOrder': float(entity.get("UnitsOnOrder", 0)),
        'ReorderLevel': float(entity.get("ReorderLevel", 0)),
        'Discontinued': entity.get("Discontinued"),
        'ImageUrl': entity.get("ImageUrl"),
        'CategoryName': categories.get(str(entity.get("CategoryID")), {}).get('CategoryName', ''),
        'SupplierName': suppliers.get(str(entity.get("SupplierID")), {}).get('CompanyName', ''),
        'SupplierCity': suppliers.get(str(entity.get("SupplierID")), {}).get('City', ''),
        'InventoryStatus': '',
        'InventoryValue': 0,
        'UnitSales': 0,
        'Revenue': 0,
        'AverageDiscount': 0
    }
    p['UnitSales'] = order_totals.get(p['ProductID'], {}).get('totalQuantity', 0)
    p['InventoryValue'] = round(p['UnitsInStock'] * p['UnitPrice'])
    p['Revenue'] = round(order_totals.get(p['ProductID'], {}).get('totalRevenue', 0))
    p['AverageDiscount'] = round(
        p['Revenue'] / order_totals.get(p['ProductID'], {}).get('totalDiscount', 1),  # Avoid division by zero
        1
    )
    p['InventoryStatus'] = get_inventory_status(p)

    return p

async def get_product_ex(product_id: int) -> ProductEx:
    table_client = TableClient.from_connection_string(Settings.STORAGE_ACCOUNT_CONNECTION_STRING, Settings.TABLE_NAME['PRODUCT'])
    entity = table_client.get_entity(partition_key=Settings.TABLE_NAME['PRODUCT'], row_key=str(product_id.strip('{}')))

    return get_product_ex_for_entity(entity)

async def update_product(updated_product: ProductEx) -> None:
    table_client = TableClient.from_connection_string(Settings.STORAGE_ACCOUNT_CONNECTION_STRING, Settings.TABLE_NAME['PRODUCT'])
    try:
        product = table_client.get_entity(partition_key=Settings.TABLE_NAME['PRODUCT'], row_key=str(updated_product['ProductID']))
        temp_product = product
        temp_product["UnitsInStock"] = updated_product["UnitsInStock"]
        temp_product["UnitsOnOrder"] = updated_product["UnitsOnOrder"]
        table_client.update_entity(
            mode=UpdateMode.MERGE, 
            entity={**product, **temp_product}
        )
    except ResourceNotFoundError:
        raise Exception("Product not found")