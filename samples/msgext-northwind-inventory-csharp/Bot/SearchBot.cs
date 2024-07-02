using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using msgext_northwind_inventory_csharp.MessageExtensions;
using Newtonsoft.Json;
using msgext_northwind_inventory_csharp.Handlers;

namespace msgext_northwind_inventory_csharp.Bots
{
    public class SearchBot : TeamsActivityHandler
    {
        private readonly ILogger<SearchBot> _logger;
        private readonly IConfiguration _configuration;

        public SearchBot(ILogger<SearchBot> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {
                query = JsonConvert.DeserializeObject<MessagingExtensionQuery>(turnContext.Activity.Value.ToString());
            }

            if (query == null)
            {
                throw new InvalidOperationException("Query is null after deserialization");
            }

            switch (query.CommandId)
            {
                case ProductSearchCommand.CommandId:
                    return await ProductSearchCommand.HandleTeamsMessagingExtensionQueryAsync(turnContext, query, _configuration, cancellationToken);

                case DiscountedSearchCommand.CommandId:
                    return await DiscountedSearchCommand.HandleTeamsMessagingExtensionQueryAsync(turnContext, query, _configuration, cancellationToken);

                default:
                    throw new InvalidOperationException("Unsupported command");
            }
        }

        protected override async Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(
            ITurnContext<IInvokeActivity> turnContext,
            AdaptiveCardInvokeValue invokeValue,
            CancellationToken cancellationToken)
        {
            try
            {
                var action = invokeValue.Action as AdaptiveCardInvokeAction;
                if (action == null)
                {
                    throw new InvalidOperationException("AdaptiveCardInvokeAction is null");
                }

                var verb = action.Verb?.ToString();

                switch (verb)
                {
                    case "ok":
                        return await CardHandler.HandleTeamsCardActionUpdateStockAsync(turnContext, _configuration ,cancellationToken);

                    case "restock":
                        return await CardHandler.HandleTeamsCardActionRestockAsync(turnContext, _configuration, cancellationToken);

                    case "cancel":
                        return await CardHandler.HandleTeamsCardActionCancelRestockAsync(turnContext, _configuration, cancellationToken);

                    default:
                        return CreateActionErrorResponse(HttpStatusCode.BadRequest, 0, $"ActionVerbNotSupported: {verb} is not a supported action verb.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling adaptive card invoke");
                return CreateActionErrorResponse(HttpStatusCode.InternalServerError, 0, ex.Message);
            }
        }

        protected override async Task<InvokeResponse> OnInvokeActivityAsync(
            ITurnContext<IInvokeActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                switch (turnContext.Activity.Name)
                {
                    case "adaptiveCard/action":
                        var adaptiveCardInvokeValue = JsonConvert.DeserializeObject<AdaptiveCardInvokeValue>(turnContext.Activity.Value.ToString());
                        var adaptiveCardResponse = await OnAdaptiveCardInvokeAsync(turnContext, adaptiveCardInvokeValue, cancellationToken);
                        return new InvokeResponse
                        {
                            Status = (int)HttpStatusCode.OK,
                            Body = adaptiveCardResponse
                        };

                    case "composeExtension/query":
                        var response = await OnTeamsMessagingExtensionQueryAsync(turnContext, turnContext.Activity.Value as MessagingExtensionQuery, cancellationToken);
                        return new InvokeResponse
                        {
                            Status = (int)HttpStatusCode.OK,
                            Body = response
                        };

                    default:
                        return new InvokeResponse
                        {
                            Status = (int)HttpStatusCode.OK,
                            Body = $"Unknown invoke activity handled as default - {turnContext.Activity.Name}"
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnInvokeActivityAsync");
                return new InvokeResponse
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Body = $"Invoke activity received - {turnContext.Activity.Name}"
                };
            }
        }

        private AdaptiveCardInvokeResponse CreateActionErrorResponse(HttpStatusCode statusCode, int errorCode, string message)
        {
            return new AdaptiveCardInvokeResponse
            {
                StatusCode = (int)statusCode,
                Type = "application/vnd.microsoft.error",
                Value = new { Code = errorCode, Message = message }
            };
        }
    }

    public static class ActionHandler
    {
        public static async Task<AdaptiveCardInvokeResponse> HandleTeamsCardActionUpdateStockAsync(
            ITurnContext<IInvokeActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // Implement your logic here
            return await Task.FromResult(new AdaptiveCardInvokeResponse());
        }

        public static async Task<AdaptiveCardInvokeResponse> HandleTeamsCardActionRestockAsync(
            ITurnContext<IInvokeActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // Implement your logic here
            return await Task.FromResult(new AdaptiveCardInvokeResponse());
        }

        public static async Task<AdaptiveCardInvokeResponse> HandleTeamsCardActionCancelRestockAsync(
            ITurnContext<IInvokeActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // Implement your logic here
            return await Task.FromResult(new AdaptiveCardInvokeResponse());
        }
    }
}
