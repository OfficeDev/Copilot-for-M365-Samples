import { CardFactory, MessagingExtensionAction, MessagingExtensionActionResponse, TurnContext } from "botbuilder";
import config from '../config';
import { createSupplier } from "../northwindDB/suppliers";
import { Supplier } from "../northwindDB/model";
const COMMAND_ID = "addSupplier";
async function handleTeamsMessagingExtensionFetchTask(
    context: TurnContext,
    action: MessagingExtensionAction
): Promise<MessagingExtensionActionResponse> {
    try {
        if (action.commandId === COMMAND_ID) {
            let initialParameters = {};
            if (action.data && action.data.taskParameters) {
                initialParameters = action.data.taskParameters;
            }else {
                initialParameters = action.data
            }
            const url = `${config.botEndPoint}/public/supplier.html?p=${encodeURIComponent(JSON.stringify(initialParameters))}&appId=${config.teamsAppId}`;
            try {
                return {
                    task: {
                        type: 'continue',
                        value: {
                            width: 400,
                            height: 400,
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
       
        if (action.commandId === COMMAND_ID) {
            //for Copilot action
            let initialParameters = {};
            if (action.data && action.data.taskParameters) {
                initialParameters = action.data.taskParameters;
            } else {
                initialParameters = action.data
            }
            const supplier:Supplier={
                etag: "",
                partitionKey: "",
                rowKey: "",
                timestamp: new Date(),
                SupplierID: "",
                CompanyName:initialParameters["companyName"],
                ContactName:initialParameters["contactName"],
                ContactTitle:initialParameters["contactTitle"]?initialParameters["contactTitle"]:"",
                Address:initialParameters["address"]?initialParameters["address"]:"",
                City:initialParameters["city"]?initialParameters["city"]:"",
                Region:"",
                PostalCode:"",
                Country:"",
                Phone:"",
                Fax:"",
                HomePage:""
            }
            await createSupplier(supplier);            

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


