using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using MsgExtProductSupportSSOCSharp.Models;
using System.Diagnostics;
using Activity = Microsoft.Bot.Schema.Activity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace MsgExtProductSupportSSOCSharp.Bot;

public class TeamsMessageExtension : TeamsActivityHandler
{
    private readonly string connectionName;
    private readonly string spoHostname;
    private readonly string spoSiteUrl;

    public TeamsMessageExtension(IConfiguration configuration)
    {
        connectionName = configuration["CONNECTION_NAME"];
        spoHostname = configuration["SPO_HOSTNAME"];
        spoSiteUrl = configuration["SPO_SITE_URL"];
    }

    #region TeamsActivityHandler overrides

    protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
    {
        // see if we have a token for the user
        var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
        var tokenResponse = await GetToken(userTokenClient, query.State, turnContext.Activity.From.Id, turnContext.Activity.ChannelId, connectionName, cancellationToken);

        // check to see if a token was returned from the token service
        if (!HasToken(tokenResponse))
        {
            // no token returned so ask the user to sign in and consent to the required permissions
            return await CreateAuthResponse(userTokenClient, connectionName, (Activity)turnContext.Activity, cancellationToken);
        }

        // get the query parameter values
        var initialRun = GetQueryData(query.Parameters, "initialRun");
        var name = GetQueryData(query.Parameters, "ProductName");
        var retailCategory = GetQueryData(query.Parameters, "TargetAudience");

        Debug.WriteLine($"🔍 Is initial run?: {!string.IsNullOrEmpty(initialRun)}");
        Debug.WriteLine($"🔍 Name: '{name}'");
        Debug.WriteLine($"🔍 Retail category: '{retailCategory}'");

        // define filters to be used when querying SharePoint
        var nameFilter = !string.IsNullOrEmpty(name) ? $"startswith(fields/Title, '{name}')" : string.Empty;
        var retailCategoryFilter = !string.IsNullOrEmpty(retailCategory) ? $"fields/RetailCategory eq '{retailCategory}'" : string.Empty;
        var filters = new List<string> { nameFilter, retailCategoryFilter };

        // remove any empty filters
        filters.RemoveAll(f => string.IsNullOrEmpty(f));

        // create the filter string to be used when querying SharePoint
        var filterQuery = filters.Count == 1 ? filters.FirstOrDefault() : string.Join(" and ", filters);
        Debug.WriteLine($"🔍 Filter query: {filterQuery}");

        // create a Graph client
        var graphClient = CreateGraphClient(tokenResponse);

        // get the Product Marketing site and Product items
        var site = await GetSharePointSite(graphClient, spoHostname, spoSiteUrl, cancellationToken);
        var items = await GetProducts(graphClient, site.SharepointIds.SiteId, filterQuery, cancellationToken);

        // create the adaptive card template
        var adaptiveCardJson = File.ReadAllText(@"AdaptiveCards\Product.json");
        var template = new AdaptiveCardTemplate(adaptiveCardJson);

        // get the Product Imagery drive to get the images for the Product
        var drive = await GetSharePointDrive(graphClient, site.SharepointIds.SiteId, "Product Imagery", cancellationToken);

        // create an an array of attachments to be sent in the response
        var attachments = new List<MessagingExtensionAttachment>();

        // iterate through the Product items
        foreach (var item in items.Value)
        {
            // deserialize the JSON into a Product object
            var product = JsonConvert.DeserializeObject<Product>(item.AdditionalData["fields"].ToString());
            product.Id = item.Id;

            // get the Thumbnail images the Product
            ThumbnailSet thumbnails = await GetThumbnails(graphClient, drive.Id, product.PhotoSubmission, cancellationToken);

            // render the adaptive card using template
            // shown in the message when the user selects a product in Teams UI or
            // shown as a preview when the user hovers over a reference to the product in Copilot messages
            var resultCard = template.Expand(new
            {
                Product = product,
                ProductImage = thumbnails.Large.Url,
                SPOHostname = spoHostname,
                SPOSiteUrl = spoSiteUrl,
            });

            // create the preview card
            // shown in the search results in Teams UI
            // shown in the references section of Copilot messages
            var previewcard = new ThumbnailCard
            {
                Title = product.Title,
                Subtitle = product.RetailCategory,
                Images = new List<CardImage> { new() { Url = thumbnails.Small.Url } }
            }.ToAttachment();

            // create the attachment to be sent in the response using the adaptive card and preview card
            var attachment = new MessagingExtensionAttachment
            {
                Content = JsonConvert.DeserializeObject(resultCard),
                ContentType = AdaptiveCard.ContentType,
                Preview = previewcard
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

    protected override async Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
    {
        // get data from the invoke value
        var data = JsonConvert.DeserializeObject<ActionData>(invokeValue.Action.Data.ToString());

        // get the token for the user
        var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
        var tokenResponse = await GetToken(userTokenClient, invokeValue.State, turnContext.Activity.From.Id, turnContext.Activity.ChannelId, connectionName, cancellationToken);

        // check if the user has a token
        if (!HasToken(tokenResponse))
        {
            // no token returned so ask the user to sign in
            // this typically happens when the user's token has expired or they have not yet signed in using the messaging extension in Teams
            return await CreateOAuthCardResponse(userTokenClient, connectionName, (Activity)turnContext.Activity, cancellationToken);
        }

        // create a Graph client
        var graphClient = CreateGraphClient(tokenResponse);

        // get the Product Marketing site, Product item and Product Imagery drive
        var site = await GetSharePointSite(graphClient, spoHostname, spoSiteUrl, cancellationToken);
        var product = await GetProduct(data.Id, graphClient, site.SharepointIds.SiteId, cancellationToken);
        var drive = await GetSharePointDrive(graphClient, site.SharepointIds.SiteId, "Product Imagery", cancellationToken);

        // handle the action verb
        return invokeValue.Action.Verb switch
        {
            "edit" => await HandleEditAction(graphClient, site, product, cancellationToken),
            "edit-save" => await HandleSaveAction(data, graphClient, site.SharepointIds.SiteId, data.Id, product, drive.Id, cancellationToken),
            "edit-cancel" => await HandleCancelAction(graphClient, product, drive.Id, cancellationToken),
            _ => null
        };
    }

    protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken) {
        if (action.CommandId.ToUpper() == "SIGNOUT") {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            
            await SignOut(userTokenClient, turnContext.Activity.From.Id, turnContext.Activity.ChannelId, connectionName, cancellationToken);
            
            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = new Attachment
                        {
                            Content = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
                            {
                                Body = new List<AdaptiveElement>() { new AdaptiveTextBlock() { Text = "You have been signed out." } },
                                Actions = new List<AdaptiveAction>() { new AdaptiveSubmitAction() { Title = "Close" } },
                            },
                            ContentType = AdaptiveCard.ContentType,
                        },
                        Height = 200,
                        Width = 400,
                        Title = "Signed out",
                    },
                },
            };
        }
        return null;
    }
    
    protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
    {
        return Task.FromResult(new MessagingExtensionActionResponse());
    }

    #endregion

    #region Adaptive card action handlers

    private async Task<AdaptiveCardInvokeResponse> HandleCancelAction(GraphServiceClient graphClient, Product product, string driveId, CancellationToken cancellationToken)
    {
        // get the thumbnails for the Product
        ThumbnailSet thumbnails = await GetThumbnails(graphClient, driveId, product.PhotoSubmission, cancellationToken);

        return new AdaptiveCardInvokeResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Type = AdaptiveCard.ContentType,
            Value = CreateAdaptiveCard(@"AdaptiveCards\Product.json", new
            {
                Product = product,
                ProductImage = thumbnails.Large.Url,
                SPOHostname = spoHostname,
                SPOSiteUrl = spoSiteUrl
            })
        };
    }

    private async Task<AdaptiveCardInvokeResponse> HandleSaveAction(ActionData data, GraphServiceClient graphClient, string siteId, string itemId, Product product, string driveId, CancellationToken cancellationToken)
    {
        // update the Product item and get the thumbnails
        var updatedProduct = await UpdateProduct(itemId, graphClient, siteId, data, cancellationToken);
        ThumbnailSet thumbnails = await GetThumbnails(graphClient, driveId, product.PhotoSubmission, cancellationToken);

        return new AdaptiveCardInvokeResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Type = AdaptiveCard.ContentType,
            Value = CreateAdaptiveCard(@"AdaptiveCards\Product.json", new
            {
                Product = updatedProduct,
                ProductImage = thumbnails.Large.Url,
                SPOHostname = spoHostname,
                SPOSiteUrl = spoSiteUrl
            })
        };
    }

    private static async Task<AdaptiveCardInvokeResponse> HandleEditAction(GraphServiceClient graphClient, Site site, Product product, CancellationToken cancellationToken)
    {
        // get the retail categories
        var retailCategories = await GetRetailCategories(graphClient, site.SharepointIds.SiteId, cancellationToken);

        return new AdaptiveCardInvokeResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Type = AdaptiveCard.ContentType,
            Value = CreateAdaptiveCard(
                @"AdaptiveCards\EditForm.json",
                new
                {
                    Product = product,
                    RetailCategories = retailCategories
                })
        };
    }

    #endregion

    #region Microsoft Graph helpers

    private static GraphServiceClient CreateGraphClient(TokenResponse tokenResponse)
    {
        TokenProvider provider = new() { Token = tokenResponse.Token };
        var authenticationProvider = new BaseBearerTokenAuthenticationProvider(provider);
        var graphClient = new GraphServiceClient(authenticationProvider);
        return graphClient;
    }

    private static async Task<Site> GetSharePointSite(GraphServiceClient graphClient, string hostName, string siteUrl, CancellationToken cancellationToken)
    {
        return await graphClient.Sites[$"{hostName}:/{siteUrl}"].GetAsync(r => r.QueryParameters.Select = new string[] { "sharePointIds" }, cancellationToken);
    }

    private static async Task<SiteCollectionResponse> GetProducts(GraphServiceClient graphClient, string siteId, string filterQuery, CancellationToken cancellationToken)
    {
        var fields = new string[]
        {
            "fields/Id",
            "fields/Title",
            "fields/RetailCategory",
            "fields/PhotoSubmission",
            "fields/CustomerRating",
            "fields/ReleaseDate"
        };

        var requestUrl = string.IsNullOrEmpty(filterQuery)
            ? $"https://graph.microsoft.com/v1.0/sites/{siteId}/lists/Products/items?expand={string.Join(",", fields)}"
            : $"https://graph.microsoft.com/v1.0/sites/{siteId}/lists/Products/items?expand={string.Join(",", fields)}&$filter={filterQuery}";

        Debug.WriteLine($"🔍 Request URL: {requestUrl}");
        var request = graphClient.Sites.WithUrl(requestUrl);
        return await request.GetAsync(null, cancellationToken);
    }

    private static async Task<Product> GetProduct(string itemId, GraphServiceClient graphClient, string siteId, CancellationToken cancellationToken)
    {
        var item = await graphClient.Sites[siteId].Lists["Products"].Items[itemId].GetAsync(r => r.QueryParameters.Expand = new string[] { "fields" }, cancellationToken);
        item.Fields.AdditionalData["Id"] = itemId;
        var json = JsonConvert.SerializeObject(item.Fields.AdditionalData);
        return JsonConvert.DeserializeObject<Product>(json);
    }

    private static async Task<ThumbnailSet> GetThumbnails(GraphServiceClient graphClient, string driveId, string photoUrl, CancellationToken cancellationToken)
    {
        var fileName = photoUrl.Split('/').Last();
        var driveItem = await graphClient.Drives[driveId].Root.ItemWithPath(fileName).GetAsync(null, cancellationToken);
        var thumbnails = await graphClient.Drives[driveId].Items[driveItem.Id].Thumbnails["0"].GetAsync(r => r.QueryParameters.Select = new string[] { "small", "large" }, cancellationToken);
        return thumbnails;
    }

    private static async Task<Drive> GetSharePointDrive(GraphServiceClient graphClient, string siteId, string name, CancellationToken cancellationToken)
    {
        var drives = await graphClient.Sites[siteId].Drives.GetAsync(r => r.QueryParameters.Select = new string[] { "id", "name" }, cancellationToken);
        var drive = drives.Value.Find(d => d.Name == name);
        return drive;
    }

    private static async Task<List<string>> GetRetailCategories(GraphServiceClient graphClient, string siteId, CancellationToken cancellationToken)
    {
        var column = await graphClient.Sites[siteId].Lists["Products"].Columns["RetailCategory"].GetAsync(null, cancellationToken);
        return column.Choice.Choices;
    }

    private static async Task<Product> UpdateProduct(string itemId, GraphServiceClient graphClient, string siteId, ActionData data, CancellationToken cancellationToken)
    {
        // update the Product item
        var fields = await graphClient.Sites[siteId].Lists["Products"].Items[itemId].Fields.PatchAsync(new FieldValueSet
        {
            AdditionalData = new Dictionary<string, object>
            {
                { "Title", data.Title },
                { "RetailCategory", data.RetailCategory },
                { "ReleaseDate", data.ReleaseDate.ToString("u") }
            }
        }, cancellationToken: cancellationToken);

        // add the itemId to the AdditionalData object as it's not returned
        fields.AdditionalData["Id"] = itemId;

        var json = JsonConvert.SerializeObject(fields.AdditionalData);
        return JsonConvert.DeserializeObject<Product>(json);
    }

    #endregion

    #region Authentication helpers

    private static async Task<TokenResponse> GetToken(UserTokenClient userTokenClient, string state, string userId, string channelId, string connectionName, CancellationToken cancellationToken)
    {
        var magicCode = string.Empty;

        if (!string.IsNullOrEmpty(state))
        {
            if (int.TryParse(state, out var parsed))
            {
                magicCode = parsed.ToString();
            }
        }

        return await userTokenClient.GetUserTokenAsync(userId, connectionName, channelId, magicCode, cancellationToken);
    }

    private static bool HasToken(TokenResponse tokenResponse)
    {
        return tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token);
    }

    private static async Task<MessagingExtensionResponse> CreateAuthResponse(UserTokenClient userTokenClient, string connectionName, Activity activity, CancellationToken cancellationToken)
    {
        // get the sign in resource
        var resource = await userTokenClient.GetSignInResourceAsync(connectionName, activity, null, cancellationToken);

        return new MessagingExtensionResponse
        {
            ComposeExtension = new MessagingExtensionResult
            {
                Type = "auth",
                SuggestedActions = new MessagingExtensionSuggestedAction
                {
                    Actions = new List<CardAction>
                    {
                        new() {
                            Type = ActionTypes.OpenUrl,
                            Value = resource.SignInLink,
                            Title = "Sign In",
                        },
                    },
                },
            },
        };
    }

    private static async Task<AdaptiveCardInvokeResponse> CreateOAuthCardResponse(UserTokenClient userTokenClient, string connectionName, Activity activity, CancellationToken cancellationToken)
    {
        // get the sign in resource
        var resource = await userTokenClient.GetSignInResourceAsync(connectionName, activity, null, cancellationToken);

        return new AdaptiveCardInvokeResponse
        {
            StatusCode = 401,
            Type = $"{Activity.ContentType}.loginRequest",
            Value = JObject.FromObject(new OAuthCard
            {
                Buttons = new List<CardAction>
                {
                    new() {
                        Title = "Sign In",
                        Type = ActionTypes.Signin,
                        Value = resource.SignInLink
                    }
                },
                Text = "Please sign in to continue.",
                ConnectionName = connectionName,
            })
        };
    }

    private static async Task SignOut(UserTokenClient userTokenClient, string userId, string channelId, string connectionName, CancellationToken cancellationToken)
    {
        await userTokenClient.SignOutUserAsync(userId, connectionName, channelId, cancellationToken);
    }

    #endregion

    #region Adaptive card helpers

    private static object CreateAdaptiveCard(string path, object data)
    {
        // read the adaptive card template from the file system
        var templateText = File.ReadAllText(path);
        // create the adaptive card template
        var template = new AdaptiveCardTemplate(templateText);
        // render the adaptive card using template and data
        var result = template.Expand(data);

        return JsonConvert.DeserializeObject(result);
    }

    #endregion

    #region Message extension helpers

    private static string GetQueryData(IList<MessagingExtensionParameter> parameters, string key)
    {
        // if no parameters were passed in then return an empty string
        if (parameters.Any() != true)
        {
            return string.Empty;
        }

        // find the parameter with the specified key and return the value
        var foundPair = parameters.FirstOrDefault(pair => pair.Name == key);
        return foundPair?.Value?.ToString() ?? string.Empty;
    }

    #endregion
}
