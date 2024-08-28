using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using NorthwindInventory.NorthwindDB;
using NorthwindInventory.Handlers;

namespace NorthwindInventory.MessageExtensions
{
    public static class ProductSearchCommand
    {
        public const string CommandId = "inventorySearch";
        private static int queryCount = 0;

        public static async Task<MessagingExtensionResponse> HandleTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, IConfiguration configuration, CancellationToken cancellationToken)
        {
            string productName, categoryName, inventoryStatus, supplierCity, stockLevel;

            if (query.Parameters.Count == 1 && query.Parameters[0]?.Name == "productName")
            {
                var values = query.Parameters[0]?.Value.ToString().Split(',');
                productName = Utils.CleanupParam(values.ElementAtOrDefault(0));
                categoryName = Utils.CleanupParam(values.ElementAtOrDefault(1));
                inventoryStatus = Utils.CleanupParam(values.ElementAtOrDefault(2));
                supplierCity = Utils.CleanupParam(values.ElementAtOrDefault(3));
                stockLevel = Utils.CleanupParam(values.ElementAtOrDefault(4));
            }
            else
            {
                productName = Utils.CleanupParam(query.Parameters?.FirstOrDefault(p => p.Name == "productName")?.Value as string);
                categoryName = Utils.CleanupParam(query.Parameters?.FirstOrDefault(p => p.Name == "categoryName")?.Value as string);
                inventoryStatus = Utils.CleanupParam(query.Parameters?.FirstOrDefault(p => p.Name == "inventoryStatus")?.Value as string);
                supplierCity = Utils.CleanupParam(query.Parameters?.FirstOrDefault(p => p.Name == "supplierCity")?.Value as string);
                stockLevel = Utils.CleanupParam(query.Parameters?.FirstOrDefault(p => p.Name == "stockQuery")?.Value as string);
            }

            Console.WriteLine($"🔎 Query #{++queryCount}:\nproductName={productName}, categoryName={categoryName}, inventoryStatus={inventoryStatus}, supplierCity={supplierCity}, stockLevel={stockLevel}");

            var productService = new ProductService(configuration);
            var products = await productService.SearchProducts(productName, categoryName, inventoryStatus, supplierCity, stockLevel);

            Console.WriteLine($"Found {products.Count} products in the Northwind database");
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
