using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using NorthwindInventory.NorthwindDB;
using NorthwindInventory.Handlers;

namespace NorthwindInventory.MessageExtensions
{
    public static class RevenueSearchCommand
    {
        public const string CommandId = "revenueSearch";
        private static int queryCount = 0;

        public static async Task<MessagingExtensionResponse> HandleTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            IConfiguration configuration,
            CancellationToken cancellationToken)
        {
            // Seek the parameter by name, don't assume it's in element 0 of the array
            string revenueRange = Utils.CleanupParam(
                query.Parameters?.FirstOrDefault(p => p.Name == "revenueRange")?.Value as string);

            Console.WriteLine($"💰 Revenue query #{++queryCount}: Products with revenue in range={revenueRange}");

            var productService = new ProductService(configuration);
            var products = await productService.GetProductsByRevenueRange(revenueRange);

            Console.WriteLine($"Found {products.Count} products in the Northwind database");
            var attachments = new List<MessagingExtensionAttachment>();

            foreach (var product in products)
            {
                var preview = new HeroCard
                {
                    Title = product.ProductName,
                    Subtitle = $"Revenue/period {product.Revenue.ToString("N0")}",
                    Images = new List<CardImage> { new(product.ImageUrl) }
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

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments
                }
            };
        }
    }
}