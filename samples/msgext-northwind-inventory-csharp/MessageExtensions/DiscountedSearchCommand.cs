using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Teams;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Schema.Teams;
using msgext_northwind_inventory_csharp.Models;
using msgext_northwind_inventory_csharp.NorthwindDB;
using msgext_northwind_inventory_csharp.Handlers;

namespace msgext_northwind_inventory_csharp.MessageExtensions
{
    public static class DiscountedSearchCommand
    {
        public const string CommandId = "discountSearch";
        private static int queryCount = 0;

        public static async Task<MessagingExtensionResponse> HandleTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, IConfiguration configuration, CancellationToken cancellationToken)
        {
            var categoryName = Utils.CleanupParam(query.Parameters?.FirstOrDefault(p => p.Name == "categoryName")?.Value as string);
            ProductService productService = new ProductService(configuration);
            var products = await productService.GetDiscountedProductsByCategory(categoryName);

            try
            {
                var attachments = new List<MessagingExtensionAttachment>();

                foreach (var product in products)
                {
                    var preview = new HeroCard
                    {
                        Title = product.ProductName,
                        Subtitle = $"Supplied by {product.SupplierName} of {product.SupplierCity}<br />{product.UnitsInStock} in stock",
                        Images = new List<CardImage> { new CardImage(product.ImageUrl) }
                    }.ToAttachment();

                    var resultCard = CardHandler.GetEditCard(product);

                    var attachment = new MessagingExtensionAttachment
                    {
                        ContentType = resultCard.ContentType,
                        Content = resultCard.Content,
                        Preview = preview
                    };

                    attachments.Add(attachment);
                }


                IList<MessagingExtensionAttachment> messagingExtensionsAttachments = attachments
                    .Select(a => new MessagingExtensionAttachment
                    {
                        ContentType = a.ContentType,
                        Content = a.Content,
                        Preview = a.Preview
                    })
                    .ToList();

                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = "list",
                        Attachments = messagingExtensionsAttachments
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
    }
}
