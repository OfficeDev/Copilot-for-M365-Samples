import { CardFactory, MessagingExtensionAction, MessagingExtensionActionResponse, TurnContext } from "botbuilder";
import { createProduct, getProductEx } from "../northwindDB/products";
import { Product } from "../northwindDB/model";
const COMMAND_ID = "addProduct";

async function handleTeamsMessagingExtensionFetchTask(
    context: TurnContext,
    action: MessagingExtensionAction
): Promise<MessagingExtensionActionResponse> {
    try {
        if (action.commandId === COMMAND_ID) {
            const templateJson = require('../adaptiveCards/addProduct.json');
            const card = CardFactory.adaptiveCard(templateJson);
            return {
                task: {
                    type: 'continue',
                    value: {
                        card: card,
                        height: 400,
                        title: `Add a product`,
                        width: 300
                    }
                }
            };
        }

    } catch (e) {
        console.log(e);
    }
}
async function handleTeamsMessagingExtensionSubmitAction(
    context: TurnContext,
    action: MessagingExtensionAction
): Promise<MessagingExtensionActionResponse> {
    try {       
        const data = action.data;
        if (action.commandId === COMMAND_ID) {
            switch (data.action) {
                case "submit": {
                    const product: Product = {
                        etag: "",
                        partitionKey: "",
                        rowKey: "",
                        timestamp: new Date(),
                        ProductID: "",
                        ProductName: data.productName,
                        SupplierID: "1",
                        CategoryID: "1",
                        QuantityPerUnit: "10 boxes x 20 bags",
                        UnitPrice: data.unitPrice,
                        UnitsInStock: 10,
                        UnitsOnOrder: 5,
                        ReorderLevel: 5,
                        Discontinued: false,
                        ImageUrl: "https://picsum.photos/seed/1/200/300"
                    }
                    await createProduct(product);                    
                    const heroCard = CardFactory.heroCard('Product added successfully', data.productName);
                    const attachment = {
                        contentType: heroCard.contentType,
                        content: heroCard.content,
                        preview: heroCard
                    };
            
                    return {
                        composeExtension: {
                            type: 'result',
                            attachmentLayout: 'list',
                            attachments: [
                                attachment
                            ]
                        }
                    };
                    
                }
                case "cancel": {

                }

            }

        }

    } catch (e) {
        console.log(e);
    }
}

export default { COMMAND_ID, handleTeamsMessagingExtensionFetchTask, handleTeamsMessagingExtensionSubmitAction }
