import os
import json
import uuid
import argparse
from azure.data.tables import TableServiceClient, TableClient
from config import Settings

def get_flag_url(country):
    country_codes = {
        "australia": "au",
        "brazil": "br",
        "canada": "ca",
        "denmark": "dk",
        "france": "fr",
        "germany": "de",
        "finland": "fi",
        "italy": "it",
        "japan": "jp",
        "netherlands": "nl",
        "norway": "no",
        "singapore": "sg",
        "spain": "es",
        "sweden": "se",
        "uk": "gb",
        "usa": "us",
        "mexico": "mx",
        "argentina": "ar",
        "switzerland": "ch",
        "austria": "at",
        "portugal": "pt",
        "venezuela": "ve",
        "ireland": "ie",
        "belgium": "be",
        "poland": "pl"
    }
    return f"https://flagcdn.com/32x24/{country_codes[country.lower()]}.png"

def get_tables(table_service_client):
    tables = []
    tables_list = table_service_client.list_tables()
    for table in tables_list:
        tables.append(table.name)
    return tables

def main(connection_string, reset):
    table_service_client = TableServiceClient.from_connection_string(connection_string)

    if reset:
        tables = get_tables(table_service_client)
        for table in tables:
            table_client = TableClient.from_connection_string(connection_string, table)
            print(f"Deleting table: {table}")
            table_client.delete_table()
        
        while True:
            print("Waiting for tables to be deleted...")
            tables = get_tables(table_service_client)
            if not tables:
                print("All tables deleted.")
                break

    tables = ["Categories", "Customers", "Employees", "Orders", "OrderDetails", "Products", "Suppliers"]
    row_key_column_names = ["CategoryID", "CustomerID", "EmployeeID", "OrderID", None, "ProductID", "SupplierID"]
    generate_image = [False, True, False, False, False, True, True]
    generate_flag = [False, True, False, False, False, False, True]

    for table, row_key_column_name, gen_image, gen_flag in zip(tables, row_key_column_names, generate_image, generate_flag):
        tables = get_tables(table_service_client)
        if table in tables:
            print(f"Table {table} already exists, skipping...")
            continue

        print(f"Creating table: {table}")
        table_service_client.create_table(table)
        table_client = TableClient.from_connection_string(connection_string, table)

        json_file_path = os.path.join(os.path.dirname(__file__), 'db', f'{table}.json')
        with open(json_file_path, 'r') as json_file:
            entities = json.load(json_file)[table]

        for entity in entities:
            row_key = entity[row_key_column_name] if row_key_column_name else str(uuid.uuid4())
            print(f"Added entity to {table} with key {row_key}")

            if gen_image and "ImageUrl" not in entity:
                entity["ImageUrl"] = f"https://picsum.photos/seed/{row_key}/200/300"
            if gen_flag:
                entity["FlagUrl"] = get_flag_url(entity["Country"])

            entity["PartitionKey"] = table
            entity["RowKey"] = str(row_key)
            try:
                print(f"Adding entity to {table} with key {row_key}: {entity}")
                table_client.create_entity(entity)
                print(f"Entity added to {table} with key {row_key}")
            except Exception as e:
                print(f"Error adding entity to {table} with key {row_key}: {e}")
                print(f"Entity data: {entity}")

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description="Setup Azure Tables.")
    parser.add_argument('--reset', '-r', action='store_true', help="Reset the tables.")
    args = parser.parse_args()
    
    settings = Settings()
    connection_string = settings.AZURE_STORAGE_CONNECTION_STRING
    main(connection_string, args.reset)