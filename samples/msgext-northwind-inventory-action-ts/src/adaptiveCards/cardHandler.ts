import {
    TurnContext,
    CardFactory
} from "botbuilder";
import { updateProduct, getProductEx, getCategories, getSuppliers } from "../northwindDB/products";
import { ProductEx } from '../northwindDB/model';
import editCard from './editCard.json';
import successCard from './successCard.json';
import * as ACData from "adaptivecards-templating";
import editProduct from '../adaptiveCards/editProduct.json';
import { CreateActionErrorResponse, CreateAdaptiveCardInvokeResponse, getInventoryStatus } from './utils';

function getEditCard(product: ProductEx): any {
    var card = getEditCardExpandedTemplate(product);
    return CardFactory.adaptiveCard(card);
}

function getEditCardExpandedTemplate(product: ProductEx): any {

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
            inventoryValue: product.InventoryValue,
            revenue: product.Revenue,
            averageDiscount: product.AverageDiscount
        }
    });
    return card;
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

async function handleTeamsCardActionRefresh(context: TurnContext) {

    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling refresh action, product ${data.productId}`);
    if (data.productId) {
        const product = await getProductEx(data.productId);
        const expandedTemplate = await getEditCardExpandedTemplate(product);
        return CreateAdaptiveCardInvokeResponse(200, expandedTemplate);
    }
}

async function handleTeamsCardActionEditProduct(context: TurnContext) {
    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling product edit`)
    if (data.productId) {

        const product = await getProductEx(data.productId);
        const categories = await getCategories();
        const catArray = Object.values(categories);
        const categoryChoices = catArray.map(category => ({
            title: category.CategoryName,
            value: category.CategoryID.toString()
        }));
        const suppliers = await getSuppliers();
        const suppArray = Object.values(suppliers);
        const supplierChoices = suppArray.map(supplier => ({
            title: supplier.CompanyName,
            value: supplier.SupplierID.toString()
        }));
        const template = new ACData.Template(editProduct);
        const card = template.expand({
            $root: {
                Categories: categoryChoices,
                Suppliers: supplierChoices,
                Product: product
            }
        });
        return CreateAdaptiveCardInvokeResponse(200, card);

    } else {
        return CreateActionErrorResponse(400, 0, "Invalid request");
    }
}

async function handleTeamsCardActionSaveProduct(context: TurnContext) {

    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling update save product action, productId=${data.productId}`);

    if (data.productId) {

        const product = await getProductEx(data.productId);
        product.ProductID = data.productId;
        product.ProductName = data.productName;
        product.CategoryID = data.categoryID.value;
        product.SupplierID = data.supplierID.value;
        product.QuantityPerUnit = data.qtyPerUnit ?? "0";
        product.UnitPrice = data.UnitPrice ?? "0";
        product.UnitsInStock = data.unitsInStock ?? "0";
        product.UnitsOnOrder = data.unitsOnOrder ?? "0";
        product.ReorderLevel = data.reorderLevel ?? "0";
        product.Discontinued = data.discontinued;
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
                message: `Product updated for ${product.ProductName}!`
            }
        });

        return CreateAdaptiveCardInvokeResponse(200, card);

    } else {

        return CreateActionErrorResponse(400, 0, "Invalid request");
    }
}

export default {
    getEditCard,
    handleTeamsCardActionUpdateStock,
    handleTeamsCardActionRestock,
    handleTeamsCardActionCancelRestock,
    handleTeamsCardActionRefresh,
    handleTeamsCardActionEditProduct,
    handleTeamsCardActionSaveProduct
}
