import os

class Settings:
    APP_ID = os.getenv("APP_ID", "")
    APP_PASSWORD = os.getenv("APP_PASSWORD", "")
    STORAGE_ACCOUNT_CONNECTION_STRING = os.getenv("STORAGE_ACCOUNT_CONNECTION_STRING", "")

    TABLE_NAME = {
    'PRODUCT': 'Products',
    'CATEGORY': 'Categories',
    'SUPPLIER': 'Suppliers',
    'ORDER_DETAIL': 'OrderDetails'
    }