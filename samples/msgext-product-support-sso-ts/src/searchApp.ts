import {
  TeamsActivityHandler,
  TurnContext,
  MessagingExtensionQuery,
  AdaptiveCardInvokeValue,
  AdaptiveCardInvokeResponse,
  MessagingExtensionResponse,
} from 'botbuilder';
import {HandleMessagingExtensionQuery} from './activityHandler.ts/HandleMessagingExtensionQuery';

import {HandleAdaptiveCardInvoke} from './activityHandler.ts/HandleAdaptiveCardInvoke';
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
}
