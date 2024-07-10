import { Client } from '@microsoft/microsoft-graph-client';
import { ProductItem } from '../types/ProductItems';
import config from '../config';
import { getFileNameFromUrl } from '../util';
import { ColumnDefinition, ThumbnailSet } from '@microsoft/microsoft-graph-types';

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
  private graphClient: Client;

  constructor(token: string) {
    if (!token || !token.trim()) {
      throw new Error('SimpleGraphClient: Invalid token received.');
    }
    this._token = token;

    this.graphClient = Client.init({
      authProvider: done => {
        done(null, this._token);
      },
    });
  }
  async getSharePointSiteId(): Promise<string> {
    const { sharepointIds } = await this.graphClient
      .api(`/sites/${config.sharepointHost}:/${config.sharepointSite}`)
      .select('sharepointIds')
      .get();

    return sharepointIds.siteId;
  }

  async getProducts(productName: string, retailCategory: string): Promise<ProductItem[]> {
    const siteId = await this.getSharePointSiteId();

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

    const productItems: ProductItem[] = products.value.map((item: any) => {
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

  async getRetailCategories(): Promise<string[]> {
    const siteId = await this.getSharePointSiteId();

    const column: ColumnDefinition = await this.graphClient
      .api(`/sites/${siteId}/lists/Products/columns/RetailCategory`)
      .get();

    return column.choice.choices;
  }

  async updateProduct(product): Promise<ProductItem> {
    const siteId = await this.getSharePointSiteId();

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

  async getProduct(productId: string): Promise<ProductItem> {
    const siteId = await this.getSharePointSiteId();

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

  async getPhotoFromSharePoint(nameOfDrive: string, photoUrl: string): Promise<ThumbnailSet> {
    const siteId = await this.getSharePointSiteId();

    const drive = await this.graphClient.api(`/sites/${siteId}/drives`).get();
    const driveId = drive.value.find(drive => drive.name === nameOfDrive).id;

    const fileName = getFileNameFromUrl(photoUrl);

    const photo = await this.graphClient
      .api(`/sites/${siteId}/drives/${driveId}/root:/${fileName}:/thumbnails/0`)
      .get();

    return photo;
  }
}
