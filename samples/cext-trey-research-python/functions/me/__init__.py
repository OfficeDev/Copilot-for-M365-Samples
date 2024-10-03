import sys
import os
import azure.functions as func
# Add the parent directory to the system path to access the `services` module
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '../../')))

from .me import me

async def main(req: func.HttpRequest) -> func.HttpResponse:
    return await me(req)