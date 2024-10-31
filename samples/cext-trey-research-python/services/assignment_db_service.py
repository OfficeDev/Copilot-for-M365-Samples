from .db_service import DbService
from model.db_model import DbAssignment
from model.base_model import Assignment
from .utilities import HttpError
from datetime import datetime
import logging
TABLE_NAME = "Assignment"

class AssignmentDbService:

    # NOTE: Assignments are READ-WRITE so disable local caching
    def __init__(self):
        self.db_service = DbService[DbAssignment](ok_to_cache_locally=False)

    async def get_assignments(self) -> list[Assignment]:
        assignments = await self.db_service.get_entities(TABLE_NAME)
        result = [self.convert_db_assignment(e) for e in assignments]
        return result

    async def charge_hours_to_project(self, project_id: str, consultant_id: str, month: int, year: int, hours: int) -> int:
        try:
            logging.info(f"tablename : {TABLE_NAME},project id : {project_id}, consultant_id :{consultant_id} ")
            db_assignment = await self.db_service.get_entity_by_row_key(TABLE_NAME, f"{project_id},{consultant_id}")
            if not db_assignment:
                raise HttpError(404, "Assignment not found")

            # Add the hours delivered
            if not db_assignment.get("delivered"):
                db_assignment["delivered"] = [{"month": month, "year": year, "hours": hours}]
            else:
                assignment_record = next((d for d in db_assignment["delivered"] if d["month"] == month and d["year"] == year), None)
                if assignment_record:
                    assignment_record["hours"] += hours
                else:
                    db_assignment["delivered"].append({"month": month, "year": year, "hours": hours})

            db_assignment["delivered"].sort(key=lambda d: (d["year"], d["month"]))

            # Subtract the hours from the forecast
            remaining_forecast = -hours
            if not db_assignment.get("forecast"):
                db_assignment["forecast"] = [{"month": month, "year": year, "hours": -hours}]
            else:
                forecast_record = next((d for d in db_assignment["forecast"] if d["month"] == month and d["year"] == year), None)
                if forecast_record:
                    forecast_record["hours"] -= hours
                    remaining_forecast = forecast_record["hours"]
                else:
                    db_assignment["forecast"].append({"month": month, "year": year, "hours": -hours})

            db_assignment["forecast"].sort(key=lambda d: (d["year"], d["month"]))

            await self.db_service.update_entity(TABLE_NAME, db_assignment)
            return remaining_forecast

        except Exception as e:
            raise HttpError(404, f"Assignment not found : {e}")


    async def add_consultant_to_project(self, project_id: str, consultant_id: str, role: str, hours: int) -> int:
        month = datetime.utcnow().month
        year = datetime.utcnow().year

        db_assignment = None
        try:
            db_assignment = await self.db_service.get_entity_by_row_key(TABLE_NAME, f"{project_id},{consultant_id}")
        except Exception:
            pass

        if db_assignment:
            raise HttpError(403, "Assignment already exists")

        try:
            new_assignment = DbAssignment(
                partition_key=TABLE_NAME,
                row_key=f"{project_id},{consultant_id}",
                timestamp=datetime.utcnow(),
                id=f"{project_id},{consultant_id}",
                projectId=project_id,
                consultantId=consultant_id,
                role=role,
                billable=True,
                rate=100,
                forecast=[{"month": month, "year": year, "hours": hours}],
                delivered=[],
                etag=""
            )

            await self.db_service.create_entity(TABLE_NAME, new_assignment.row_key, new_assignment)
            return hours

        except Exception as e:
            raise HttpError(500, f"Unable to add assignment :{e}")


    def convert_db_assignment(self, db_assignment: DbAssignment) -> Assignment:
        result = Assignment(
            id=db_assignment.get('id'),
            projectId=db_assignment.get('projectId'),
            consultantId=db_assignment.get('consultantId'),
            role=db_assignment.get('role'),
            billable=db_assignment.get('billable'),
            rate=db_assignment.get('rate'),
            forecast=db_assignment.get('forecast'),
            delivered=db_assignment.get('delivered')
        )
        return result

# Instantiate and use the AssignmentDbService
assignment_db_service = AssignmentDbService()