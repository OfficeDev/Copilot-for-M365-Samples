import os

class Settings:
    APP_ID = os.environ.get("MicrosoftAppId", "<<MICROSOFT-APP-ID>>")
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword", "<<MICROSOFT-APP-PASSWORD>>")
    STORAGE_ACCOUNT_CONNECTION_STRING = os.environ.get("STORAGE_ACCOUNT_CONNECTION_STRING", "<<STORAGE_ACCOUNT_CONNECTION_STRING>>")

    TABLE_NAME = {
    'PRODUCT': 'Products',
    'CATEGORY': 'Categories',
    'SUPPLIER': 'Suppliers',
    'ORDER_DETAIL': 'OrderDetails'
    }