# Building Message Extensions for Microsoft Copilot for Microsoft 365

TABLE OF CONTENTS

* [Welcome](./Exercise%2000%20-%20Welcome.md) 
* [Exercise 1](./Exercise%2001%20-%20Set%20up.md) - Set up your development Environment 
* [Exercise 2](./Exercise%2002%20-%20Run%20sample%20app.md) - Run the sample as a Message Extension
* [Exercise 3](./Exercise%2003%20-%20Run%20in%20Copilot.md) - Run the sample as a Copilot plugin
* [Exercise 4](./Exercise%2004%20-%20Add%20a%20new%20command.md) - Add a new command
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

Now open the file **bot/search_bot.py**. This application contains the "bot" code, which communicates with the Azure Bot Framework using the [Bot Builder SDK](https://learn.microsoft.com/azure/bot-service/index-bf-sdk?view=azure-bot-service-4.0).

Notice that the bot extends an SDK class **TeamsActivityHandler**.

~~~python
  class SearchBot(TeamsActivityHandler):
    def __init__(self):
        super().__init__()
        
    async def on_teams_messaging_extension_query(
        self, context: TurnContext, query: MessagingExtensionQuery
    ):
  ...
~~~

By overriding the methods of the **TeamsActivityHandler**, the application is able to handle messages (called "activities") coming from Microsoft 365.

The first of these is a Messaging Extension Query activity ("messaging extension" is another historical name for a message extension). This function is called when a user types into a message extension or when Copilot calls it.

~~~python
async def handle_teams_messaging_extension_query(self, context: TurnContext, query: MessagingExtensionQuery) -> MessagingExtensionResponse:
    if query.command_id == product_search_command.COMMAND_ID:
        return await product_search_command.handle_teams_messaging_extension_query(context, query)
    elif query.command_id == discounted_search_command.COMMAND_ID:
        return await discounted_search_command.handle_teams_messaging_extension_query(context, query)
    else:
        return MessagingExtensionResponse(
            compose_extension={
                "type": "message",
                "text": "Unknown command"
            }
        )
~~~

All it's doing is dispatching the query to the based on the command ID. These are the same command ID's used in the manifest above.

The other type of activity our app needs to handle is the adaptive card actions, such as when a user clicks on "Update stock" or "Reorder" on an adaptive card. we have specific method for an adaptive card action, the code overrides `on_adaptive_card_invoke()`, which is a much broader class of activity that includes message extension queries. For that reason, the code manually checks the activity verb, and dispatches to the appropriate handler. If the activity verb isn't for an adaptive card action, the `else` clause runs the base implementation of `InvokeResponse`.

~~~python
from botbuilder.core import TurnContext, InvokeResponse, CardFactory
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema.teams import MessagingExtensionQuery, MessagingExtensionResponse, MessagingExtensionAttachment, MessagingExtensionResult
from message_extensions import product_search_command, discounted_search_command
from adaptive_cards import card_handler, utils, cards

class SearchBot(TeamsActivityHandler):
    def __init__(self):
        super().__init__()
        
    async def on_teams_messaging_extension_query(
        self, context: TurnContext, query: MessagingExtensionQuery
    ):
        query_data = context.activity.value
        command_id = query_data.get("commandId")
        parameters = query_data.get("parameters", [])
        query_options = query_data.get("queryOptions", {})

        # Construct the MessagingExtensionQuery object
        query = MessagingExtensionQuery(
            command_id=command_id,
            parameters=parameters,
            query_options=query_options
        )

        response = await self.handle_teams_messaging_extension_query(context, query)
        return response
    
    async def handle_teams_messaging_extension_query(self, context: TurnContext, query: MessagingExtensionQuery) -> MessagingExtensionResponse:
        if query.command_id == product_search_command.COMMAND_ID:
            return await product_search_command.handle_teams_messaging_extension_query(context, query)
        elif query.command_id == discounted_search_command.COMMAND_ID:
            return await discounted_search_command.handle_teams_messaging_extension_query(context, query)
        else:
            return MessagingExtensionResponse(
                compose_extension={
                    "type": "message",
                    "text": "Unknown command"
                }
            )

    async def on_adaptive_card_invoke(self, context: TurnContext, value) -> InvokeResponse:
        try:
            verb = context.activity.value['action']['verb']
            if verb == 'ok':
                return await card_handler.handle_teams_card_action_update_stock(context)
            elif verb == 'restock':
                return await card_handler.handle_teams_card_action_restock(context)
            elif verb == 'cancel':
                return await card_handler.handle_teams_card_action_cancel_restock(context)
            else:
                return utils.create_action_error_response(200, 0, f'ActionVerbNotSupported: {verb} is not a supported action verb.')
        except Exception as e:
            return utils.create_action_error_response(500, 0, str(e))
    ...
~~~

## Step 3 - Examine the message extension command code

In an effort to make the code more modular, readable, and reusable, each message extension command has been placed in its own TypeScript module. Have a look at **\message_extensions\discounted_search_command.py** as an example.

First, note that the module exports a constant `COMMAND_ID`, which contains the same command ID found in the app manifest, and allows the switch statement in **Search_bot.py** to work properly.

Then it provides a function, `handle_teams_messaging_extension_query()`, to handle incoming queries for discounted products by category.

~~~python
async def handle_teams_messaging_extension_query(context: TurnContext, query: MessagingExtensionQuery) -> MessagingExtensionResponse:
    global query_count

    # Seek the parameter by name, don't assume it's in element 0 of the array
    category_name = cleanup_param(next((param.get("value") for param in query.parameters if param.get("name") == 'categoryName'), None))

    print(f'💰 Discount query #{query_count + 1}: Discounted products with categoryName={category_name}')
    query_count += 1

    products = await get_discounted_products_by_category(category_name)

    print(f'Found {len(products)} products in the Northwind database')

    attachments = []
    for obj in products:
        hero_card = HeroCard(
            title=obj["ProductName"], tap=CardAction(type="invoke", value=obj),
            text=f'Avg discount {obj["AverageDiscount"]}%<br />Supplied by {obj["SupplierName"]} of {obj["SupplierCity"]}',
            images=[CardImage(url=obj["ImageUrl"])]
        )

        attachment = MessagingExtensionAttachment(
            content_type=CardFactory.content_types.adaptive_card,
            content=cards.edit_card(obj),
            preview=CardFactory.hero_card(hero_card),
        )
        attachments.append(attachment)
    return MessagingExtensionResponse(
        compose_extension=MessagingExtensionResult(
            type="result", attachment_layout="list", attachments=attachments
        )
    )

def cleanup_param(value: str) -> str:
    if not value:
        return ''
    result = value.strip()
    result = result.split(',')[0]  
    result = result.replace('*', '')
    return result
~~~

Notice that the index in the `query.Parameters` array may not correspond to the parameter's position in the manifest. While this is generally only an issue for a multi-parameter command, the code will still get the value based on the parameter name rather than hard coding an index.
After cleaning up the parameter (trimming it, and handling the fact that sometimes Copilot assumes "*" is a wildcard that matches everything), the code calls the Northwind data access layer to `get_discounted_products_by_category()`.

Then it iterates through the products and creates two cards for each:

* a _preview_ card, which is implemented as a "hero" card (these predate adaptive cards and are very simple). This is what's displayed in the search results in the user interface and in some citations in Copilot.
* a _result_ card, which is implemented as an "adaptive" card that includes all the details.

In the next step, we'll review the adaptive card code and check out the Adaptive Card designer.

## Step 4 - Examine the adaptive cards and related code

The project's adaptive cards are in the **/adaptive_cards/cards.py** folder. There are 2 cards, each implemented as a JSON file.

* **edit_card** - This is the initial card that's displayed by the message extension or a Copilot reference
* **success_card** - When a user takes action, this card is displayed to indicate success. It's mostly the same as the edit card except it includes a message to the user.

In **/adaptive_cards/card_handler.py**, you'll see the handler for each of the action buttons on the card. The card submits data when an action button is clicked - specifically `data["txtStock"]`, which is the "Quantity" input box on the card, and `data["productId"]`, which is sent in each card action to let the code know what product to update.

~~~python
async def handle_teams_card_action_update_stock(context: TurnContext):
    request = context.activity.value
    data = request['action']['data']
    print(f'🎬 Handling update stock action, quantity={data["txtStock"]}')

    if data["txtStock"] and data["productId"]:
        product = await get_product_ex(data["productId"])
        product["UnitsInStock"] = int(data["txtStock"])
        await update_product(product)
        message = f"Stock updated for {product["ProductName"]} to {product["UnitsInStock"]}!"
        edit_card = cards.success_card(product, message)
        return create_adaptive_card_invoke_response(200, edit_card)
    else:
        return create_action_error_response(400, 0, "Invalid request")
~~~

As you can see, the code obtains these two values, updates the database, and then sends a new card that contains a message and the updated data.

## Congratulations

You have completed Exercise 5 and the Copilot for Microsoft 365 Message Extensions plugin lab. Thanks very much for doing these labs!