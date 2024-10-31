from dataclasses import dataclass, field
from typing import List

@dataclass
class Location:
    street: str
    city: str
    state: str
    country: str
    postalCode: str
    latitude: float
    longitude: float


@dataclass
class HoursEntry:
    month: int
    year: int
    hours: float


@dataclass
class Project:
    id: str
    name: str
    description: str
    clientName: str
    clientContact: str
    clientEmail: str
    location: Location
    mapUrl: str


@dataclass
class Consultant:
    id: str
    name: str
    email: str
    phone: str
    consultantPhotoUrl: str
    location: Location
    skills: List[str]
    certifications: List[str]
    roles: List[str]


@dataclass
class Assignment:
    id: str  # The assignment ID is "projectid,consultantid"
    projectId: str
    consultantId: str
    role: str
    billable: bool
    rate: float
    forecast: List[HoursEntry] = field(default_factory=list)
    delivered: List[HoursEntry] = field(default_factory=list)
