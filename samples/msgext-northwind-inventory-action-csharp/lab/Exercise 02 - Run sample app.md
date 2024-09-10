# Building Message Extensions for Microsoft Copilot for Microsoft 365

TABLE OF CONTENTS

* [Welcome](./Exercise%2000%20-%20Welcome.md) 
* [Exercise 1](./Exercise%2001%20-%20Set%20up.md) - Set up your development Environment 
* Exercise 2 - Run the sample as a Message Extension **(THIS PAGE)**
* [Exercise 3](./Exercise%2003%20-%20Run%20in%20Copilot.md) - Run the sample as a Copilot plugin
* [Exercise 5](./Exercise%2005%20-%20Code%20tour.md) - Code tour

## Exercise 2 - Run sample app

## Step 1 - Set up the project for first use

Open your working folder in Visual Studio.

Teams Toolkit stores environment variables in the **env** folder, and it will fill in all the values automatically to **appsettings.json** when you start your project the first time. However there's one value that's specific to the sample application, and that's the connection string for accessing the Northwind database.

In this project, the Northwind database is stored in Azure Table Storage; when you're debugging locally, it uses the [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage#running-azurite-from-an-aspnet-project) storage emulator. That's mostly built into the project, but the project won't build unless you provide the connection string.

The necessary setting is provided in a file **appsettings.sample.json** under msgext-northwind-inventory-action-csharp project. Copy the **"StorageConnectionString": "UseDevelopmentStorage=true"** value from this file, and add it to **appsettings.json**. This is where secret or sensitive settings are stored.

The resulting **appsettings.json** file should be similar to the one below:

~~~text
{
    "MicrosoftAppType": "{Some value}",
    "MicrosoftAppId": "{Some value}",
    "MicrosoftAppPassword": "{Some value}",
    "MicrosoftAppTenantId": "{Some value}",
    "StorageConnectionString": "UseDevelopmentStorage=true"
}
~~~

- Running Azurite from an ASP.NET project

Right-click on "Connected Services" in Solution Explorer and select **Add** from the dropdown menu. Choose **Azure Storage**.

![Running Azurite](./images/02-01-Running-Azurite-01.png)

Select service dependency **Storage Azurite emulator(local)** and click **Next**.

![Running Azurite](./images/02-01-Running-Azurite-02.png)

Provide connection string name as **StorageConnectionString** and click **Finish**.

![Running Azurite](./images/02-01-Running-Azurite-03.png)

When the configuration completes, select Close, and the Azurite emulator starts automatically. The output looks similar to the following screenshot.

![Running Azurite](./images/02-01-Running-Azurite-04.png)

- Enable Multi-Project Launch Profiles

Click the "Debug" menu in Visual Studio and then select "Options".

![Enable Multi-Project](./images/02-01-Enable-multiprojects-01.png)

Navigate to Environment > Preview Features and check the checkbox labeled "Enable Multi-Project Launch Profiles".

![Enable Multi-Project](./images/02-01-Enable-multiprojects-02.png)

## Step 2 - Run the application locally

Click F5 to start debugging, or click the start button 1️⃣. Make sure that the debug profile is **Microsoft Teams (browser)** 2️⃣.

![Run application locally](./images/01-04-debug-dropdown.png)

> Note : Running this sample for the first time may prompt you to trust the ASP.NET Core SSL Certificate. In that case, click "Yes".

![SSL Certificate](./images/02-01-SSL-Certificate-01.png)

Once you're in, Microsoft Teams should open up and display a dialog offering to install your application.
Take note of the information displayed; which is from the [app manifest](../TeamsApp/appPackage/manifest.json).

Click "Add" to add Northwind Inventory as a personal application.

![App installation screen with large Add button](./images/02-02-Run-Project-04.png)

You should be directed to a chat within the application, however you could use the app in any chat.

## Step 3 - Test in Microsoft Teams

In any Teams chat - including the Northwind Inventory chat - begin typing a message 1️⃣ that refers to a product. Then, to insert an adaptive card for the product, click the + 2️⃣ . In the fly-up panel, select the Northwind Inventory application you just installed 3️⃣ .

![Clicking the "+" to open the message extension panel](./images/02-03-TestME-Teams-take2-01.png)

You will see a search dialog with 3 tabs 1️⃣. The "Product Inventory" tab allows searching for products by name; for now, just type a product name, or the beginning of a product name such as "chai" into the search box 2️⃣ . If you pause as you type the first letters, you'll be given more choices of products that begin with the same characters.

Click Chai 3️⃣ to insert an adaptive card into the conversation along with your comment.

![Selecting Chai from the results](./images/02-03-TestME-Teams-take2-02.png)

You can see the card, but you can't use it until you send it. Make any final edits to your message and click "Send".

![Sending the card](./images/02-03-TestME-Teams-take2-03.png)

> NOTE: Card actions won't work until you send the card. If you get an error, please check and make sure you've sent the message and are working with the card after it has been sent.

> NOTE: This is really only useful in conversations with other users; the chat in the Northwind Inventory app is just for testing.

Notice that there is no chai on order 1️⃣ . The authors of this lab are big chai drinkers and they may be stopping by so you'd better order more! Click the "take action" button 2️⃣ to open a child card. Enter a quantity 3️⃣ and click the "Restock" button 4️⃣ . The card will refresh with (hopefully) a success message and an updated number of units on order.

![Update the quantity of Chai in the adaptive card](./images/02-03-TestME-Teams-take2-04.png)

You can cancel the order or modify the stock levels using the other two buttons.

## Step 4 - Advanced queries

Back in Visual Studio, have a look at the app manifest, which is in a file called **manifest.json** in the **appPackage** directory. You'll notice that the app information that was displayed when you installed the app is all here.

Scroll down a bit to `composeExtensions:`.
Compose extensions is the historical name for a message extension; the Northwind Inventory message extension is defined here.

Here's an abbreviated version of that JSON for easy reference.

~~~json
"composeExtensions": [
    {
        "botId": "${{BOT_ID}}",
        "commands": [
            {
                "id": "inventorySearch",
                ...
                "description": "Search products by name, category, inventory status, supplier location, stock level",
                "title": "Product inventory",
                "type": "query",
                "parameters": [ ... ]
            },
            {
                "id": "discountSearch",
                ...
                "description": "Search for discounted products by category",
                "title": "Discounts",
                "type": "query",
                "parameters": [ ...]
            },
            {
                "id": "revenueSearch",
                ...
                "id": "revenueSearch",
                "description": "Find products based on their revenue/period",
                "title": "Revenue",
                "type": "query",
                "parameters": [ ... ]
            }
        ]
    }
],
~~~

Notice first that there's a bot ID. That's because Microsoft Teams uses the Azure bot channel to exchange secure, real-time messages with your application. Teams Toolkit will register the bot and fill in the ID for you.

Then there's a collection of commands. These correspond to the tabs in the search dialog in Teams. In this application, the commands are really intended for Copilot more than regular users!

You already ran the first command when you searched for a product by name. You can try the other two now.

Enter "Beverages", "Dairy", or "Produce" into the "Discounts" tab, and you'll see the products within those categories that are discounted. Copilot will use this to answer questions about discounted products.

![Searching for beverages under the discount tab](./images/02-03-Test-Multi-02.png)

Enter "high", "low", "0-10000", or "100000-" in the Revenue tab. It works, but it's not very user friendly. We're counting on Copilot to look at the manifest and figure out how to use it:

~~~json
{
    "id": "revenueSearch",
    "context": [
        "compose",
        "commandBox"
    ],
    "description": "Find products based on their revenue/period",
    "title": "Revenue",
    "type": "query",
    "parameters": [
        {
            "name": "revenueRange",
            "title": "Revenue range",
            "description": "Enter 'high' or 'low' or enter a range of integers such as 0-10000 or 5000- using this exact format",
            "inputType": "text"
        }
    ]
}
~~~

![Searching for high on the revenue tab](./images/02-03-Test-Multi-03.png)

The parameter description succinctly explains how the queries should be written, and Copilot will read this to answer prompts calling for high revenue products, or products with less than 10,000 items.

Now examine the first command again. You'll notice it has 5 parameters!

~~~json
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
~~~

Unfortunately, Teams can only display the first parameter, but Copilot can use all 5. This will allow it to do more advanced queries of the Northwind inventory data.

As a work-around to the Teams UI limitation, in the "Northwind Inventory" tab, the application will accept up to 5 parameters comma separated, using the format:

~~~text
name,category,inventoryStatus,supplierCity,supplierName
~~~

![Entering multiple comma separated fields into the Northwind Inventory tab](./images/02-03-Test-Multi-04.png)

Read the descriptions in the JSON above carefully when entering a query. Try entering these, and as you do, keep an eye on the debug console tab in Visual Studio, where you'll see each query as it's run.

* "chai" - find products with names that begin with "chai
* "c,bev" - find products in categories beginning with "bev" and names that begin with "c
* ",,out" - find products that are out of stock
* ",,on,london" - find products that are on order from suppliers in London
* "tofu,produce,,osaka" - find products in the "produce" category with suppliers in Osaka and names that begin with "tofu"

Each query term filters the list of products down. The format of each query term is arbitrary - just be sure to explain it to Copilot in the description of each parameter.

## Step 5 - Test in Microsoft Outlook (Optional)

Let's take a brief detour so you can see how message extensions work in Microsoft Outlook.

First, click the "Waffle" menu 1️⃣ and select "Outlook" 2️⃣ .

![Opening Outlook for Microsoft 365](./images/02-04-Test-ME-Outlook-01.png)

Click the "New mail" button to begin composing a message.

![Creating a new email in Outlook](./images/02-04-Test-ME-Outlook-02.png)

Add a recipient 1️⃣ and a subject 2️⃣ , and position your cursor in the body of the message 3️⃣ . You can even type something. When you're ready, click the app button in the toolbar 4️⃣ .

![Clicking the App button while composing an Outlook message](./images/02-04-Test-ME-Outlook-03.png)

Select the Northwind Inventory application, searching if necessary to find it.

![Clicking the Northwind Inventory application](./images/02-04-Test-ME-Outlook-04.png)

Now you can search for "Chai" 1️⃣ as before and select it to insert the adaptive card 2️⃣ .

![Entering a search for Chai](./images/02-04-Test-ME-Outlook-05.png)

Note that the adaptive card won't work until you send the message. If the recipient doesn't have Microsoft Outlook, they won't be able to view the card, and if they don't have the Northwind Inventory app installed, they won't be able to take action on it.

![Taking action on a message in Outlook](./images/02-04-Test-ME-Outlook-07a.png)

## Step 6 (optional) - View Northwind database in Azure Storage Explorer

The Northwind Database isn't fancy but it's real! If you want to peek at or even modify the data, open the Azure Storage Explorer while Azurite is running. (Running the app starts Azurite automatically).

To view the Northwind data, open "Emulator & Attached", "Storage Accounts", "Emulator - Default Ports", and "Tables". There you'll see the old Northwind database tables, which aren't nearly as handy in a NO SQL environment, but there they are!

![Azure Storage Explorer showing the Northwind database tables](./images/02-06-AzureStorageExplorer-01.png)

The code reads the Products table on each query, but the other tables are only accessed when the app starts. So if you want to add a new category, you'll need to restart the app for it to appear.

## Congratulations

You have completed Exercise 2.
Please proceed to [Exercise 3](./Exercise%2003%20-%20Run%20in%20Copilot.md) in which you will run the sample application in Microsoft Copilot for Microsoft 365!
