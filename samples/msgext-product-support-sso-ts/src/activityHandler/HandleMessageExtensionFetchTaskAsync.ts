import config from "../config";
import {
  MessagingExtensionAction,
  TurnContext,
  CloudAdapter,
  MessagingExtensionActionResponse,
  TaskModuleTaskInfo,
} from "botbuilder";
import { UserTokenClient } from "botframework-connector";
import { CreateErrorResponseActionResponse } from "../util";

export const HandleTeamsMessagingExtensionFetchTaskAsync = async (
  context: TurnContext,
  action: MessagingExtensionAction
): Promise<MessagingExtensionActionResponse> => {
  try {
    if (action.commandId.toUpperCase() === "SIGNOUT") {
      const adapter = context.adapter as CloudAdapter;
      const cardAttachment = {
        contentType: `application/vnd.microsoft.card.adaptive`,
        content: JSON.parse(`{
                    "type": "AdaptiveCard",
                    "version": "1.0",
                    "body": [
                        {
                            "type": "TextBlock",
                            "text": "You have been signed out!",
                            "wrap": true
                        }
                    ]
                }`),
      };

      const taskInfo: TaskModuleTaskInfo = {
        card: cardAttachment,
        title: "Sign out confirmation",
      };

      const client: UserTokenClient = context.turnState.get<UserTokenClient>(
        adapter.UserTokenClientKey
      );

      await client.signOutUser(
        context.activity.from.id,
        config.connectionName,
        context.activity.channelId
      );

      return {
        task: {
          type: "continue",
          value: taskInfo,
        },
      };
    }
  } catch (error) {
    return CreateErrorResponseActionResponse(error.message);
  }
};
