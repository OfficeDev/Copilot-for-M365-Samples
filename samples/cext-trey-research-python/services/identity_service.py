from azure.functions import HttpRequest
from .utilities import HttpError
from model.base_model import Consultant
from model.api_model import ApiConsultant
from .consultant_api_service import consultant_api_service
import logging

# This is a DEMO ONLY identity solution.
class IdentityService:
    def __init__(self):
        self.request_number = 1  # Number the requests for logging purposes

    async def validate_request(self, req: HttpRequest) -> ApiConsultant:
        # Default user used for unauthenticated testing
        user_id = "1"
        user_name = "Avery Howard"
        user_email = "avery@treyresearch.com"

        # ** INSERT REQUEST VALIDATION HERE (see Lab E6) **
        # You would add actual request validation logic here in a real-world scenario.

        # Get the consultant record for this user; create one if necessary
        consultant = None
        try:
            consultant = await consultant_api_service.get_api_consultant_by_id(user_id)
        except HttpError as ex:
            if ex.status != 404:
                raise
            # Consultant was not found, so we'll create one below
            consultant = None

        if not consultant:
            consultant = await self.create_consultant_for_user(user_id, user_name, user_email)

        return consultant

    async def create_consultant_for_user(self, user_id: str, user_name: str, user_email: str) -> ApiConsultant:
        # Create a new consultant record for this user with default values
        consultant = Consultant(
            id=user_id,
            name=user_name,
            email=user_email,
            phone="1-555-123-4567",
            consultant_photo_url="https://microsoft.github.io/copilot-camp/demo-assets/images/consultants/Unknown.jpg",
            location={
                "street": "One Memorial Drive",
                "city": "Cambridge",
                "state": "MA",
                "country": "USA",
                "postal_code": "02142",
                "latitude": 42.361366,
                "longitude": -71.081257
            },
            skills=["JavaScript", "TypeScript"],
            certifications=["Azure Development"],
            roles=["Architect", "Project Lead"]
        )
        result = await consultant_api_service.create_api_consultant(consultant)
        return result

# Instantiate and use the Identity service
identity_service = IdentityService()