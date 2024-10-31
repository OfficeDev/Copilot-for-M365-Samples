import azure.functions as func
import logging
import json
import re
from services.identity_service import identity_service
from services.consultant_api_service import consultant_api_service
from services.utilities import HttpError, clean_up_parameter
import dataclasses

async def me(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("HTTP trigger function 'me' processed a request.")

    res = {
        "status": 200,
        "results": []
    }

    try:
        me = await identity_service.validate_request(req)
        command = req.route_params.get("command", "").lower()
        body = None

        if req.method == "GET":
            if command:
                raise HttpError(400, f"Invalid command: {command}")

            logging.info("➡️ GET /api/me request")
            res["results"] = [dataclasses.asdict(me)]
            logging.info(f"✅ GET /me response status {res['status']}; {len(res['results'])} consultants returned")
            return func.HttpResponse(
                json.dumps(res),
                status_code=res["status"],
                mimetype="application/json"
            )

        elif req.method == "POST":
            if command == "chargetime":
                try:
                    raw_body = req.get_body().decode('utf-8')
                    formatted_body = re.sub(r'([a-zA-Z0-9_]+):', r'"\1":', raw_body)
                    body = json.loads(formatted_body)
                except ValueError as e:
                    logging.error(f"Failed to parse JSON body: {e}")
                    raise HttpError(400, "No body to process this request.")

                if body:
                    project_name = clean_up_parameter("projectName", body.get("projectName"))
                    if not project_name:
                        raise HttpError(400, "Missing project name")

                    hours = body.get("hours")
                    if not hours:
                        raise HttpError(400, "Missing hours")
                    if not isinstance(hours, (int, float)) or hours < 0 or hours > 24:
                        raise HttpError(400, f"Invalid hours: {hours}")

                    logging.info(f"➡️ POST /api/me/chargetime request for project {project_name}, hours {hours}")
                    result = await consultant_api_service.charge_time_to_project(project_name, me.id, hours)

                    res["results"] = {
                        "status": 200,
                        "clientName": result.clientName,
                        "projectName": result.projectName,
                        "remainingForecast": result.remainingForecast,
                        "message": result.message
                    }
                    logging.info(f"✅ POST /api/me/chargetime response status {res['status']}; {result.message}")

                else:
                    raise HttpError(400, "Missing request body")
                return func.HttpResponse(
                    json.dumps(res),
                    status_code=res["status"],
                    mimetype="application/json"
                )
            else:
                raise HttpError(400, f"Invalid command: {command}")

        else:
            raise HttpError(405, f"Method not allowed: {req.method}")

    except HttpError as error:
        status = error.status or 500
        logging.error(f"⛔ Returning error status code {status}: {str(error)}")
        res["status"] = status
        res["results"] = {
            "status": status,
            "message": str(error)
        }
        return func.HttpResponse(
            json.dumps(res),
            status_code=status,
            mimetype="application/json"
        )