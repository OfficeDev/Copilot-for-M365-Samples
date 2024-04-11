import { CardFactory, MessagingExtensionAction, MessagingExtensionActionResponse, TurnContext } from "botbuilder";
import { createProduct } from "../northwindDB/products";

const COMMAND_ID = "addProduct";

async function handleTeamsMessagingExtensionFetchTask(
    context: TurnContext,
    action: MessagingExtensionAction
): Promise<MessagingExtensionActionResponse> {
    try {

        const templateJson = require('../adaptiveCards/addProduct.json');
        //const template = new ACData.Template(templateJson);     
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
    } catch (e) {
        console.log(e);
    }
}
async function handleTeamsMessagingExtensionSubmitAction(
    context: TurnContext,
    action: MessagingExtensionAction
): Promise<MessagingExtensionActionResponse> {
    try { 
        //mock product
        createProduct(null);
        const messageHtml = "Done";
        const heroCard = CardFactory.heroCard('', messageHtml);
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
    
    } catch (e) {
        console.log(e);
    }
}

export default { COMMAND_ID, handleTeamsMessagingExtensionFetchTask,handleTeamsMessagingExtensionSubmitAction }
