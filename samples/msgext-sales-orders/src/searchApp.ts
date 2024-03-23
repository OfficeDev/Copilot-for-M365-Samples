import {
  AdaptiveCardInvokeResponse,
  MessagingExtensionQuery,
  MessagingExtensionResponse,
  TeamsActivityHandler,
  TurnContext,
} from 'botbuilder';

import { ECommands } from './constants/ECommands';
import {
  handleTeamsMessagingExtensionSearch,
} from './messageExtensions/searchQuery';
import {
  CreateActionErrorResponse,
  handleTeamsCardActionUpdateStatus,
} from './tools/utils';

export class SearchApp extends TeamsActivityHandler {
  constructor() {
    super();
  }
  // Search.
  public async handleTeamsMessagingExtensionQuery(
    context: TurnContext,
    query: MessagingExtensionQuery
  ): Promise<MessagingExtensionResponse> {
    switch (query.commandId) {
      case ECommands.searchOrders: {
        return handleTeamsMessagingExtensionSearch(context, query);
      }
    }
  }

  // Handle adaptive card actions
  public async onAdaptiveCardInvoke(context: TurnContext): Promise<AdaptiveCardInvokeResponse> {
    try {
      switch (context.activity.value.action.verb) {
        case "update": {
          return handleTeamsCardActionUpdateStatus(context);
        }
        default:
          return CreateActionErrorResponse(
            400,
            0,
            `ActionVerbNotSupported: ${context.activity.value.action.verb} is not a supported action verb.`
          );
      }
    } catch (err) {
      return CreateActionErrorResponse(500, 0, err.message);
    }
  }
}
