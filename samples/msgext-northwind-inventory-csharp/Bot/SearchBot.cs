using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Schema;
using System.Net;
using NorthwindInventory.MessageExtensions;
using Newtonsoft.Json;
using NorthwindInventory.Handlers;

namespace NorthwindInventory.Bots
{
    public class SearchBot(ILogger<SearchBot> logger, IConfiguration configuration) : TeamsActivityHandler
    {
        private readonly ILogger<SearchBot> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

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

            return query.CommandId switch
            {
                ProductSearchCommand.CommandId => await ProductSearchCommand.HandleTeamsMessagingExtensionQueryAsync(turnContext, query, _configuration, cancellationToken),
                DiscountedSearchCommand.CommandId => await DiscountedSearchCommand.HandleTeamsMessagingExtensionQueryAsync(turnContext, query, _configuration, cancellationToken),
                _ => throw new InvalidOperationException("Unsupported command"),
            };
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

                return verb switch
                {
                    "ok" => await CardHandler.HandleTeamsCardActionUpdateStockAsync(turnContext, _configuration, cancellationToken),
                    "restock" => await CardHandler.HandleTeamsCardActionRestockAsync(turnContext, _configuration, cancellationToken),
                    "cancel" => await CardHandler.HandleTeamsCardActionCancelRestockAsync(turnContext, _configuration, cancellationToken),
                    _ => Utils.CreateActionErrorResponse((int)HttpStatusCode.OK, 0, $"ActionVerbNotSupported: {verb} is not a supported action verb.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling adaptive card invoke");
                return Utils.CreateActionErrorResponse((int)HttpStatusCode.InternalServerError, 0, ex.Message);
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
    }
}
