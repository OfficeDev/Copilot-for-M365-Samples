import { CardFactory, MessagingExtensionAction, MessagingExtensionActionResponse, TurnContext } from "botbuilder";
import config from '../config';
const COMMAND_ID = "addSupplier";
async function handleTeamsMessagingExtensionFetchTask(
    context: TurnContext,
    action: MessagingExtensionAction
): Promise<MessagingExtensionActionResponse> {
    try {
        if (action.commandId === COMMAND_ID) {
            let initialParameters = {};
            if (action.data && action.data.taskParameters) {
                initialParameters = action.data.dialogParameters;
            }
            const url = config.botEndPoint + "/client-pages/supplier.html";

            try {
                return {
                    task: {
                        type: 'continue',
                        value: {
                            width: 800,
                            height: 800,
                            title: "Add supplier",
                            url: url,
                            fallbackUrl: url
                        }
                    },
                };
            } catch (e) {
                console.error(e);
            }
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
            let initialParameters = {};
            if (action.data && action.data.taskParameters) {
                initialParameters = action.data.dialogParameters;
            } else {
                initialParameters = action.data
            }
            //todo: add supplier to database                

            const heroCard = CardFactory.heroCard('Supplier added successfully', initialParameters["companyName"]);
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

    } catch (e) {
        console.log(e);
    }
}

export default { COMMAND_ID, handleTeamsMessagingExtensionFetchTask, handleTeamsMessagingExtensionSubmitAction }


