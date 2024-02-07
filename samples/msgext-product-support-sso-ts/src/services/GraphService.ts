import {Client} from '@microsoft/microsoft-graph-client';
import {ProductItem} from '../types/ProductItems';
import config from '../config';
import {getFileNameFromUrl} from '../util';
import { ThumbnailSet } from '@microsoft/microsoft-graph-types';

const listFields = [
  'id',
  'fields/Title',
  'fields/RetailCategory',
  'fields/PhotoSubmission',
  'fields/CustomerRating',
  'fields/ReleaseDate',
];

export class GraphService {
  private _token: string;
  graphClient: Client;
  constructor(token) {
    if (!token || !token.trim()) {
      throw new Error('SimpleGraphClient: Invalid token received.');
    }
    this._token = token;
    // Get an Authenticated Microsoft Graph client using the token issued to the user.
    this.graphClient = Client.init({
      authProvider: done => {
        done(null, this._token); // First parameter takes an error if you can't get an access token.
      },
    });
  }
  async getSharePointStieId(): Promise<string> {
    const {sharepointIds} = await this.graphClient
      .api(`/sites/${config.sharepointHost}:/${config.sharepointSite}`)
      .select('sharepointIds')
      .get();
    return sharepointIds.siteId;
  }

  async getProducts(productName, retailCategory): Promise<ProductItem[]> {
    const siteId = await this.getSharePointStieId();
    let filterText = `&$filter=startswith(fields/Title,'${productName}')`;
    if (productName && retailCategory) {
      filterText = `&$filter=startswith(fields/Title,'${productName}') and fields/RetailCategory eq '${retailCategory}'`;
    } else if (productName) {
      filterText = `&$filter=startswith(fields/Title,'${productName}')`;
    } else if (retailCategory) {
      filterText = `&$filter=fields/RetailCategory eq '${retailCategory}'`;
    }
    const products = await this.graphClient
      .api(
        `/sites/${siteId}/lists/Products/items?expand=fields&select=${listFields.join(
          ','
        )}${filterText}`
      )
      .get();
    const productItems: ProductItem[] = products.value.map(item => {
      return {
        Id: item.id,
        Title: item.fields.Title,
        RetailCategory: item.fields.RetailCategory,
        PhotoSubmission: item.fields.PhotoSubmission,
        CustomerRating: item.fields.CustomerRating,
        ReleaseDate: item.fields.ReleaseDate,
      };
    });
    return productItems;
  }

  async getretailCategories() {
    const siteId = await this.getSharePointStieId();
    const column = await this.graphClient
      .api(`/sites/${siteId}/lists/Products/columns/RetailCategory`)
      .get();
    return column.choice.choices;
  }

  //update product details in the SharePoint list using productId and siteId with product infor
  async updateProduct(product): Promise<ProductItem> {
    const siteId = await this.getSharePointStieId();
    const item = {
      fields: {
        Title: product.Title,
        RetailCategory: product.RetailCategory,
        ReleaseDate: product.ReleaseDate,
      },
    };
    const updatedData = await this.graphClient
      .api(`/sites/${siteId}/lists/Products/items/${product.Id}`)
      .update(item);
    return {
      Id: updatedData.id,
      Title: updatedData.fields.Title,
      RetailCategory: updatedData.fields.RetailCategory,
      PhotoSubmission: updatedData.fields.PhotoSubmission,
      CustomerRating: updatedData.fields.CustomerRating,
      ReleaseDate: updatedData.fields.ReleaseDate,
    };
  }
  //get product details from the SharePoint list using productId and siteId
  async getProduct(productId): Promise<ProductItem> {
    const siteId = await this.getSharePointStieId();
    const product = await this.graphClient
      .api(`/sites/${siteId}/lists/Products/items/${productId}`)
      .get();
    return {
      Id: product.id,
      Title: product.fields.Title,
      RetailCategory: product.fields.RetailCategory,
      PhotoSubmission: product.fields.PhotoSubmission,
      CustomerRating: product.fields.CustomerRating,
      ReleaseDate: product.fields.ReleaseDate,
    };
  }
  async getPhotoFromSharePoint(nameOfDrive, photoUrl): Promise<ThumbnailSet> {
    const siteId = await this.getSharePointStieId();
    const drive = await this.graphClient.api(`/sites/${siteId}/drives`).get();
    const driveId = drive.value.find(drive => drive.name === nameOfDrive).id;
    const fileName = getFileNameFromUrl(photoUrl);
    const photo = await this.graphClient
      .api(`/sites/${siteId}/drives/${driveId}/root:/${fileName}:/thumbnails/0`)
      .get();
    return photo;
  }
}
