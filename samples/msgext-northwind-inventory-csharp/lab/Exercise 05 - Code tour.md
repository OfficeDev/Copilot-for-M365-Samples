# Building Message Extensions for Microsoft Copilot for Microsoft 365

TABLE OF CONTENTS

* [Welcome](./Exercise%2000%20-%20Welcome.md) 
* [Exercise 1](./Exercise%2001%20-%20Set%20up.md) - Set up your development Environment 
* [Exercise 2](./Exercise%2002%20-%20Run%20sample%20app.md) - Run the sample as a Message Extension
* [Exercise 3](./Exercise%2003%20-%20Run%20in%20Copilot.md) - Run the sample as a Copilot plugin
* [Exercise 4](./Exercise%2003%20-%20Add%20a%20new%20command.md) - Add a new command
* Exercise 5 - Code tour **(THIS PAGE)**

## Exercise 5 - Code tour

In this exercise, you'll review the application code so that you can understand how a Message Extension works.

## Step 1 - Examine the manifest

The core of any Microsoft 365 application is its application manifest. This is where you provide the information Microsoft 365 needs to access your application.

In your working directory, open the [manifest.json](https://github.com/OfficeDev/Copilot-for-M365-Plugins-Samples/blob/main/samples/msgext-northwind-inventory-ts/appPackage/manifest.json) file. This JSON file is placed in a zip archive with two icon files to create the application package. The "icons" property includes paths to these icons.

~~~json
"icons": {
    "color": "Northwind-Logo3-192-${{TEAMSFX_ENV}}.png",
    "outline": "Northwind-Logo3-32.png"
},
~~~

Notice the token `${{TEAMSFX_ENV}}` in one of the icon names. Teams Toolkit will replace this token with your environment name, such as "local" or "dev" (for an Azure deployment in development). Thus, the icon color will change depending on the environment.

Now have a look at the "name" and "description". Notice that the description is quite long! This is important so both users and Copilot can learn what your application does and when to use it.

~~~json
    "name": {
        "short": "Northwind Inventory",
        "full": "Northwind Inventory App"
    },
    "description": {
        "short": "App allows you to find and update product inventory information",
        "full": "Northwind Inventory is the ultimate tool for managing your product inventory. With its intuitive interface and powerful features, you'll be able to easily find your products by name, category, inventory status, and supplier city. You can also update inventory information with the app. \n\n **Why Choose Northwind Inventory:** \n\n Northwind Inventory is the perfect solution for businesses of all sizes that need to keep track of their inventory. Whether you're a small business owner or a large corporation, Northwind Inventory can help you stay on top of your inventory management needs. \n\n **Features and Benefits:** \n\n - Easy Product Search through Microsoft Copilot. Simply start by saying, 'Find northwind dairy products that are low on stock' \r - Real-Time Inventory Updates: Keep track of inventory levels in real-time and update them as needed \r  - User-Friendly Interface: Northwind Inventory's intuitive interface makes it easy to navigate and use \n\n **Availability:** \n\n To use Northwind Inventory, you'll need an active Microsoft 365 account . Ensure that your administrator enables the app for your Microsoft 365 account."
    },
~~~

Scroll down a bit to "composeExtensions". Compose extension is the historical term for message extension; this is where the app's message extensions are defined.

Within this is a bot, with the ID supplied by Teams Toolkit.

~~~json
    "composeExtensions": [
        {
            "botId": "${{AAD_APP_CLIENT_ID}}",
            "commands": [
                {
                    ...
~~~

Message extensions communicate using the Azure Bot Framework; this provides a fast and secure communication channel between Microsoft 365 and your application. When you first ran your project, Teams Toolkit registered a bot, and will place its bot ID here.

This message extension has two commands, which are defined in the `commands` array. If you have completed [Exercise 4](./Exercise%2004%20-%20Add%20a%20new%20command.md), there will be also a third one. Let's skip the first command for a moment since it's the most complex one. The second command looks like this

~~~json
{
    "id": "discountSearch",
    "context": [
        "compose",
        "commandBox"
    ],
    "description": "Search for discounted products by category",
    "title": "Discounts",
    "type": "query",
    "parameters": [
        {
            "name": "categoryName",
            "title": "Category name",
            "description": "Enter the category to find discounted products",
            "inputType": "text"
        }
    ]
},
~~~

This allows Copilot (or a user) to search for discounted products within a Northwind category. This command accepts a single parameter, "categoryName". 

OK now let's move back to the first command, "inventorySearch". It has 5 parameters, which allows for much more sophisticated queries.

~~~json
{
    "id": "inventorySearch",
    "context": [
        "compose",
        "commandBox"
    ],
    "description": "Search products by name, category, inventory status, supplier location, stock level",
    "title": "Product inventory",
    "type": "query",
    "parameters": [
        {
            "name": "productName",
            "title": "Product name",
            "description": "Enter a product name here",
            "inputType": "text"
        },
        {
            "name": "categoryName",
            "title": "Category name",
            "description": "Enter the category of the product",
            "inputType": "text"
        },
        {
            "name": "inventoryStatus",
            "title": "Inventory status",
            "description": "Enter what status of the product inventory. Possible values are 'in stock', 'low stock', 'on order', or 'out of stock'",
            "inputType": "text"
        },
        {
            "name": "supplierCity",
            "title": "Supplier city",
            "description": "Enter the supplier city of product",
            "inputType": "text"
        },
        {
            "name": "stockQuery",
            "title": "Stock level",
            "description": "Enter a range of integers such as 0-42 or 100- (for >100 items). Only use if you need an exact numeric range.",
            "inputType": "text"
        }
    ]
},
~~~

Copilot is able to fill these in, again based on the descriptions, and the message extension will return a list of products filtered by all the non-blank parameters.

## Step 2 - Examine the "Bot" code

Now open the file **Bot/SearchBot.cs**. This application contains the "bot" code, which communicates with the Azure Bot Framework using the [Bot Builder SDK](https://learn.microsoft.com/azure/bot-service/index-bf-sdk?view=azure-bot-service-4.0).

Notice that the bot extends an SDK class **TeamsActivityHandler**.

~~~csharp
  public class SearchBot : TeamsActivityHandler
  {
      private readonly ILogger<SearchBot> _logger;
      private readonly IConfiguration _configuration;

      public SearchBot(ILogger<SearchBot> logger, IConfiguration configuration)
      {
          _logger = logger;
          _configuration = configuration;
      }
  }
  ...
~~~

By overriding the methods of the **TeamsActivityHandler**, the application is able to handle messages (called "activities") coming from Microsoft 365.

The first of these is a Messaging Extension Query activity ("messaging extension" is another historical name for a message extension). This function is called when a user types into a message extension or when Copilot calls it.

~~~csharp
  // Handle search message extension
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
~~~

All it's doing is dispatching the query to the based on the command ID. These are the same command ID's used in the manifest above.

The other type of activity our app needs to handle is the adaptive card actions, such as when a user clicks on "Update stock" or "Reorder" on an adaptive card. Since there is no specific method for an adaptive card action, the code overrides `OnInvokeActivityAsync()`, which is a much broader class of activity that includes message extension queries. For that reason, the code manually checks the activity name, and dispatches to the appropriate handler. If the activity name isn't for an adaptive card action, the `else` clause runs the base implementation of `OnInvokeActivityAsync()` which, among other things, will call our `HandleTeamsMessagingExtensionQueryAsync()` method if the Invoke activity is a query.

~~~csharp
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
        }...
~~~

## Step 3 - Examine the message extension command code

In an effort to make the code more modular, readable, and reusable, each message extension command has been placed in its own TypeScript module. Have a look at **\MessageExtensions\DiscountedSearchCommand.cs** as an example.

First, note that the module exports a constant `COMMAND_ID`, which contains the same command ID found in the app manifest, and allows the switch statement in **SearchBot.cs** to work properly.

Then it provides a function, `HandleTeamsMessagingExtensionQueryAsync()`, to handle incoming queries for discounted products by category.

~~~csharp
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
~~~

Notice that the index in the `query.Parameters` array may not correspond to the parameter's position in the manifest. While this is generally only an issue for a multi-parameter command, the code will still get the value based on the parameter name rather than hard coding an index.
After cleaning up the parameter (trimming it, and handling the fact that sometimes Copilot assumes "*" is a wildcard that matches everything), the code calls the Northwind data access layer to `GetDiscountedProductsByCategory()`.

Then it iterates through the products and creates two cards for each:

* a _preview_ card, which is implemented as a "hero" card (these predate adaptive cards and are very simple). This is what's displayed in the search results in the user interface and in some citations in Copilot.
* a _result_ card, which is implemented as an "adaptive" card that includes all the details.

In the next step, we'll review the adaptive card code and check out the Adaptive Card designer.

## Step 4 - Examine the adaptive cards and related code

The project's adaptive cards are in the **src/adaptiveCards** folder. There are 3 cards, each implemented as a JSON file.

* **editCard.json** - This is the initial card that's displayed by the message extension or a Copilot reference
* **successCard.json** - When a user takes action, this card is displayed to indicate success. It's mostly the same as the edit card except it includes a message to the user.
* **errorCard.json** - If an action fails, this card is displayed.

Let's take a look at the edit card in the Adaptive Card Designer. Open your web browser to [https://adaptivecards.io](https://adaptivecards.io) and click the "Designer" option at the top.

![image](./images/05-01-AdaptiveCardDesigner-01.png)

Notice the data binding expressions such as `"text": "📦 ${productName}",`. This binds the `productName` property in the data to the text on the card.

Now select "Microsoft Teams" as the host application 1️⃣ . Paste the entire contents of **editCard.json** into the Card Payload Editor 2️⃣ , and the contents of **sampleData.json** into the Sample Data Editor 3️⃣ . The sample data is identical to a product as provided in the code.

![image](./images/05-01-AdaptiveCardDesigner-02.png)

You should see the card as rendered, except for a small error which arises due to the designer's inability to display one of the adaptive card formats.

Near the top of the page, try changing the Theme and Emulated Device to see how the card would look in dark theme or on a mobile device. This is the tool that was used to build adaptive cards for the sample application.

Now, back in Visual Studio, open **CardHandler.cs**. The function `GetEditCard()` is called from each of the message extension commands to obtain a result card. The code reads the adaptive card JSON - which is considered a template - and then binds it to product data. The result is more JSON - the same card as the template, with the data binding expressions all filled in. Finally, the JSON converted into an adaptive card attachment object for rendering.

~~~csharp
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
~~~

Scrolling down, you'll see the handler for each of the action buttons on the card. The card submits data when an action button is clicked - specifically `data?["txtStock"]?.ToString()`, which is the "Quantity" input box on the card, and `data?["productId"]?.ToString()`, which is sent in each card action to let the code know what product to update.

~~~csharp
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
~~~

As you can see, the code obtains these two values, updates the database, and then sends a new card that contains a message and the updated data.

## Congratulations

You have completed Exercise 5 and the Copilot for Microsoft 365 Message Extensions plugin lab. Thanks very much for doing these labs!
