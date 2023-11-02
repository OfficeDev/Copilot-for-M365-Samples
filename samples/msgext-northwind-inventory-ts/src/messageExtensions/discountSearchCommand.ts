import {
    CardFactory,
    TurnContext,
    MessagingExtensionQuery,
    MessagingExtensionResponse,
} from "botbuilder";
import { getDiscountedProductsByCategory } from "../northwindDB/products";
import { editCard } from '../adaptiveCards/editCard';
import * as ACData from "adaptivecards-templating";

const COMMAND_ID = "discountSearch";

// #region Query handling

let queryCount = 0;
async function handleTeamsMessagingExtensionQuery(
    context: TurnContext,
    query: MessagingExtensionQuery
): Promise<MessagingExtensionResponse> {

    let categoryName = cleanupParam(query.parameters[0]?.value);

    console.log(`💰 Discount query #${++queryCount}: Discounted products with categoryName=${categoryName}`);

    const products = await getDiscountedProductsByCategory(categoryName);

    console.log(`Found ${products.length} products in the Northwind database`)
    const attachments = [];
    products.forEach((product) => {
        const preview = CardFactory.heroCard(product.ProductName,
            `Avg discount ${product.AverageDiscount}%<br />Supplied by ${product.SupplierName} of ${product.SupplierCity}`,
            [product.ImageUrl]);
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
        const adaptive = CardFactory.adaptiveCard(card);
        const attachment = { ...adaptive, preview };
        attachments.push(attachment);
    });
    return {
        composeExtension: {
            type: "result",
            attachmentLayout: "list",
            attachments: attachments,
        },
    };
}

function cleanupParam(value: string): string {

    if (!value) {
        return "";
    } else {
        let result = value.trim();
        result = result.split(',')[0];          // Remove extra data
        result = result.replace("*", "");       // Remove wildcard characters from Copilot
        return result;
    }
}

//#endregion

export default { COMMAND_ID, handleTeamsMessagingExtensionQuery }
