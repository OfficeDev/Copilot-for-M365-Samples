from dataclasses import dataclass, field
from typing import List
from model.base_model import Project, Consultant, Location

# GET requests for /projects

@dataclass
class ApiProjectAssignment:
    consultantName: str
    consultantLocation: Location
    role: str
    forecastThisMonth: float
    forecastNextMonth: float
    deliveredLastMonth: float
    deliveredThisMonth: float


@dataclass
class ApiProject(Project):
    consultants: List[ApiProjectAssignment] = field(default_factory=list)
    forecastThisMonth: float = 0.0
    forecastNextMonth: float = 0.0
    deliveredLastMonth: float = 0.0
    deliveredThisMonth: float = 0.0


# GET requests for /me and /consultants

@dataclass
class ApiConsultantAssignment:
    projectName: str
    projectDescription: str
    projectLocation: Location
    clientName: str
    clientContact: str
    clientEmail: str
    role: str
    forecastThisMonth: float
    forecastNextMonth: float
    deliveredLastMonth: float
    deliveredThisMonth: float


@dataclass
class ApiConsultant(Consultant):
    projects: List[ApiConsultantAssignment] = field(default_factory=list)
    forecastThisMonth: float = 0.0
    forecastNextMonth: float = 0.0
    deliveredLastMonth: float = 0.0
    deliveredThisMonth: float = 0.0


# POST request to /api/me/chargeTime

@dataclass
class ApiChargeTimeRequest:
    projectName: str
    hours: float


@dataclass
class ApiChargeTimeResponse:
    clientName: str
    projectName: str
    remainingForecast: float
    message: str


# POST request to /api/projects/assignConsultant

@dataclass
class ApiAddConsultantToProjectRequest:
    projectName: str
    consultantName: str
    role: str
    hours: float


@dataclass
class ApiAddConsultantToProjectResponse:
    clientName: str
    projectName: str
    consultantName: str
    remainingForecast: float
    message: str


@dataclass
class ErrorResult:
    status: int
    message: str
