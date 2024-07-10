import {
  AdaptiveCardInvokeResponse,
  AdaptiveCardInvokeValue,
  TurnContext,
} from 'botbuilder';
import {
  CreateActionErrorResponse,
  CreateAdaptiveCardInvokeResponse,
} from '../util';
import {AuthService} from '../services/AuthService';
import {GraphService} from '../services/GraphService';
import * as AdaptiveCards from 'adaptivecards-templating';
import viewProduct from '../adaptiveCards/viewProduct.json';
import editProduct from '../adaptiveCards/editForm.json';
import { ThumbnailSet } from '@microsoft/microsoft-graph-types';
import config from '../config';
export const HandleAdaptiveCardInvoke = async (
  context: TurnContext,
  invokeValue: AdaptiveCardInvokeValue
): Promise<AdaptiveCardInvokeResponse> => {
  if (invokeValue.action.type !== 'Action.Execute') {
    return CreateActionErrorResponse(
      400,
      0,
      `ActionTypeNotSupported: ${invokeValue.action.type} is not a supported action.`
    );
  }

  const credentials = new AuthService(context);
  const token = await credentials.getUserToken();
  if (!token) {
    // There is no token, so the user has not signed in yet.
    return credentials.getSignInAdaptiveCardInvokeResponse();
  }
  const graphService = new GraphService(token);
  const categories = await graphService.getretailCategories();
  const verb = invokeValue.action.verb;
  const data: Record<string, unknown> = invokeValue.action.data;
  try {
    switch (verb) {
      case 'edit-save':
        const updatedProduct = {
          Id: data.Id,
          Title: data.Title,
          RetailCategory: data.RetailCategory,
          ReleaseDate: data.ReleaseDate,
        };
        const product = await graphService.updateProduct(updatedProduct);
        const viewTemplate = new AdaptiveCards.Template(viewProduct);
        const photo: ThumbnailSet = await graphService.getPhotoFromSharePoint(
          'Product Imagery',
          product.PhotoSubmission
        );
        const cardS = viewTemplate.expand({
          $root: {
            Product: product,
            Imageurl: photo.medium.url,
            SPOHostname:config.sharepointHost,
            SPOSiteUrl:config.sharepointSite
          },
        });
        return CreateAdaptiveCardInvokeResponse(200, cardS);
      case 'edit':
          return await createEditForm(data.Id);
      case 'cancel':
        return await refreshCard(data.Id);
      case 'refresh':
        return await refreshCard(data.Id);
      default:
        return CreateActionErrorResponse(
          400,
          0,
          `ActionVerbNotSupported: ${verb} is not a supported action verb.`
        );
    }
  } catch (error) {
    return CreateActionErrorResponse(500, 0, error.message);
  }
  //create editform
  async function createEditForm(id) {
    const product = await graphService.getProduct(id);
    const categories = await graphService.getretailCategories();
    const editTemplate = new AdaptiveCards.Template(editProduct);
    const cardP = editTemplate.expand({
      $root: {
        Product: product,
        RetailCategories: categories,
      },
    });
    return CreateAdaptiveCardInvokeResponse(200, cardP);
  }
  //refresh to initial card
  async function refreshCard(id) {
    const response = await graphService.getProduct(id);
    const viewTemplate = new AdaptiveCards.Template(viewProduct);
    const cardP = viewTemplate.expand({
      $root: {
        Product: response,
        RetailCategories: categories,
      },
    });
    return CreateAdaptiveCardInvokeResponse(200, cardP);
  }
};
