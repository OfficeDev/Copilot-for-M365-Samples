from typing import List, Optional
from datetime import datetime
from model.base_model import Consultant, HoursEntry, Assignment
from model.api_model import ApiConsultant, ApiChargeTimeResponse
from services.project_db_service import project_db_service
from services.assignment_db_service import assignment_db_service
from services.consultant_db_service import consultant_db_service
from .utilities import HttpError
import asyncio

AVAILABLE_HOURS_PER_MONTH = 160

class ConsultantApiService:

    async def get_api_consultant_by_id(self, consultant_id: str) -> Optional[ApiConsultant]:
        result = None
        consultant = await consultant_db_service.get_consultant_by_id(consultant_id)
        if consultant:
            assignments = await assignment_db_service.get_assignments()
            result = await self.get_api_consultant_for_base_consultant(consultant, assignments)
        return result

    async def get_api_consultants(self, consultant_name: str, project_name: str, skill: str,
                                  certification: str, role: str, hours_available: str) -> List[ApiConsultant]:

        consultants = await consultant_db_service.get_consultants()
        assignments = await assignment_db_service.get_assignments()

        # Filter on base properties
        if consultant_name:
            consultants = [c for c in consultants if consultant_name.lower() in c.get("name").lower()]
        if skill:
            consultants = [c for c in consultants if any(skill.lower() in s.lower() for s in c.get("skills"))]
        if certification:
            consultants = [c for c in consultants if any(certification.lower() in cert.lower() for cert in c.get("certifications"))]
        if role:
            consultants = [c for c in consultants if role.lower() in [r.lower() for r in c.get("roles")]]

        # Augment the base properties with assignment information
        result = await asyncio.gather(*[self.get_api_consultant_for_base_consultant(c, assignments) for c in consultants])

        # Filter on project name
        if project_name:
            result = [c for c in result if any(
                project_name.lower() in (p.get("projectName").lower() + p.get("clientName").lower()) for p in c.projects
            )]

        # Filter on available hours
        if hours_available:
            hours_available = int(hours_available)
            result = [c for c in result if AVAILABLE_HOURS_PER_MONTH * 2 - c.forecast_this_month - c.forecast_next_month >= hours_available]

        return result

    async def create_api_consultant(self, consultant: Consultant) -> ApiConsultant:
        await consultant_db_service.create_consultant(consultant)
        assignments = await assignment_db_service.get_assignments()

        new_api_consultant = await self.get_api_consultant_for_base_consultant(consultant, assignments)
        return new_api_consultant

    async def get_api_consultant_for_base_consultant(self, consultant: Consultant, assignments: List[Assignment]) -> ApiConsultant:
        result = ApiConsultant(
            id=consultant.get('id'),
            name=consultant.get('name'),
            email=consultant.get('email'),
            phone=consultant.get('phone'),
            consultantPhotoUrl=consultant.get('consultantPhotoUrl'),
            location=consultant.get('location'),
            skills=consultant.get('skills'),
            certifications=consultant.get('certifications'),
            roles=consultant.get('roles'),
            projects=[],  
            forecastThisMonth=0,
            forecastNextMonth= 0,
            deliveredLastMonth= 0,
            deliveredThisMonth= 0
        )

        assignments = [a for a in assignments if a.consultantId == consultant.get('id')]

        for assignment in assignments:
            project = await project_db_service.get_project_by_id(assignment.projectId)
            forecast_hours = self.find_hours(assignment.forecast)
            delivered_hours = self.find_hours(assignment.delivered)
            result.projects.append({
                "projectName": project.name,
                "projectDescription": project.description,
                "projectLocation": project.location,
                "mapUrl": project.mapUrl,
                "clientName": project.clientName,
                "clientContact": project.clientContact,
                "clientEmail": project.clientEmail,
                "role": assignment.role,
                "forecastThisMonth": forecast_hours["thisMonthHours"],
                "forecastNextMonth": forecast_hours["nextMonthHours"],
                "deliveredLastMonth": delivered_hours["lastMonthHours"],
                "deliveredThisMonth": delivered_hours["thisMonthHours"]
            })

            result.forecastThisMonth += forecast_hours["thisMonthHours"]
            result.forecastNextMonth += forecast_hours["nextMonthHours"]
            result.deliveredLastMonth += delivered_hours["lastMonthHours"]
            result.deliveredThisMonth += delivered_hours["thisMonthHours"]

        return result

    def find_hours(self, hours: List[HoursEntry]) -> dict:
        now = datetime.now()
        this_month = now.month
        this_year = now.year

        last_month = 12 if this_month == 1 else this_month - 1
        last_year = this_year - 1 if this_month == 1 else this_year

        next_month = 1 if this_month == 12 else this_month + 1
        next_year = this_year + 1 if this_month == 12 else this_year
        return {
            "lastMonthHours": next((h.get('hours') for h in hours if h.get('month') == last_month and h.get('year') == last_year), 0),
            "thisMonthHours": next((h.get('hours') for h in hours if h.get('month') == this_month and h.get('year') == this_year), 0),
            "nextMonthHours": next((h.get('hours') for h in hours if h.get('month') == next_month and h.get('year') == next_year), 0)
        }

    async def charge_time_to_project(self, project_name: str, consultant_id: str, hours: int) -> ApiChargeTimeResponse:
        from services.project_api_service import project_api_service
        projects = await project_api_service.get_api_projects(project_name, "")
        if not projects:
            raise HttpError(404, f"Project not found: {project_name}")
        if len(projects) > 1:
            raise HttpError(406, f"Multiple projects found with the name: {project_name}")
        
        project = projects[0]

        # Always charge to the current month
        month = datetime.now().month
        year = datetime.now().year

        remaining_forecast = await assignment_db_service.charge_hours_to_project(project.id, consultant_id, month, year, hours)

        if remaining_forecast < 0:
            message = f"Charged {hours} hours to {project.clientName} on project \"{project.name}\". You are {-remaining_forecast} hours over your forecast this month."
        else:
            message = f"Charged {hours} hours to {project.clientName} on project \"{project.name}\". You have {remaining_forecast} hours remaining this month."

        return ApiChargeTimeResponse(
            clientName=project.clientName,
            projectName=project.name,
            remainingForecast=remaining_forecast,
            message=message
        )

# Export instance of ConsultantApiService
consultant_api_service = ConsultantApiService()