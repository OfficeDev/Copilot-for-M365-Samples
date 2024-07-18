using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using NorthwindInventory.NorthwindDB;
using NorthwindInventory.Handlers;

namespace NorthwindInventory.MessageExtensions
{
    public static class DiscountedSearchCommand
    {
        public const string CommandId = "discountSearch";

        public static async Task<MessagingExtensionResponse> HandleTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, IConfiguration configuration, CancellationToken cancellationToken)
        {
            var categoryName = Utils.CleanupParam(query.Parameters?.FirstOrDefault(p => p.Name == "categoryName")?.Value as string);
            var productService = new ProductService(configuration);
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
                        Images = [new(product.ImageUrl)]
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

                var messagingExtensionsAttachments = attachments
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
