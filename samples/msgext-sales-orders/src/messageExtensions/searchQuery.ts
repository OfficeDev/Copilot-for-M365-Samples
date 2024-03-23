import {
  CardFactory,
  MessagingExtensionQuery,
  MessagingExtensionResponse,
  TurnContext,
} from 'botbuilder';

import orderDetails from '../adaptiveCards/orderDetails.json';
import { EOrderActions } from '../constants/EOrderStatus';
import { IOrder } from '../models/IOrder';
import { getOrders } from '../services/IBMDB2Service';
import {
  cleanupParameter,
  getCardTemplate,
  getParameterByName,
  getPreviewCard,
} from '../tools/utils';

let queryCount = 0;
let isRunning = false;
export const handleTeamsMessagingExtensionSearch = async (
  context: TurnContext,
  query: MessagingExtensionQuery
): Promise<MessagingExtensionResponse> => {
  let searchOrdersQuery, clientQuery, orderStatusQuery, orderDateQuery, orderAmountQuery;

  // For now we have the ability to pass parameters comma separated for testing until the UI supports it.
  // So try to unpack the parameters but when issued from Copilot or the multi-param UI they will come
  // in the parameters array.
  if (query.parameters.length === 1 && query.parameters[0]?.name === "searchOrdersQuery") {
    [searchOrdersQuery, clientQuery, orderStatusQuery, orderDateQuery, orderAmountQuery] = getParameterByName(
      query.parameters,
      "searchOrdersQuery"
    ).split(",");
  } else {
    searchOrdersQuery = cleanupParameter(getParameterByName(query.parameters, "searchOrdersQuery"));
    clientQuery = cleanupParameter(getParameterByName(query.parameters, "clientQuery"));
    orderStatusQuery = cleanupParameter(getParameterByName(query.parameters, "orderStatusQuery"));
    orderDateQuery = cleanupParameter(getParameterByName(query.parameters, "orderDateQuery"));
    orderAmountQuery = cleanupParameter(getParameterByName(query.parameters, "orderAmountQuery"));
  }

   
   const products = await getOrders(searchOrdersQuery, clientQuery, orderStatusQuery, orderDateQuery, orderAmountQuery);
   
   const attachments = [];
   products.forEach((order:IOrder) => {
     const card = getCardTemplate(orderDetails,order,  EOrderActions.LIST)
     const preview = getPreviewCard(order);
     const attachment = { ...CardFactory.adaptiveCard(card), preview };
     attachments.push(attachment);
   });

          return {
            composeExtension: {
              type: "result",
              attachmentLayout: "list",
              attachments: attachments,
            },
          };  
};
