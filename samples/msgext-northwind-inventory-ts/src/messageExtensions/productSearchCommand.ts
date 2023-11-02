import {
    CardFactory,
    TurnContext,
    MessagingExtensionQuery,
    MessagingExtensionResponse,
} from "botbuilder";
import { searchProducts } from "../northwindDB/products";
import cardHandler from "../adaptiveCards/cardHandler";

const COMMAND_ID = "inventorySearch";

let queryCount = 0;
async function handleTeamsMessagingExtensionQuery(
    context: TurnContext,
    query: MessagingExtensionQuery
): Promise<MessagingExtensionResponse> {

    // Unpack the parameters. From Copilot they'll come in the parameters array; from a human they'll be comma separated
    let [productName, categoryName, inventoryStatus, supplierCity, stockLevel] = (query.parameters[0]?.value.split(','));

    productName = cleanupParam(query.parameters[0]?.value);
    categoryName ??= cleanupParam(query.parameters[1]?.value);
    inventoryStatus ??= cleanupParam(query.parameters[2]?.value);
    supplierCity ??= cleanupParam(query.parameters[3]?.value);
    stockLevel ??= cleanupParam(query.parameters[4]?.value);
    console.log(`🔎 Query #${++queryCount}:\nproductName=${productName}, categoryName=${categoryName}, inventoryStatus=${inventoryStatus}, supplierCity=${supplierCity}, stockLevel=${stockLevel}`);

    const products = await searchProducts(productName, categoryName, inventoryStatus, supplierCity, stockLevel);

    console.log(`Found ${products.length} products in the Northwind database`)
    const attachments = [];
    products.forEach((product) => {
        const preview = CardFactory.heroCard(product.ProductName,
            `Supplied by ${product.SupplierName} of ${product.SupplierCity}<br />${product.UnitsInStock} in stock`,
            [product.ImageUrl]);
        
        const resultCard = cardHandler.getEditCard(product);
        const attachment = { ...resultCard, preview };
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

export default { COMMAND_ID, handleTeamsMessagingExtensionQuery }
