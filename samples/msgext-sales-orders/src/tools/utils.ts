import * as ACData from 'adaptivecards-templating';
import {
  AdaptiveCardInvokeResponse,
  CardFactory,
  InvokeResponse,
  MessagingExtensionParameter,
  TurnContext,
} from 'botbuilder';

import orderDetails from '../adaptiveCards/orderDetails.json';
import { DEFAULT_IMAGE } from '../constants/constants';
import { EOrderActions } from '../constants/EOrderStatus';
import { IOrder } from '../models/IOrder';
import {
  getOrder,
  updateOrderStatus,
} from '../services/IBMDB2Service';

export const getParameterByName = (parameters: MessagingExtensionParameter[], name: string) => {
  const param = parameters.find((p) => p.name === name);
  return param?.value ?? "";
};

export const cleanupParameter = (value: string): string => {
  if (!value) {
    return "";
  } else {
    let result = value.trim();
    result = result.split(",")[0]; // Remove extra data
    result = result.replace("*", ""); // Remove wildcard characters from Copilot
    return result;
  }
};

export const mapValues = (order: IOrder, action:EOrderActions) => {
  return {
    $root: {
      orderUpdatedStaus: action === EOrderActions.UPDATE ? "true" : "false",
      image: DEFAULT_IMAGE,
      orderId: order.ORD_NBR,
      orderDate: order.ORD_DATE,
      orderStatus: order.ORD_STAT,
      orderTotal: order.ORD_TOT_COST,
      orderPaymentMethod: order.ORD_PYMT_METH,
      orderShipDate: order.ORD_SHIP_DATE,
      orderNumberOfItems: order.ORD_NBR_OF_ITEMS,
      orderNumberOfProducts: order.ORD_NBR_OF_PRODS,
      orderShippingCost: order.ORD_SHIP_COST,
      orderTaxCost: order.ORD_TAX_COST,
      customerCode: order.CUST_CODE,
      customerFirstName: order.CUST_FRST_NAME,
      customerLastName: order.CUST_LAST_NAME,
      customerAddress: order.CUST_ADDR1,
      customerCity: order.CUST_CITY,
      customerPostalZone: order.CUST_POST_ZONE,
      customerCountryCode: order.CUST_CTRY_CODE,
      customerPhone: order.CUST_PHN_NBR,
      customerInfo: order.CUST_INFO,
      customerEmail: order.CUST_EMAIL,
      customerGenderCode: order.CUST_GNDR_CODE,
      customerProvinceState: order.CUST_PROV_STATE,    
    },
  };
};

export const getCardTemplate = (template: any, order: IOrder, action:EOrderActions) => {
  const cardTemplate = new ACData.Template(template);
  const mapEdValues =  mapValues(order, action ?? EOrderActions.LIST);
  const card = cardTemplate.expand(mapEdValues);
  return card;
};

export const getPreviewCard = (order: IOrder) => {
  return CardFactory.heroCard(
    `${order.ORD_NBR} - ${order.CUST_FRST_NAME} ${order.CUST_LAST_NAME}`,
    `Status: ${order.ORD_STAT}`,
    [
      DEFAULT_IMAGE
    ]
  );
};

export const CreateInvokeResponse = (status: number, body?: unknown): InvokeResponse => {
  return { status, body };
};
export const CreateAdaptiveCardInvokeResponse = (statusCode: number, body?: Record<string, unknown>): AdaptiveCardInvokeResponse => {
  return {
           statusCode: statusCode,
           type: 'application/vnd.microsoft.card.adaptive',
           value: body
       };
};
export const CreateActionErrorResponse = ( statusCode: number, errorCode: number = -1, errorMessage: string = 'Unknown error') => {
  return {
      statusCode: statusCode,
      type: 'application/vnd.microsoft.error',
      value: {
          error: {
              code: errorCode,
              message: errorMessage,
          },
      },
  };
};

 export const   handleTeamsCardActionUpdateStatus =  async (context: TurnContext)  => {

  const request = context.activity.value;
  const data = request.action.data;
  console.log(`🎬 update Order Status, status=${data.orderStatus}`);

  if (data.orderStatus && data.orderId) {
      
      await updateOrderStatus(data.orderId, data.orderStatus);
      const order = await getOrder(data.orderId);    
      const card = getCardTemplate(orderDetails,order[0], EOrderActions.UPDATE);
      return CreateAdaptiveCardInvokeResponse(200, card );
  } else {
     
      return CreateActionErrorResponse(400,0, "Invalid request");
  }
}
