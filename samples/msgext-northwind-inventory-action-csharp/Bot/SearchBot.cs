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
                RevenueSearchCommand.CommandId => await RevenueSearchCommand.HandleTeamsMessagingExtensionQueryAsync(turnContext, query, _configuration, cancellationToken),
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
                    "edit" => await CardHandler.HandleTeamsCardActionEditProductAsync(turnContext, _configuration, cancellationToken),
                    "edit-save" => await CardHandler.HandleTeamsCardActionSaveProductAsync(turnContext, _configuration, cancellationToken),
                    _ => Utils.CreateActionErrorResponse((int)HttpStatusCode.OK, 0, $"ActionVerbNotSupported: {verb} is not a supported action verb.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling adaptive card invoke");
                return Utils.CreateActionErrorResponse((int)HttpStatusCode.InternalServerError, 0, ex.Message);
            }
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            try
            {
                switch (action.CommandId)
                {
                    case AddProductCommand.CommandId:
                        return await AddProductCommand.HandleTeamsMessagingExtensionFetchTaskAsync(turnContext, action, _configuration, cancellationToken);
                    case AddSupplierCommand.CommandId:
                        return await AddSupplierCommand.HandleTeamsMessagingExtensionFetchTaskAsync(turnContext, action, _configuration, cancellationToken);
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            try
            {
                switch (action.CommandId)
                {
                    case AddProductCommand.CommandId:
                        return await AddProductCommand.HandleTeamsMessagingExtensionSubmitActionAsync(turnContext, action, _configuration, cancellationToken);
                    case AddSupplierCommand.CommandId:
                        return await AddSupplierCommand.HandleTeamsMessagingExtensionSubmitActionAsync(turnContext, action, _configuration, cancellationToken);
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
