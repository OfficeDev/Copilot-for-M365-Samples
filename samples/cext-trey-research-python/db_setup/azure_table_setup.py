
import os
import sys
import json
import time
import uuid
from azure.data.tables import TableServiceClient, TableClient
from pathlib import Path

TABLE_NAMES = ["Project", "Consultant", "Assignment"]

def get_tables(table_service_client):
    tables = []
    table_iter = table_service_client.list_tables()
    for table in table_iter:
        tables.append(table.name)
    return tables

def delete_tables(table_service_client, connection_string):
    tables = get_tables(table_service_client)
    for table in tables:
        table_client = TableClient.from_connection_string(conn_str=connection_string, table_name=table)
        print(f"Deleting table: {table}")
        table_client.delete_table()
    
    while True:
        print("Waiting for tables to be deleted...")
        tables = get_tables(table_service_client)
        if not tables:
            print("All tables deleted.")
            break
        time.sleep(1)

def create_and_populate_tables(table_service_client, connection_string):
    for table_name in TABLE_NAMES:
        tables = get_tables(table_service_client)
        if table_name in tables:
            print(f"Table {table_name} already exists, skipping...")
            continue

        print(f"Creating table: {table_name}")
        table_created = False
        while not table_created:
            try:
                table_service_client.create_table(table_name=table_name)
                table_created = True
            except Exception as e:
                if '409' in str(e):
                    print('Table is marked for deletion, retrying in 5 seconds...')
                    time.sleep(5)
                else:
                    raise e

        table_client = TableClient.from_connection_string(conn_str=connection_string, table_name=table_name)
        json_file_path = Path(__file__).resolve().parent / "db" / f"{table_name}.json"
        with open(json_file_path, "r", encoding="utf8") as file:
            entities = json.load(file)

        for entity in entities["rows"]:
            row_key = str(entity.get("id", uuid.uuid4()))
            # Convert any nested objects to JSON strings
            for key, value in entity.items():
                if isinstance(value, (dict, list)):
                    entity[key] = json.dumps(value)
            
            table_client.create_entity({
                'PartitionKey': table_name,
                'RowKey': row_key,
                **entity
            })
            print(f"Added entity to {table_name} with key {row_key}")

def main(connection_string="UseDevelopmentStorage=true", reset=False):
    table_service_client = TableServiceClient.from_connection_string(conn_str=connection_string)

    if reset:
        delete_tables(table_service_client, connection_string)

    create_and_populate_tables(table_service_client, connection_string)

if __name__ == "__main__":
    main()