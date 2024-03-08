import config from '../config';
import {
    AdaptiveCardInvokeResponse,
    MessagingExtensionAction,
    TurnContext,
    CloudAdapter
  } from 'botbuilder';
  import {UserTokenClient} from 'botframework-connector';
  import {AuthService} from '../services/AuthService';
import { CreateActionErrorResponse } from '../util';
  
export const HandleTeamsMessagingExtensionFetchTaskAsync = async (
    context: TurnContext, action: MessagingExtensionAction,
): Promise<AdaptiveCardInvokeResponse> => {
  
  try{
    if (action.commandId.toUpperCase() === "SIGNOUT") {
      const adapter = context.adapter as CloudAdapter;
      
      const client:UserTokenClient = context.turnState.get<UserTokenClient>(
        adapter.UserTokenClientKey
      );       
        await client.signOutUser(context.activity.from.id,config.connectionName,context.activity.channelId);
        const credentials = new AuthService(context);
        const token = await credentials.getUserToken();
        if (!token) {
          // There is no token, so the user has not signed in yet.
          return credentials.getSignInAdaptiveCardInvokeResponse();
        }     
}
    
  }catch (error) {
    return CreateActionErrorResponse(500, 0, error.message);
  }  

}
