import {
    TurnContext,
    CardFactory
} from "botbuilder";
import { updateProduct, getProductEx } from "../northwindDB/products";
import { ProductEx } from '../northwindDB/model';
import editCard from './editCard.json';
import successCard from './successCard.json';
import * as ACData from "adaptivecards-templating";

import { CreateActionErrorResponse, CreateAdaptiveCardInvokeResponse, getInventoryStatus } from './utils';
import { sendEmailMessage, sendSMSMessage, sendWhatsAppMessage } from "../AzureCommunicationServices/contactSupplier";

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
            supplierPhone: product.SupplierPhone,
            supplierEmail: product.SupplierEmail,
            categoryName: product.CategoryName,
            inventoryStatus: product.InventoryStatus,
            unitPrice: product.UnitPrice,
            quantityPerUnit: product.QuantityPerUnit,
            unitsOnOrder: product.UnitsOnOrder,
            reorderLevel: product.ReorderLevel,
            unitSales: product.UnitSales,
            inventoryValue: product.InventoryValue,
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

        const product = await getProductEx(data.productId);
        product.UnitsInStock = Number(data.txtStock);
        await updateProduct(product);

        var template = new ACData.Template(successCard);
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
                inventoryStatus: getInventoryStatus(product),
                unitPrice: product.UnitPrice,
                quantityPerUnit: product.QuantityPerUnit,
                unitsOnOrder: product.UnitsOnOrder,
                reorderLevel: product.ReorderLevel,
                unitSales: product.UnitSales,
                inventoryValue: product.UnitsInStock * product.UnitPrice,
                revenue: product.Revenue,
                averageDiscount: product.AverageDiscount,
                // Card message
                message: `Stock updated for ${product.ProductName} to ${product.UnitsInStock}!`
            }
        });

        return CreateAdaptiveCardInvokeResponse(200, card);

    } else {

        return CreateActionErrorResponse(400, 0, "Invalid request");
    }
}
async function handleTeamsCardActionCancelRestock(context: TurnContext) {

    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling cancel restock action`)

    if (data.productId) {

        const product = await getProductEx(data.productId);
        product.UnitsOnOrder = 0;
        await updateProduct(product);

        var template = new ACData.Template(successCard);
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
                inventoryStatus: getInventoryStatus(product),
                unitPrice: product.UnitPrice,
                quantityPerUnit: product.QuantityPerUnit,
                unitsOnOrder: product.UnitsOnOrder,
                reorderLevel: product.ReorderLevel,
                unitSales: product.UnitSales,
                inventoryValue: product.UnitsInStock * product.UnitPrice,
                revenue: product.Revenue,
                averageDiscount: product.AverageDiscount,
                // Card message                
                message: `Restock cancelled for ${product.ProductName}.`
            }
        });
        return CreateAdaptiveCardInvokeResponse(200, card);

    } else {
        return CreateActionErrorResponse(400, 0, "Invalid request");
    }
}
async function handleTeamsCardActionRestock(context: TurnContext) {
    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling restock action, quantity=${data.txtStock}`)
    if (data.productId) {

        const product = await getProductEx(data.productId);
        product.UnitsOnOrder = Number(product.UnitsOnOrder) + Number(data.txtStock);
        await updateProduct(product);

        var template = new ACData.Template(successCard);
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
                inventoryStatus: getInventoryStatus(product),
                unitPrice: product.UnitPrice,
                quantityPerUnit: product.QuantityPerUnit,
                unitsOnOrder: product.UnitsOnOrder,
                reorderLevel: product.ReorderLevel,
                unitSales: product.UnitSales,
                inventoryValue: product.UnitsInStock * product.UnitPrice,
                revenue: product.Revenue,
                averageDiscount: product.AverageDiscount,
                // Card message
                message: `Restocking ${product.ProductName} placed order for ${data.txtStock ?? 0} units.`
            }
        });
        return CreateAdaptiveCardInvokeResponse(200, card);

    } else {
        return CreateActionErrorResponse(400, 0, "Invalid request");
    }
}

async function handleTeamsCardActionPlaceOrder(context: TurnContext) {
    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling place an order action, quantity=${data.txtQuantity}`);

    if (data.txtQuantity && data.productId) {

        const product = await getProductEx(data.productId);

        // send communication to the supplier
        var sendSMS = data.checkBoxSMS;
        var sendEmail = data.checkBoxEmail;
        var sendWhatsApp = data.checkBoxWhatsApp;

        product.UnitsOnOrder = Number(data.txtQuantity);

        try {
            var successMsg = await contactSupplier(product, sendSMS, sendEmail, sendWhatsApp);

            // After communication is successfully sent to the supplier, update the UnitsInOrder property in the product inventory.
            await updateProduct(product);
        }
        catch (e) {
            return CreateActionErrorResponse(400, 0, e);
        }

        var template = new ACData.Template(successCard);
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
                inventoryStatus: getInventoryStatus(product),
                unitPrice: product.UnitPrice,
                quantityPerUnit: product.QuantityPerUnit,
                unitsOnOrder: product.UnitsOnOrder,
                reorderLevel: product.ReorderLevel,
                unitSales: product.UnitSales,
                inventoryValue: product.UnitsInStock * product.UnitPrice,
                revenue: product.Revenue,
                averageDiscount: product.AverageDiscount,
                // Card message
                message: `Units on order updated for ${product.ProductName} to ${product.UnitsOnOrder}!\n${successMsg} sent to the supplier.`
            }
        });

        return CreateAdaptiveCardInvokeResponse(200, card);
    }
}

async function contactSupplier(product: ProductEx, sendSMS: any, sendEmail: any, sendWhatsApp: any): Promise<string> {
    const message = `Hello ${product.SupplierName}, please place an order for ${product.UnitsOnOrder} units of ${product.ProductName}. Thank you, Northwind Traders.`;
    let successMsg: string;

    if (sendSMS == 'true') {
        await sendSMSMessage(message, product.SupplierPhone);
        successMsg = "SMS";
    }
    if (sendEmail == 'true') {
        await sendEmailMessage(message, `New Order for ${product.ProductName}`, product.SupplierEmail);
        successMsg += ", Email";
    }
    if (sendWhatsApp == 'true') {
        await sendWhatsAppMessage(product.UnitsOnOrder.toString(), product.ProductName, product.SupplierPhone, product.SupplierName);
        successMsg += ", WhatsApp";
    }
    return successMsg;
}


export default {
    getEditCard,
    handleTeamsCardActionUpdateStock,
    handleTeamsCardActionRestock,
    handleTeamsCardActionCancelRestock,
    handleTeamsCardActionPlaceOrder,
}