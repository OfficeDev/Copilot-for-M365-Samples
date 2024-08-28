import {
    TABLE_NAME, Supplier
} from './model';

import DbService, { DbSupplier } from './dbService';

export async function createSupplier(supplier: Supplier): Promise<string> {  
    const dbService = new DbService<DbSupplier>(false);
    const nextId = await dbService.getNextId(TABLE_NAME.SUPPLIER);
    const newSupplier: DbSupplier = {
        etag: "",
        partitionKey: TABLE_NAME.SUPPLIER,
        rowKey: nextId.toString(),
        timestamp: new Date(),
        SupplierID: nextId.toString(),
        CompanyName: supplier.CompanyName,
        ContactName: supplier.ContactName,
        ContactTitle: supplier.ContactTitle,
        Address:supplier.Address,
        City: supplier.City,
        Region: "",
        PostalCode: "",
        Country: "",
        Phone: "",
        Fax: "",
        HomePage: ""
    };
    await dbService.createEntity(TABLE_NAME.SUPPLIER, newSupplier.SupplierID, newSupplier);
    return newSupplier.SupplierID;
};
