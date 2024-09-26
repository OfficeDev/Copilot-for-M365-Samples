from .db_service import DbService
from model.db_model import DbProject
from model.base_model import Project
import logging

TABLE_NAME = "Project"

class ProjectDbService:
    
    def __init__(self):
        # NOTE: Projects are READ ONLY in this demo app, so we are free to cache them in memory.
        self.db_service = DbService[DbProject](ok_to_cache_locally=True)
    
    async def get_project_by_id(self, id: str) -> Project:
        db_project = await self.db_service.get_entity_by_row_key(TABLE_NAME, id)
        return self.convert_db_project(db_project)
    
    async def get_projects(self) -> list[Project]:
        db_projects = await self.db_service.get_entities(TABLE_NAME)
        return [self.convert_db_project(p) for p in db_projects]

    def convert_db_project(self, db_project: DbProject) -> Project:
        return Project(
            id=db_project.get('id'),
            name=db_project.get('name'),
            description=db_project.get('description'),
            clientName=db_project.get('clientName'),
            clientContact=db_project.get('clientContact'),
            clientEmail=db_project.get('clientEmail'),
            location=db_project.get('location'),
            mapUrl=self.get_map_url(db_project)
        )

    def get_map_url(self, project: Project) -> str:
        company_name_kabob_case = project.get('clientName').lower().replace(" ", "-")
        return f"https://microsoft.github.io/copilot-camp/demo-assets/images/maps/{company_name_kabob_case}.jpg"


# Create an instance of ProjectDbService to use as a singleton
project_db_service = ProjectDbService()