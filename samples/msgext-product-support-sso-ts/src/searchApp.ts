import {
  TeamsActivityHandler,
  TurnContext,
  MessagingExtensionQuery,
  AdaptiveCardInvokeValue,
  AdaptiveCardInvokeResponse,
  MessagingExtensionResponse,
  MessagingExtensionAction,
} from "botbuilder";
import { HandleMessagingExtensionQuery } from "./activityHandler/HandleMessagingExtensionQuery";
import { HandleTeamsMessagingExtensionFetchTaskAsync } from "./activityHandler/HandleMessageExtensionFetchTaskAsync";
import { HandleAdaptiveCardInvoke } from "./activityHandler/HandleAdaptiveCardInvoke";

export class SearchApp extends TeamsActivityHandler {
  constructor() {
    super();
  }

  public override async handleTeamsMessagingExtensionQuery(
    context: TurnContext,
    query: MessagingExtensionQuery
  ): Promise<MessagingExtensionResponse> {
    return await HandleMessagingExtensionQuery(context, query);
  }

  public async onAdaptiveCardInvoke(
    context: TurnContext,
    invokeValue: AdaptiveCardInvokeValue
  ): Promise<AdaptiveCardInvokeResponse> {
    return HandleAdaptiveCardInvoke(context, invokeValue);
  }

  protected async handleTeamsMessagingExtensionFetchTask(
    context: TurnContext,
    action: MessagingExtensionAction
  ): Promise<any> {
    return HandleTeamsMessagingExtensionFetchTaskAsync(context, action);
  }
}
