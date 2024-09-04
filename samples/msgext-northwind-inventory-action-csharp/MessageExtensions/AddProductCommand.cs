using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using NorthwindInventory.NorthwindDB;
using NorthwindInventory.Models;
using NorthwindInventory.AdaptiveCardMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NorthwindInventory.MessageExtensions
{
    public static class AddProductCommand
    {
        public const string CommandId = "addProduct";

        public static async Task<MessagingExtensionActionResponse> HandleTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            IConfiguration configuration,
            CancellationToken cancellationToken)
        {
            try
            {
                if (action.CommandId == CommandId)
                {
                    var productService = new ProductService(configuration);
                    var categories = await productService.GetCategoriesAsync();
                    var categoryChoices = categories.Select(c => new Choice
                    {
                        Title = c.Value.CategoryName,
                        Value = c.Value.CategoryID.ToString()
                    }).ToList();

                    var suppliers = await productService.GetSuppliersAsync();
                    var supplierChoices = suppliers.Select(s => new Choice
                    {
                        Title = s.Value.CompanyName,
                        Value = s.Value.SupplierID.ToString()
                    }).ToList();

                    // Handle initial parameters from Copilot action
                    var initialParameters = action.Data != null? JsonConvert.DeserializeObject<Dictionary<string, string>>(((JObject)action.Data).SelectToken("taskParameters").ToString()): new Dictionary<string, string>();

                    var productName = initialParameters.ContainsKey("name") ? initialParameters["name"] : "";

                    var resultCard = AdaptiveCardMethods.AdaptiveCardMethods.CreateAddProductCard(categoryChoices, supplierChoices, productName);

                    return new MessagingExtensionActionResponse()
                    {
                        Task = new TaskModuleContinueResponse()
                        {
                            Value = new TaskModuleTaskInfo()
                            {
                                Card = resultCard,
                                Title = "Add a product"
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }

        public static async Task<MessagingExtensionActionResponse> HandleTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            IConfiguration configuration,
            CancellationToken cancellationToken)
        {
            try
            {
                if (action.CommandId == CommandId)
                {
                    var jsonString = action.Data.ToString();
                    var data = JsonConvert.DeserializeObject<ProductActionData>(jsonString);

                    // Handle initial parameters from Copilot action
                    var initialParameters = action.Data != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(action.Data.ToString()) : new Dictionary<string, string>();

                    switch (data.action)
                    {
                        case "submit":
                            var product = new Product
                            {
                                ETag = string.Empty,
                                PartitionKey = string.Empty,
                                RowKey = string.Empty,
                                Timestamp = DateTime.UtcNow,
                                ProductID = null,
                                ProductName = initialParameters.ContainsKey("productName") ? initialParameters["productName"] : data.productName,
                                SupplierID = initialParameters.ContainsKey("supplierID") ? int.Parse(initialParameters["supplierID"]) : data.supplierID,
                                CategoryID = initialParameters.ContainsKey("categoryID") ? int.Parse(initialParameters["categoryID"]) : data.categoryID,
                                QuantityPerUnit = initialParameters.ContainsKey("qtyPerUnit") ? initialParameters["qtyPerUnit"] : data.qtyPerUnit,
                                UnitPrice = initialParameters.ContainsKey("unitPrice") ? int.Parse(initialParameters["unitPrice"]) : data.unitPrice,
                                UnitsInStock = initialParameters.ContainsKey("unitsInStock") ? int.Parse(initialParameters["unitsInStock"]) : data.unitsInStock,
                                UnitsOnOrder = initialParameters.ContainsKey("unitsOnOrder") ? int.Parse(initialParameters["unitsOnOrder"]) : data.unitsOnOrder,
                                ReorderLevel = initialParameters.ContainsKey("reorderLevel") ? int.Parse(initialParameters["reorderLevel"]) : data.reorderLevel,
                                Discontinued = initialParameters.ContainsKey("discontinued") ? bool.Parse(initialParameters["discontinued"]) : data.discontinued,
                                ImageUrl = "https://picsum.photos/seed/1/200/300"
                            };

                            var productService = new ProductService(configuration);
                            await productService.CreateProductAsync(product);

                            var heroCard = new HeroCard
                            {
                                Title = "Product added successfully",
                                Subtitle = data.productName
                            }.ToAttachment();

                            var attachment = new MessagingExtensionAttachment
                            {
                                ContentType = heroCard.ContentType,
                                Content = heroCard.Content,
                                Preview = heroCard
                            };

                            return new MessagingExtensionActionResponse
                            {
                                ComposeExtension = new MessagingExtensionResult
                                {
                                    Type = "result",
                                    AttachmentLayout = "list",
                                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                                }
                            };

                        case "cancel":
                            // Handle cancel action if needed
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }
    }
}