import azure.functions as func
import logging
import json
import re
from services.project_api_service import project_api_service
from services.identity_service import identity_service
from services.utilities import HttpError, clean_up_parameter

async def projects(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("HTTP trigger function 'projects' processed a request.")

    res = {
        "status": 200,
        "results": []
    }

    try:
        # Will throw an exception if the request is not valid
        user_info = await identity_service.validate_request(req)

        # Extract the 'id' parameter from the route
        project_id = req.route_params.get('id', '').lower()

        if req.method == 'GET':
            project_name = req.params.get('projectName', '').lower()
            consultant_name = req.params.get('consultantName', '').lower()

            logging.info(f"➡️ GET /api/projects: request for projectName={project_name}, consultantName={consultant_name}, id={project_id}")

            project_name = clean_up_parameter('projectName', project_name)
            consultant_name = clean_up_parameter('consultantName', consultant_name)

            if project_id:
                result = await project_api_service.get_api_project_by_id(project_id)
                
                # Convert the ApiProject object to a dictionary
                res['results'] = [result.to_dict()] if hasattr(result, 'to_dict') else [vars(result)]
                
                logging.info(f"✅ GET /api/projects: response status {res['status']}; 1 project returned")
                return func.HttpResponse(
                    json.dumps(res),
                    status_code=res['status'],
                    mimetype="application/json"
                )

            # Fetch projects for the current user if projectName includes 'user_profile'
            if 'user_profile' in project_name:
                result = await project_api_service.get_api_projects("", user_info['name'])
                res['results'] = [result.to_dict()] if hasattr(result, 'to_dict') else [vars(result)]
                logging.info(f"✅ GET /api/projects for current user response status {res['status']}; {len(result)} projects returned")
                return func.HttpResponse(
                    json.dumps(res),
                    status_code=res['status'],
                    mimetype="application/json"
                )

            # Fetch by projectName and consultantName
            result = await project_api_service.get_api_projects(project_name, consultant_name)
            
            # Convert the list of ApiProject objects to dictionaries
            res['results'] = [r.to_dict() if hasattr(r, 'to_dict') else vars(r) for r in result]

            logging.info(f"✅ GET /api/projects: response status {res['status']}; {len(result)} projects returned")
            return func.HttpResponse(
                json.dumps(res),
                status_code=res['status'],
                mimetype="application/json"
            )

        elif req.method == 'POST':
            if project_id == 'assignconsultant':
                try:
                    raw_body = req.get_body().decode('utf-8')
                    formatted_body = re.sub(r'([a-zA-Z0-9_]+):', r'"\1":', raw_body)
                    body = json.loads(formatted_body)
                except ValueError:
                    raise HttpError(400, "No body to process this request.")

                if body:
                    project_name = clean_up_parameter('projectName', body.get('projectName'))
                    if not project_name:
                        raise HttpError(400, "Missing project name")

                    consultant_name = clean_up_parameter('consultantName', body.get('consultantName', '').lower())
                    if not consultant_name:
                        raise HttpError(400, "Missing consultant name")

                    role = clean_up_parameter('Role', body.get('role'))
                    if not role:
                        raise HttpError(400, "Missing role")

                    forecast = body.get('forecast', 0)

                    logging.info(f"➡️ POST /api/projects: assignconsultant request, projectName={project_name}, consultantName={consultant_name}, role={role}, forecast={forecast}")
                    result = await project_api_service.add_consultant_to_project(project_name, consultant_name, role, forecast)

                    res['results'] = {
                        'status': 200,
                        'clientName': result.clientName,
                        'projectName': result.projectName,
                        'consultantName': result.consultantName,
                        'remainingForecast': result.remainingForecast,
                        'message': result.message
                    }

                    logging.info(f"✅ POST /api/projects: response status {res['status']} - {result.message}")
                    return func.HttpResponse(
                        json.dumps(res),
                        status_code=res['status'],
                        mimetype="application/json"
                    )
                else:
                    raise HttpError(400, "Missing request body")
            else:
                raise HttpError(400, f"Invalid command: {project_id}")

        else:
            raise HttpError(405, f"Method not allowed: {req.method}")

    except HttpError as error:
        status = error.status or 500
        logging.error(f"⛔ Returning error status code {status}: {str(error)}")
        res['status'] = status
        res['results'] = {
            'status': status,
            'message': str(error)
        }
        return func.HttpResponse(
            json.dumps(res),
            status_code=status,
            mimetype="application/json"
        )