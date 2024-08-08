import sys
import traceback
from datetime import datetime
from db_setup import azure_table_setup
from http import HTTPStatus

from aiohttp import web
from aiohttp.web import Request, Response, json_response

from botbuilder.core import (
    BotFrameworkAdapterSettings,
    TurnContext,
    BotFrameworkAdapter,
)
from botbuilder.core.integration import aiohttp_error_middleware

from botbuilder.schema import Activity, ActivityTypes
from bot import SearchBot
from config import Settings

CONFIG = Settings()

# Create adapter.
# See https://aka.ms/about-bot-adapter to learn more about how bots work.
SETTINGS = BotFrameworkAdapterSettings(CONFIG.APP_ID, CONFIG.APP_PASSWORD)
ADAPTER = BotFrameworkAdapter(SETTINGS)

# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    # This check writes out errors to console log vs. app insights.
    # NOTE: In a production environment, consider logging this to Azure Application Insights.
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Send a message to the user
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity(
        "To continue to run this bot, please fix the bot source code."
    )
    # Send a trace activity if we're talking to the Bot Framework Emulator
    if context.activity.channel_id == "emulator":
        # Create a trace activity that contains the error object
        trace_activity = Activity(
            label="TurnError",
            name="on_turn_error Trace",
            timestamp=datetime.utcnow(),
            type=ActivityTypes.trace,
            value=f"{error}",
            value_type="https://www.botframework.com/schemas/error",
        )

        # Send a trace activity, which will be displayed in Bot Framework Emulator
        await context.send_activity(trace_activity)

ADAPTER.on_turn_error = on_error

# Create the Bot
BOT = SearchBot()

# Setup Azure Tables
def setup_tables():
    connection_string = CONFIG.STORAGE_ACCOUNT_CONNECTION_STRING
    azure_table_setup.main(connection_string, reset=False)  # Adjust `reset` as needed

setup_tables()

# Listen for incoming requests on /api/messages
async def messages(req: Request) -> Response:
    # Main bot message handler.
    if "application/json" in req.headers.get("Content-Type", ""):
        body = await req.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    try:
        activity = Activity().deserialize(body)
    except Exception as e:
        print(f"Error deserializing activity: {e}", file=sys.stderr)
        return Response(status=HTTPStatus.BAD_REQUEST)

    auth_header = req.headers.get("Authorization", "")

    try:
        response = await ADAPTER.process_activity(activity, auth_header, BOT.on_turn)
        if response:
            return json_response(data=response.body, status=response.status)
    except Exception as e:
        print(f"Error processing activity: {e}", file=sys.stderr)
        return Response(status=HTTPStatus.INTERNAL_SERVER_ERROR)

    return Response(status=HTTPStatus.OK)

APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=3978)
    except Exception as error:
        print(f"Failed to start application: {error}", file=sys.stderr)
        raise error