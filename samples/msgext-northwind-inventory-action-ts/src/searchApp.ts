import {
  TeamsActivityHandler,
  TurnContext,
  MessagingExtensionQuery,
  MessagingExtensionResponse,
  AdaptiveCardInvokeResponse,
  InvokeResponse
} from "botbuilder";
import productSearchCommand from "./messageExtensions/productSearchCommand";
import discountedSearchCommand from "./messageExtensions/discountSearchCommand";
import revenueSearchCommand from "./messageExtensions/revenueSearchCommand";
import actionHandler from "./adaptiveCards/cardHandler";
import { CreateActionErrorResponse } from "./adaptiveCards/utils";
import addProductCommand from "./messageExtensions/addProductCommand";
import addSupplierCommand from "./messageExtensions/addSupplierCommand";
export class SearchApp extends TeamsActivityHandler {
  constructor() {
    super();
  }

  // Handle search message extension
  public async handleTeamsMessagingExtensionQuery(
    context: TurnContext,
    query: MessagingExtensionQuery
  ): Promise<MessagingExtensionResponse> {

    switch (query.commandId) {
      case productSearchCommand.COMMAND_ID: {
        return productSearchCommand.handleTeamsMessagingExtensionQuery(context, query);
      }
      case discountedSearchCommand.COMMAND_ID: {
        return discountedSearchCommand.handleTeamsMessagingExtensionQuery(context, query);
      }
      case revenueSearchCommand.COMMAND_ID: {
        return revenueSearchCommand.handleTeamsMessagingExtensionQuery(context, query);
      }
    }

  }

  // Handle adaptive card actions
  public async onAdaptiveCardInvoke(context: TurnContext): Promise<AdaptiveCardInvokeResponse> {
    try {

      switch (context.activity.value.action.verb) {
        case 'ok': {
          return actionHandler.handleTeamsCardActionUpdateStock(context);
        }
        case 'restock': {
          return actionHandler.handleTeamsCardActionRestock(context);
        }
        case 'cancel': {
          return actionHandler.handleTeamsCardActionCancelRestock(context);
        }
        case 'edit': {
          return actionHandler.handleTeamsCardActionEditProduct(context);
        }
        case 'edit-save': {
          return actionHandler.handleTeamsCardActionSaveProduct(context);
        }
        case 'refresh': {
          return actionHandler.handleTeamsCardActionRefresh(context);
        }          
        default:
          return CreateActionErrorResponse(400, 0, `ActionVerbNotSupported: ${context.activity.value.action.verb} is not a supported action verb.`);

      }

    } catch (err) {
      return CreateActionErrorResponse(500, 0, err.message);
    }
  }

  public async handleTeamsMessagingExtensionFetchTask(context, action): Promise<MessagingExtensionResponse> {
    try {

      switch (action.commandId) {
        case addProductCommand.COMMAND_ID: {
          return await addProductCommand.handleTeamsMessagingExtensionFetchTask(context, action);
        }
        case addSupplierCommand.COMMAND_ID: {
          return await addSupplierCommand.handleTeamsMessagingExtensionFetchTask(context, action);
        }
        default: {
          return null;
        }
      }
    }

    catch (e) {
      console.log(e);
    }
  }
  public async handleTeamsMessagingExtensionSubmitAction(context, action): Promise<MessagingExtensionResponse> {
    try {
      //commandId
      switch (action.commandId) {
        case addProductCommand.COMMAND_ID: {
          return await addProductCommand.handleTeamsMessagingExtensionSubmitAction(context, action);
        }
        case addSupplierCommand.COMMAND_ID: {
          return await addSupplierCommand.handleTeamsMessagingExtensionSubmitAction(context, context.activity.value);
        } 
        default:
          return null;
      }
      //
    }
    catch (e) {
      console.log(e);
    }
  }

  
  

}


