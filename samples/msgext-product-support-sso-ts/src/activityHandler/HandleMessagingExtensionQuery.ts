import {
  TurnContext,
  MessagingExtensionQuery,
  MessagingExtensionResponse,
  ThumbnailCard,
  Attachment,
} from "botbuilder";
import * as AdaptiveCards from "adaptivecards-templating";
import productCard from "../adaptiveCards/product.json";
import { AuthService } from "../services/AuthService";
import { GraphService } from "../services/GraphService";
import { cleanupParam } from "../util";
import { ThumbnailSet } from "@microsoft/microsoft-graph-types";
import config from "../config";

import {
  MessagingExtensionAttachment,
  MessagingExtensionResult,
} from "botframework-schema";

let queryCount = 0;

export const HandleMessagingExtensionQuery = async (
  context: TurnContext,
  query: MessagingExtensionQuery
): Promise<MessagingExtensionResponse> => {
  let productName = null,
    retailCategory = null;

  if (
    query.parameters.length === 1 &&
    query.parameters[0]?.name === "productName"
  ) {
    [productName, retailCategory] = (query.parameters[0]?.value ?? "").split(
      ","
    );
  } else {
    productName = cleanupParam(
      query.parameters.find((element) => element.name === "productName")?.value
    );
    retailCategory = cleanupParam(
      query.parameters.find((element) => element.name === "targetAudience")
        ?.value
    );
  }

  console.log(
    `🔎 Query #${++queryCount}:\nproductName=${productName}, retailCategory=${retailCategory}`
  );

  const credentials = new AuthService(context);
  const token = await credentials.getUserToken(query);
  if (!token) {
    return credentials.getSignInComposeExtension();
  }

  const graphService = new GraphService(token);
  const products = await graphService.getProducts(productName, retailCategory);
  
  const attachments = [] as MessagingExtensionAttachment[];

  for (const product of products) {
    const template = new AdaptiveCards.Template(productCard);

    const photo: ThumbnailSet = await graphService.getPhotoFromSharePoint(
      "Product Imagery",
      product.PhotoSubmission
    );

    const resultCard = template.expand({
      $root: {
        Product: product,
        Imageurl: photo.medium.url,
        SPOHostname: config.sharepointHost,
        SPOSiteUrl: config.sharepointSite,
      },
    });

    const preview = {
      title: product.Title,
      subtitle: product.RetailCategory,
      images: [{ url: photo.small.url }],
    } as ThumbnailCard;

    const previewAttachment = {
      contentType: "application/vnd.microsoft.card.thumbnail",
      content: preview,
    } as Attachment;

    const attachment = {
      contentType: "application/vnd.microsoft.card.adaptive",
      content: resultCard,
      preview: previewAttachment,
    } as MessagingExtensionAttachment;

    attachments.push(attachment);
  }

  return {
    composeExtension: {
      type: "result",
      attachmentLayout: "list",
      attachments: attachments,
    } as MessagingExtensionResult,
  } as MessagingExtensionResponse;
};
