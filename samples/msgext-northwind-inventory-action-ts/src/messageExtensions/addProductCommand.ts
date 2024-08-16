import { CardFactory, MessagingExtensionAction, MessagingExtensionActionResponse, TurnContext } from "botbuilder";
import { createProduct, getCategories, getSuppliers } from "../northwindDB/products";
import { Product } from "../northwindDB/model";
const COMMAND_ID = "addProduct";
import * as ACData from "adaptivecards-templating";
import addProduct from '../adaptiveCards/addProduct.json';
async function handleTeamsMessagingExtensionFetchTask(
    context: TurnContext,
    action: MessagingExtensionAction
): Promise<MessagingExtensionActionResponse> {
    try {
        if (action.commandId === COMMAND_ID) {
            const categories=await getCategories(); 
            const catArray = Object.values(categories);     
            const categoryChoices = catArray.map(category => ({
                title: category.CategoryName,
                value: category.CategoryID.toString()
            }));
            const suppliers=await getSuppliers();   
            const suppArray = Object.values(suppliers);     
            const supplierChoices = suppArray.map(supplier => ({
                title: supplier.CompanyName,
                value: supplier.SupplierID.toString()
            }));

            //pass initial values for copilot action   
            let initialParameters = {};
            if (action.data && action.data.taskParameters) {
                initialParameters = action.data.taskParameters;
            } else {
                initialParameters = action.data
            }
            const template = new ACData.Template(addProduct);
            let card = template.expand({
                $root: {
                    Categories: categoryChoices,
                    Suppliers: supplierChoices,
                    productName: "",
                }
            });
            if (initialParameters) {
                 card = template.expand({
                    $root: {
                        Categories: categoryChoices,
                        Suppliers: supplierChoices,
                        productName: initialParameters["name"] ? initialParameters["name"] : "",
                    }
                });

            }
            const resultCard = CardFactory.adaptiveCard(card);     
          
            return {
                task: {
                    type: 'continue',
                    value: {
                        card: resultCard,                        
                        title: `Add a product`,
                        
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
            //for Copilot action
            let initialParameters = {};
            if (action.data && action.data.taskParameters) {
                initialParameters = action.data.taskParameters;
            } else {
                initialParameters = action.data
            }
            switch (data.action) {
                case "submit": {
                    const product: Product = {
                        etag: "",
                        partitionKey: "",
                        rowKey: "",
                        timestamp: new Date(),
                        ProductID: "",
                        ProductName: initialParameters["productName"],
                        SupplierID: initialParameters["supplierID"],
                        CategoryID: initialParameters["categoryID"],
                        QuantityPerUnit: initialParameters["qtyPerUnit"],
                        UnitPrice: initialParameters["unitPrice"],
                        UnitsInStock: initialParameters["unitsInStock"],
                        UnitsOnOrder: initialParameters["unitsOnOrder"],
                        ReorderLevel:initialParameters["reorderLevel"],
                        Discontinued: initialParameters["discontinued"],
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
