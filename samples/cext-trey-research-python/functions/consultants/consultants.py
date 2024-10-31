import azure.functions as func
import logging
from services.consultant_api_service import consultant_api_service
from services.identity_service import identity_service
from services.utilities import clean_up_parameter
import json
import dataclasses

async def consultants(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("HTTP trigger function consultants processed a request.")

    # Initialize the response JSON
    response_json = {
        "results": []
    }

    try:
        # Validate the request using IdentityService
        await identity_service.validate_request(req)
        # Get the input parameters from the request
        consultant_name = req.params.get('consultantName', '').lower()
        project_name = req.params.get('projectName', '').lower()
        skill = req.params.get('skill', '').lower()
        certification = req.params.get('certification', '').lower()
        role = req.params.get('role', '').lower()
        hours_available = req.params.get('hoursAvailable', '').lower()
        logging.info("Checking consultant_id from route parameters.")
        consultant_id = req.route_params.get('id')

        if consultant_id:
            consultant_id = consultant_id.lower()
            logging.info(f"➡️ GET /api/consultants/{consultant_id}: request for consultant {consultant_id}")
            result = await consultant_api_service.get_api_consultant_by_id(consultant_id)
            response_json['results'] = [vars(result)]
            logging.info(f"   ✅ GET /api/consultants/{consultant_id}: response status 1 consultant returned")
            return func.HttpResponse(
                body=json.dumps(response_json),
                mimetype="application/json",
                status_code=200
            )

        logging.info(f"➡️ GET /api/consultants: request for consultantName={consultant_name}, "
                     f"projectName={project_name}, skill={skill}, certification={certification}, "
                     f"role={role}, hoursAvailable={hours_available}")

        # Clean up the parameters
        consultant_name = clean_up_parameter("consultantName", consultant_name)
        project_name = clean_up_parameter("projectName", project_name)
        skill = clean_up_parameter("skill", skill)
        certification = clean_up_parameter("certification", certification)
        role = clean_up_parameter("role", role)
        hours_available = clean_up_parameter("hoursAvailable", hours_available)

        # Get consultants based on the filters
        result = await consultant_api_service.get_api_consultants(
            consultant_name, project_name, skill, certification, role, hours_available
        )
        response_json['results'] = [dataclasses.asdict(ApiConsultant) for ApiConsultant in result]
        logging.info(f"   ✅ GET /api/consultants: response status 200; {len(result)} consultants returned")
        return func.HttpResponse(
            body=json.dumps(response_json),
            mimetype="application/json",
            status_code=200
        )

    except Exception as e:
        logging.error(f"   ⛔ Returning error status code 500: {str(e)}")
        response_json['results'] = {
            "status": 500,
            "message": str(e)
        }
        return func.HttpResponse(
            body=json.dumps(response_json),
            mimetype="application/json",
            status_code=500
        )