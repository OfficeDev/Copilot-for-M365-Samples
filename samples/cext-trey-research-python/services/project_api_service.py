from model.base_model import Project, HoursEntry, Assignment
from model.api_model import ApiProject, ApiAddConsultantToProjectResponse
from services.project_db_service import project_db_service
from services.assignment_db_service import assignment_db_service
from services.consultant_db_service import consultant_db_service
from services.consultant_api_service import consultant_api_service
from .utilities import HttpError
from typing import List, Tuple
from datetime import datetime
import logging

class ProjectApiService:

    async def get_api_project_by_id(self, project_id: str) -> ApiProject:
        project = await project_db_service.get_project_by_id(project_id)
        assignments = await assignment_db_service.get_assignments()

        result = await self.get_api_project(project, assignments)
        return result

    async def get_api_projects(self, project_or_client_name: str, consultant_name: str) -> List[ApiProject]:
        projects = await project_db_service.get_projects()
        assignments = await assignment_db_service.get_assignments()

        # Filter on base properties
        if project_or_client_name:
            projects = [p for p in projects if (
                project_or_client_name.lower() in (p.name or '').lower() or 
                project_or_client_name.lower() in (p.clientName or '').lower()
            )]

        result = []

        # Remove duplicates
        projects = list({p.id: p for p in projects}.values())

        # Augment the base properties with assignment information
        for project in projects:
            api_project = await self.get_api_project(project, assignments)
            result.append(api_project)

        # Filter on augmented properties
        if result and consultant_name:
            result = [
                p for p in result if any(
                    consultant_name.lower() in c['consultantName'].lower() for c in p.consultants
                )
            ]

        return result

    async def get_api_project(self, project: Project, assignments: List[Assignment]) -> ApiProject:
        result = ApiProject(
        id=project.id,
        name=project.name,
        description=project.description,
        location=project.location,
        clientName=project.clientName,
        clientContact=project.clientContact,
        clientEmail=project.clientEmail,
        mapUrl=project.mapUrl,
        consultants=[],  
        forecastThisMonth=0,
        forecastNextMonth=0,
        deliveredLastMonth=0,
        deliveredThisMonth=0
        )

        # Filter the assignments related to this project
        filtered_assignments = [a for a in assignments if a.projectId == project.id]

        # Populate consultants and forecast/delivery data
        for assignment in filtered_assignments:
            # Get the consultant associated with the assignment
            consultant = await consultant_db_service.get_consultant_by_id(assignment.consultantId)

            # Calculate the forecast and delivered hours
            forecast_last_month, forecast_this_month, forecast_next_month = self.find_hours(assignment.forecast)
            delivered_last_month, delivered_this_month, _ = self.find_hours(assignment.delivered)

            # Append consultant details to the project
            result.consultants.append({
                'consultantName': consultant.get('name'),
                'consultantLocation': consultant.get('location'),
                'role': assignment.role,
                'forecastThisMonth': forecast_this_month,
                'forecastNextMonth': forecast_next_month,
                'deliveredLastMonth': delivered_last_month,
                'deliveredThisMonth': delivered_this_month,
            })

            # Update the overall project forecasts and deliveries
            result.forecastThisMonth += forecast_this_month
            result.forecastNextMonth += forecast_next_month
            result.deliveredLastMonth += delivered_last_month
            result.deliveredThisMonth += delivered_this_month

        return result

    def find_hours(self, hours: List[HoursEntry]) -> Tuple[int, int, int]:
        now = datetime.now()
        this_month = now.month
        this_year = now.year

        last_month = 12 if this_month == 1 else this_month - 1
        last_year = this_year - 1 if this_month == 1 else this_year

        next_month = 1 if this_month == 12 else this_month + 1
        next_year = this_year + 1 if this_month == 12 else this_year

        last_month_hours = next((h.get('hours') for h in hours if h.get('month') == last_month and h.get('year') == last_year), 0)
        this_month_hours = next((h.get('hours') for h in hours if h.get('month') == this_month and h.get('year') == this_year), 0)
        next_month_hours = next((h.get('hours') for h in hours if h.get('month') == next_month and h.get('year') == next_year), 0)

        return last_month_hours, this_month_hours, next_month_hours

    async def add_consultant_to_project(self, project_name: str, consultant_name: str, role: str, hours: int) -> ApiAddConsultantToProjectResponse:
        projects = await self.get_api_projects(project_name, "")
        consultants = await consultant_api_service.get_api_consultants(consultant_name, "", "", "", "", "")

        if len(projects) == 0:
            raise HttpError(404, f"Project not found: {project_name}")
        elif len(projects) > 1:
            raise HttpError(406, f"Multiple projects found with the name: {project_name}")
        elif len(consultants) == 0:
            raise HttpError(404, f"Consultant not found: {consultant_name}")
        elif len(consultants) > 1:
            raise HttpError(406, f"Multiple consultants found with the name: {consultant_name}")

        project = projects[0]
        consultant = consultants[0]

        # Always charge to the current month
        remaining_forecast = await assignment_db_service.add_consultant_to_project(project.id, consultant.id, role, hours)
        message = f"Added consultant {consultant.name} to {project.clientName} on project '{project.name}' with {remaining_forecast} hours forecast this month."

        return ApiAddConsultantToProjectResponse(
            clientName=project.clientName,
            projectName=project.name,
            consultantName=consultant.name,
            remainingForecast=remaining_forecast,
            message=message
        )


# This would initialize the service in a similar way to the TypeScript `export default`
project_api_service = ProjectApiService()