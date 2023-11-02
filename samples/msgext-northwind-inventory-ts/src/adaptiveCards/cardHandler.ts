import {
    TurnContext,
    CardFactory
} from "botbuilder";
import { updateProduct, getProduct } from "../northwindDB/products";
import { ProductEx } from '../northwindDB/model';
import editCard from './editCard.json';
import successCard from './successCard.json';
import errorCard from './errorCard.json'
import * as ACData from "adaptivecards-templating";

import { CreateInvokeResponse, getInventoryStatus } from './utils';

function getEditCard(product: ProductEx): any {

    var template = new ACData.Template(editCard);
    var card = template.expand({
        $root: {
            productName: product.ProductName,
            unitsInStock: product.UnitsInStock,
            productId: product.ProductID,
            categoryId: product.CategoryID,
            imageUrl: product.ImageUrl,
            supplierName: product.SupplierName,
            supplierCity: product.SupplierCity,
            categoryName: product.CategoryName,
            inventoryStatus: product.InventoryStatus,
            unitPrice: product.UnitPrice,
            quantityPerUnit: product.QuantityPerUnit,
            unitsOnOrder: product.UnitsOnOrder,
            reorderLevel: product.ReorderLevel,
            unitSales: product.UnitSales,
            inventoryValue: product.InventoryCost,
            revenue: product.Revenue,
            averageDiscount: product.AverageDiscount
        }
    });
    return CardFactory.adaptiveCard(card);
}

async function handleTeamsCardActionUpdateStock(context: TurnContext) {

    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling update stock action, quantity=${data.txtStock}`);

    if (data.txtStock && data.productId) {
        
        const product = await getProduct(data.productId);
        product.UnitsInStock = Number(data.txtStock);
        await updateProduct(product);
        
        var template = new ACData.Template(successCard);
        var card = template.expand({
            $root: {
                productName: product.ProductName,
                unitsInStock: product.UnitsInStock,
                productId: data.productId,
                categoryId: data.categoryId,
                imageUrl: data.imageUrl,
                supplierName: data.supplierName,
                supplierCity: data.supplierCity,
                categoryName: data.categoryName,
                inventoryStatus: getInventoryStatus(product),
                unitPrice: data.unitPrice,
                quantityPerUnit: data.quantityPerUnit,
                unitsOnOrder: data.unitsOnOrder,
                reorderLevel: data.reorderLevel,
                unitSales: data.unitSales,
                inventoryValue: product.UnitsInStock * product.UnitPrice,
                revenue: data.revenue,
                averageDiscount: data.averageDiscount,
                // Card message
                message: `Stock updated for ${data.productName} to ${product.UnitsInStock}!`
            }
        });
        var responseBody = { statusCode: 200, type: "application/vnd.microsoft.card.adaptive", value: card }
        return CreateInvokeResponse(responseBody);

    } else {
        var errorBody = { statusCode: 200, type: "application/vnd.microsoft.card.adaptive", value: errorCard }
        return CreateInvokeResponse(errorBody);
    }
}
async function handleTeamsCardActionCancelRestock(context: TurnContext) {

    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling cancel restock action`)

    if (data.productId) {

        const product = await getProduct(data.productId);
        product.UnitsOnOrder = 0;
        await updateProduct(product);

        var template = new ACData.Template(successCard);
        var card = template.expand({
            $root: {
                productName: data.productName,
                unitsInStock: data.unitsInStock,
                productId: data.productId,
                categoryId: data.categoryId,
                imageUrl: data.imageUrl,
                supplierName: data.supplierName,
                supplierCity: data.supplierCity,
                categoryName: data.categoryName,
                inventoryStatus: getInventoryStatus(product),
                unitPrice: data.unitPrice,
                quantityPerUnit: data.quantityPerUnit,
                unitsOnOrder: product.UnitsOnOrder,
                reorderLevel: data.reorderLevel,
                unitSales: data.unitSales,
                inventoryValue: data.inventoryValue,
                revenue: data.revenue,
                averageDiscount: data.averageDiscount,
                // Card message                
                message: `Restock cancelled for ${data.productName}.`
            }
        });
        var responseBody = { statusCode: 200, type: "application/vnd.microsoft.card.adaptive", value: card }
        return CreateInvokeResponse(responseBody);

    } else {
        var errorBody = { statusCode: 200, type: "application/vnd.microsoft.card.adaptive", value: errorCard }
        return CreateInvokeResponse(errorBody);
    }
}
async function handleTeamsCardActionRestock(context: TurnContext) {
    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling restock action, quantity=${data.txtStock}`)
    if (data.productId) {

        const product = await getProduct(data.productId);
        product.UnitsOnOrder = Number(product.UnitsOnOrder) + Number(data.txtStock);
        await updateProduct(product);

        var template = new ACData.Template(successCard);
        var card = template.expand({
            $root: {
                productName: data.productName,
                unitsInStock: product.UnitsInStock,
                productId: data.productId,
                categoryId: data.categoryId,
                imageUrl: data.imageUrl,
                supplierName: data.supplierName,
                supplierCity: data.supplierCity,
                categoryName: data.categoryName,
                inventoryStatus: getInventoryStatus(product),
                unitPrice: data.unitPrice,
                quantityPerUnit: data.quantityPerUnit,
                unitsOnOrder: product.UnitsOnOrder,
                reorderLevel: data.reorderLevel,
                unitSales: data.unitSales,
                inventoryValue: data.inventoryValue,
                revenue: data.revenue,
                averageDiscount: data.averageDiscount,
                // Card message
                message: `Restocking ${data.productName} placed order for ${data.txtStock ?? 0} units.`
            }
        });
        var responseBody = { statusCode: 200, type: "application/vnd.microsoft.card.adaptive", value: card }
        return CreateInvokeResponse(responseBody);

    } else {
        var errorBody = { statusCode: 200, type: "application/vnd.microsoft.card.adaptive", value: errorCard }
        return CreateInvokeResponse(errorBody);
    }
}

export default { getEditCard, handleTeamsCardActionUpdateStock, handleTeamsCardActionRestock, handleTeamsCardActionCancelRestock }
