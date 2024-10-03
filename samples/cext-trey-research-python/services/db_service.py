import os
import json
from azure.data.tables import TableClient, UpdateMode
from azure.core.exceptions import ResourceExistsError
from .utilities import HttpError
from typing import TypeVar, Generic, List, Any
import asyncio, logging, dataclasses


# Define a generic type for DbEntityType
DbEntityType = TypeVar('DbEntityType')

class DbService(Generic[DbEntityType]):
    
    def __init__(self, ok_to_cache_locally: bool):
        self.storage_account_connection_string = os.getenv('STORAGE_ACCOUNT_CONNECTION_STRING')
        if not self.storage_account_connection_string:
            raise ValueError("STORAGE_ACCOUNT_CONNECTION_STRING is not set")
        
        self.ok_to_cache_locally = ok_to_cache_locally
        self.entity_cache: List[DbEntityType] = []
    
    async def get_entity_by_row_key(self, table_name: str, row_key: str) -> DbEntityType:
        if not self.ok_to_cache_locally:
            entities = await self.get_entities(table_name)
            filtered_entities = [e for e in entities if e['RowKey'] == row_key]
            return self.expand_property_values(filtered_entities[0])
            
        else:
            entities = await self.get_entities(table_name)
            filtered_entities = [e for e in entities if e['RowKey'] == row_key]
            if not filtered_entities:
                raise HttpError(404, f"Entity {row_key} not found")
            return filtered_entities[0]
    
    async def get_entities(self, table_name: str) -> List[DbEntityType]:
        if not self.ok_to_cache_locally or not self.entity_cache:
            table_client = TableClient.from_connection_string(self.storage_account_connection_string, table_name)
            entities = table_client.list_entities()
            self.entity_cache = []
            for entity in entities:
                if not any(e['RowKey'] == entity['RowKey'] for e in self.entity_cache):
                    expanded_entity = self.expand_property_values(entity)
                    self.entity_cache.append(expanded_entity)
        return self.entity_cache

    async def create_entity(self, table_name: str, row_key: str, new_entity: DbEntityType) -> None:
        self.entity_cache = []
        logging.info(f"line 49 {new_entity}")
        new_entity_dict = dataclasses.asdict(new_entity)
        if isinstance(new_entity_dict.get("forecast"), list):
            new_entity_dict["forecast"] = json.dumps(new_entity_dict["forecast"])

        if isinstance(new_entity_dict.get("delivered"), list):
            new_entity_dict["delivered"] = json.dumps(new_entity_dict["delivered"])

        table_client = TableClient.from_connection_string(self.storage_account_connection_string, table_name)
        try:
            table_client.create_entity({
                "PartitionKey": table_name,
                "RowKey": row_key,
                **new_entity_dict
            })
        except ResourceExistsError:
            raise HttpError(409, f"Entity with RowKey {row_key} already exists")

    async def update_entity(self, table_name: str, updated_entity: DbEntityType) -> None:
        self.entity_cache = []
        compressed_entity = self.compress_property_values(updated_entity)
        table_client = TableClient.from_connection_string(self.storage_account_connection_string, table_name)

        # Perform synchronous update call within an asynchronous function
        loop = asyncio.get_running_loop()
        await loop.run_in_executor(None, table_client.update_entity, compressed_entity, UpdateMode.REPLACE)

    def expand_property_values(self, entity: Any) -> DbEntityType:
        expanded_entity = {}
        for key, value in entity.items():
            expanded_entity[key] = self.expand_property_value(value)
        return expanded_entity

    def expand_property_value(self, value: Any) -> Any:
        if isinstance(value, str) and (value.startswith('{') or value.startswith('[')):
            try:
                return json.loads(value)
            except json.JSONDecodeError:
                return value
        else:
            return value

    def compress_property_values(self, entity: DbEntityType) -> DbEntityType:
        compressed_entity = {}
        for key, value in entity.items():
            compressed_entity[key] = self.compress_property_value(value)
        return compressed_entity

    def compress_property_value(self, value: Any) -> Any:
        if isinstance(value, (dict, list)):
            return json.dumps(value)
        else:
            return value