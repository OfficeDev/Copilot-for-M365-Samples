import { TableClient, TableEntity } from "@azure/data-tables";
import { Product,Supplier } from "./model";
import config from "../config";
export interface DbEntity extends TableEntity {
    etag: string;
    partitionKey: string;
    rowKey: string;
    timestamp: Date;
}

export interface DbProject extends DbEntity, Product { }
export interface DbSupplier extends DbEntity, Supplier { }
export default class DbService<DbEntityType> {

    private okToCacheLocally = false;
    private entityCache: DbEntity[] = [];

    constructor(okToCacheLocally: boolean) {
        if (!config.storageAccountConnectionString) {
            throw new Error("STORAGE_ACCOUNT_CONNECTION_STRING is not set");
        }
        this.okToCacheLocally = okToCacheLocally;
    }


    async getEntityByRowKey(tableName: string, rowKey: string): Promise<DbEntity> {
        if (!this.okToCacheLocally) {
            const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, tableName);
            const result = this.expandPropertyValues(await tableClient.getEntity(tableName, rowKey) as DbEntityType);
            return result as DbEntity;
        } else {
            let result = await this.getEntities(tableName);
            result = result.filter((e) => 
            {
                return e.rowKey === rowKey
            });
            if (result.length === 0) {
              console.log("not found")
            } else {
                return result[0];
            }
        }
    }

    async getEntities(tableName: string): Promise<DbEntity[]> {

        let entities;

        if (this.okToCacheLocally && this.entityCache.length > 0) {
            entities = this.entityCache;
        } else {
            const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, tableName);
            entities = tableClient.listEntities();
            this.entityCache = [];
            for await (const entity of entities) {
                const e = this.expandPropertyValues(entity as DbEntityType);
                this.entityCache.push(e as DbEntity);
            }
        }
        return this.entityCache;
    }

    async createEntity(tableName: string, rowKey: string, newEntity: DbEntityType): Promise<void> {
        const entity = this.compressPropertyValues(newEntity) as DbEntityType;
        const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, tableName);
        await tableClient.createEntity({
            partitionKey: tableName,
            rowKey,
            ...entity
        });
    }

    async updateEntity(tableName: string, updatedEntity: DbEntityType): Promise<void> {

        const e = this.compressPropertyValues(updatedEntity) as DbEntityType;
        const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, tableName);
            await tableClient.updateEntity(e as TableEntity, "Replace");
        
    }

    async getNextId(tableName:string):Promise<number> {
        try {
            let result = await this.getEntities(tableName);    
            return result.length+1;
    
        } catch (error) {
            console.error("Error fetching entities:", error);
        }
    }  

    private expandPropertyValues(entity: DbEntityType): DbEntityType {
        const result = {} as DbEntityType;
        for (const key in entity) {
            result[key] = this.expandPropertyValue(entity[key]);
        }
        return result;
    }

    private expandPropertyValue(v: any): any {
        if (typeof v === "string" && (v.charAt(0) === '{' || v.charAt(0) === '[')) {
            try {
                return JSON.parse(v);
            }
            catch (e) {
                return v;
            }
        } else {
            return v;
        }
    };

    private compressPropertyValues(entity: DbEntityType): DbEntityType {
        const result = {} as DbEntityType;
        for (const key in entity) {
            result[key] = this.compressPropertyValue(entity[key]);
        }
        return result;
    }

    private compressPropertyValue(v: any): any {
        if (typeof v === "object") {
            return JSON.stringify(v);
        } else {
            return v;
        }
    };
  
    
}
