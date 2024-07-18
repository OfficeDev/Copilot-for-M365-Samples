using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using NorthwindInventory.Models;

namespace NorthwindInventory.Handlers
{
    public static class Utils
    {
        public static InvokeResponse CreateInvokeResponse(int status, object body = null)
        {
            return new InvokeResponse
            {
                Status = status,
                Body = body
            };
        }

        public static AdaptiveCardInvokeResponse CreateAdaptiveCardInvokeResponse(int statusCode, Dictionary<string, object> body = null)
        {
            return new AdaptiveCardInvokeResponse
            {
                StatusCode = statusCode,
                Type = "application/vnd.microsoft.card.adaptive",
                Value = body
            };
        }

        public static AdaptiveCardInvokeResponse CreateActionErrorResponse(int statusCode, int errorCode = -1, string errorMessage = "Unknown error")
        {
            var errorResponse = new
            {
                error = new
                {
                    code = errorCode,
                    message = errorMessage
                }
            };

            return new AdaptiveCardInvokeResponse
            {
                StatusCode = statusCode,
                Type = "application/vnd.microsoft.error",
                Value = errorResponse
            };
        }

        public static string GetInventoryStatus(IProduct product)
        {
            if (product.UnitsInStock >= product.ReorderLevel)
            {
                return "In stock";
            }
            else if (product.UnitsInStock < product.ReorderLevel && product.UnitsOnOrder == 0)
            {
                return "Low stock";
            }
            else if (product.UnitsInStock < product.ReorderLevel && product.UnitsOnOrder > 0)
            {
                return "On order";
            }
            else if (product.UnitsInStock == 0)
            {
                return "Out of stock";
            }
            else
            {
                return "Unknown"; // fallback
            }
        }

        public static string CleanupParam(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            return value.Trim().Split(',')[0].Replace("*", string.Empty);
        }
    }
}
