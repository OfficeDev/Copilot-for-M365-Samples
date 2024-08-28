using System.Net;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using AdaptiveCards;
using AdaptiveCards.Templating;
using NorthwindInventory.NorthwindDB;
using NorthwindInventory.Models;

namespace NorthwindInventory.Handlers
{
    public static class CardHandler
    {
        public static Attachment GetEditCard(IProductEx product)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(baseDirectory, "AdaptiveCards", "editCard.json");
            var templateJson = File.ReadAllText(filePath);
            var template = new AdaptiveCardTemplate(templateJson);

            // Create a card payload by expanding the template with product data
            var cardPayload = template.Expand(new
            {
                productName = product.ProductName,
                unitsInStock = product.UnitsInStock,
                productId = product.ProductID,
                categoryId = product.CategoryID,
                imageUrl = product.ImageUrl,
                supplierName = product.SupplierName,
                supplierCity = product.SupplierCity,
                categoryName = product.CategoryName,
                inventoryStatus = product.InventoryStatus,
                unitPrice = product.UnitPrice,
                quantityPerUnit = product.QuantityPerUnit,
                unitsOnOrder = product.UnitsOnOrder,
                reorderLevel = product.ReorderLevel,
                unitSales = product.UnitSales,
                inventoryValue = product.InventoryValue,
                revenue = product.Revenue,
                averageDiscount = product.AverageDiscount
            });

            // Create an Adaptive Card from JSON string
            var adaptiveCard = AdaptiveCard.FromJson(cardPayload).Card;

            // Convert the AdaptiveCard to an Attachment
            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };

            return attachment;
        }

        public static async Task<AdaptiveCardInvokeResponse> HandleTeamsCardActionUpdateStockAsync(ITurnContext<IInvokeActivity> turnContext,IConfiguration configuration, CancellationToken cancellationToken)
        {
            try
            {
                var request = turnContext.Activity.Value as JObject;
                var data = request?["action"]?["data"];
                var txtStock = data?["txtStock"]?.ToString();
                var productId = data?["productId"]?.ToString();

                if (int.TryParse(txtStock, out var unitsInStock) && int.TryParse(productId, out var productID))
                {
                    var productService = new ProductService(configuration);
                    var product = await productService.GetProductExAsync(productID);

                    if (product == null)
                    {
                        return Utils.CreateActionErrorResponse((int)HttpStatusCode.NotFound, 1, "Product not found");
                    }

                    product.UnitsInStock = unitsInStock;
                    await productService.UpdateProductAsync(product);

                    return Utils.CreateAdaptiveCardInvokeResponse((int)HttpStatusCode.OK, CreateCardPayload(product, $"Stock updated for {product.ProductName} to {product.UnitsInStock}!"));
                }
                else
                {
                    return Utils.CreateActionErrorResponse((int)HttpStatusCode.BadRequest, 0, "Invalid request");
                }
            }
            catch (Exception ex)
            {
                return Utils.CreateActionErrorResponse((int)HttpStatusCode.InternalServerError, 0, ex.Message);
            }
        }

        public static async Task<AdaptiveCardInvokeResponse> HandleTeamsCardActionCancelRestockAsync(ITurnContext<IInvokeActivity> turnContext, IConfiguration configuration, CancellationToken cancellationToken)
        {
            try
            {
                var request = turnContext.Activity.Value as JObject;
                var data = request?["action"]?["data"];
                var productId = data?["productId"]?.ToString();

                if (int.TryParse(productId, out var productID))
                {
                    var productService = new ProductService(configuration);
                    var product = await productService.GetProductExAsync(productID);

                    if (product == null)
                    {
                        return Utils.CreateActionErrorResponse((int)HttpStatusCode.NotFound, 1, "Product not found");
                    }

                    product.UnitsOnOrder = 0;
                    await productService.UpdateProductAsync(product);

                    return Utils.CreateAdaptiveCardInvokeResponse((int)HttpStatusCode.OK, CreateCardPayload(product, $"Restock cancelled for {product.ProductName}."));
                }
                else
                {
                    return Utils.CreateActionErrorResponse((int)HttpStatusCode.BadRequest, 0, "Invalid request");
                }
            }
            catch (Exception ex)
            {
                return Utils.CreateActionErrorResponse((int)HttpStatusCode.InternalServerError, 0, ex.Message);
            }
        }

        public static async Task<AdaptiveCardInvokeResponse> HandleTeamsCardActionRestockAsync(ITurnContext<IInvokeActivity> turnContext, IConfiguration configuration, CancellationToken cancellationToken)
        {
            try
            {
                var request = turnContext.Activity.Value as JObject;
                var data = request?["action"]?["data"];
                var txtStock = data?["txtStock"]?.ToString();
                var productId = data?["productId"]?.ToString();

                if (int.TryParse(productId, out var productID) && int.TryParse(txtStock, out var unitsToOrder))
                {
                    var productService = new ProductService(configuration);
                    var product = await productService.GetProductExAsync(productID);

                    if (product == null)
                    {
                        return Utils.CreateActionErrorResponse((int)HttpStatusCode.NotFound, 1, "Product not found");
                    }

                    product.UnitsOnOrder += unitsToOrder;
                    await productService.UpdateProductAsync(product);

                    return Utils.CreateAdaptiveCardInvokeResponse((int)HttpStatusCode.OK, CreateCardPayload(product, $"Restocking {product.ProductName} placed order for {unitsToOrder} units."));
                }
                else
                {
                    return Utils.CreateActionErrorResponse((int)HttpStatusCode.BadRequest, 0, "Invalid request");
                }
            }
            catch (Exception ex)
            {
                return Utils.CreateActionErrorResponse((int)HttpStatusCode.InternalServerError, 0, ex.Message);
            }
        }

        private static Dictionary<string, object> CreateCardPayload(IProductEx product, string message)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(baseDirectory, "AdaptiveCards", "successCard.json");
            var templateJson = File.ReadAllText(filePath);
            var template = new AdaptiveCardTemplate(templateJson);

            var cardPayload = template.Expand(new
            {
                productName = product.ProductName,
                unitsInStock = product.UnitsInStock,
                productId = product.ProductID,
                categoryId = product.CategoryID,
                imageUrl = product.ImageUrl,
                supplierName = product.SupplierName,
                supplierCity = product.SupplierCity,
                categoryName = product.CategoryName,
                inventoryStatus = Utils.GetInventoryStatus(product),
                unitPrice = product.UnitPrice,
                quantityPerUnit = product.QuantityPerUnit,
                unitsOnOrder = product.UnitsOnOrder,
                reorderLevel = product.ReorderLevel,
                unitSales = product.UnitSales,
                inventoryValue = product.UnitsInStock * product.UnitPrice,
                revenue = product.Revenue,
                averageDiscount = product.AverageDiscount,
                message = message
            });

            // Convert JObject to Dictionary<string, object>
            var jsonObject = JObject.Parse(cardPayload.ToString());
            return jsonObject.ToObject<Dictionary<string, object>>();
        }
    }
}
