from .db_service import DbService
from model.db_model import DbConsultant
from model.base_model import Consultant
from datetime import datetime

TABLE_NAME = "Consultant"

class ConsultantDbService:
    # NOTE: Consultants are READ ONLY in this demo app, so we are free to cache them in memory.
    def __init__(self):
        self.db_service = DbService[DbConsultant](ok_to_cache_locally=True)

    async def get_consultant_by_id(self, consultant_id: str) -> Consultant:
        consultant = await self.db_service.get_entity_by_row_key(TABLE_NAME, consultant_id)
        return consultant

    async def get_consultants(self) -> list[Consultant]:
        consultants = await self.db_service.get_entities(TABLE_NAME)
        return consultants

    async def create_consultant(self, consultant: Consultant) -> Consultant:
        new_db_consultant = DbConsultant(
            **consultant.__dict__,
            etag="",
            partition_key=TABLE_NAME,
            row_key=consultant.id,
            timestamp=datetime.utcnow()
        )
        await self.db_service.create_entity(TABLE_NAME, new_db_consultant.row_key, new_db_consultant)
        print(f"Added new consultant {new_db_consultant.name} ({new_db_consultant.row_key}) to the Consultant table")
        return None

# Instantiate and use the ConsultantDbService
consultant_db_service = ConsultantDbService()