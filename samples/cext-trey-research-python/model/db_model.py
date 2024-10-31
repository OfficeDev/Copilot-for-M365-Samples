from dataclasses import dataclass, field
from datetime import datetime
from model.base_model import Project, Consultant, Assignment

@dataclass
class DbEntity:
    partition_key: str = field(default="")
    row_key: str = field(default="")
    etag: str = field(default="")
    timestamp: datetime = field(default_factory=datetime.utcnow)

@dataclass
class DbProject(DbEntity, Project):
    pass

@dataclass
class DbConsultant(DbEntity, Consultant):
    pass

@dataclass
class DbAssignment(DbEntity, Assignment):
    pass