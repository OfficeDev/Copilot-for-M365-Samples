import {
  TeamsActivityHandler,
  TurnContext,
  MessagingExtensionQuery,
  MessagingExtensionResponse,
  InvokeResponse
} from "botbuilder";
import productSearchME from "./messageExtensions/productSearchME";
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
      case productSearchME.COMMAND_ID: {
        return productSearchME.handleTeamsMessagingExtensionQuery(context, query);
      }
    }

  }

  // Handle adaptive card action
  public async onInvokeActivity(context: TurnContext): Promise<InvokeResponse> {
    let runEvents = true;
    // console.log (`🎬 Invoke activity received: ${context.activity.name}`);
    try {     
      if(context.activity.name==='adaptiveCard/action'){
        switch (context.activity.value.action.verb) {
          case 'ok': {
            return productSearchME.handleTeamsCardActionUpdateStock(context);
          }
          case 'restock': {
            return productSearchME.handelTeamsCardActionRestock(context);
          }
          case 'cancel': {
            return productSearchME.handelTeamsCardActionCancelRestock(context);
          }
          default:
            runEvents = false;
            return super.onInvokeActivity(context);
        }
      } else {
          runEvents = false;
          return super.onInvokeActivity(context);
      }
    } catch (err) {
      if (err.message === 'NotImplemented') {
        return { status: 501 };
      } else if (err.message === 'BadRequest') {
        return { status: 400 };
      }
      throw err;
    } finally {
      if (runEvents) {
        this.defaultNextEvent(context)();
      }
    }
  }
}

