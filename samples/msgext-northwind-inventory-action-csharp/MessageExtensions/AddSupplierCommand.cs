using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NorthwindInventory.NorthwindDB;
using NorthwindInventory.Models;

namespace NorthwindInventory.MessageExtensions
{
    public static class AddSupplierCommand
    {
        public const string CommandId = "addSupplier";

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
                    var initialParameters = new Dictionary<string, object>();

                    if (action.Data is JObject dataObject && dataObject.TryGetValue("taskParameters", out var taskParameters))
                    {
                        initialParameters = taskParameters.ToObject<Dictionary<string, object>>();
                    }

                    var url = $"{configuration["BotEndPoint"]}/ClientPages/supplier.html?p={Uri.EscapeDataString(JsonConvert.SerializeObject(initialParameters))}";

                    return new MessagingExtensionActionResponse()
                    {
                        Task = new TaskModuleContinueResponse()
                        {
                            Value = new TaskModuleTaskInfo()
                            {
                                Width = 400,
                                Height = 400,
                                Title = "Add Supplier",
                                Url = url,
                                FallbackUrl = url
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
                    var initialParameters = new Dictionary<string, object>();

                    if (action.Data is JObject dataObject && dataObject.TryGetValue("taskParameters", out var taskParameters))
                    {
                        initialParameters = taskParameters.ToObject<Dictionary<string, object>>();
                    }
                    else if (action.Data is JObject data)
                    {
                        initialParameters = data.ToObject<Dictionary<string, object>>();
                    }

                    var supplier = new Supplier
                    {
                        ETag = string.Empty,
                        PartitionKey = string.Empty,
                        RowKey = string.Empty,
                        Timestamp = DateTime.UtcNow,
                        SupplierID = null,
                        CompanyName = initialParameters.ContainsKey("companyName") ? initialParameters["companyName"]?.ToString() : string.Empty,
                        ContactName = initialParameters.ContainsKey("contactName") ? initialParameters["contactName"]?.ToString() : string.Empty,
                        ContactTitle = string.Empty,
                        Address = string.Empty,
                        City = string.Empty,
                        Region = string.Empty,
                        PostalCode = string.Empty,
                        Country = string.Empty,
                        Phone = string.Empty,
                        Fax = string.Empty,
                        HomePage = string.Empty
                    };

                    await SupplierService.CreateSupplierAsync(supplier, configuration);

                    var heroCard = new HeroCard
                    {
                        Title = "Supplier added successfully",
                        Subtitle = initialParameters.ContainsKey("companyName") ? initialParameters["companyName"]?.ToString() : string.Empty
                    };

                    var attachment = new MessagingExtensionAttachment
                    {
                        ContentType = HeroCard.ContentType,
                        Content = heroCard,
                        Preview = heroCard.ToAttachment()
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
