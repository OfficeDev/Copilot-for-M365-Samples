# Building Message Extensions for Microsoft Copilot for Microsoft 365

TABLE OF CONTENTS

* [Welcome](./Exercise%2000%20-%20Welcome.md) 
* [Exercise 1](./Exercise%2001%20-%20Set%20up.md) - Set up your development Environment 
* Exercise 2 - Run the sample as a Message Extension **(THIS PAGE)**
* [Exercise 3](./Exercise%2003%20-%20Run%20in%20Copilot.md) - Run the sample as a Copilot plugin
* [Exercise 4](./Exercise%2004%20-%20Add%20a%20new%20command.md) - Add a new command
* [Exercise 5](./Exercise%2005%20-%20Code%20tour.md) - Code tour

## Exercise 2 - Run the sample as a Message Extension

## Step 1 - Run the application locally

Press **CTRL+Shift+P** to open the command box and enter **Python: Create Environment** to create and activate your desired virtual environment. 

![Create Environment](./images/02-02-Create-Environment-01.png)

Select **Venv** from the search results.

![Select Venv](./images/02-02-Select-Venv-01.png)

Select the version of Python that is installed on the machine.

![Select Python version](./images/02-02-Select-Python-version-01.png)

When creating the virtual environment, make sure to select `requirements.txt` as the source for installing dependencies. Please wait until all dependencies are fully installed before proceeding with any further steps.

![Select requirements](./images/02-02-Select-requirements.png)

When running the app for the first time using F5, Teams Toolkit will prompt you to sign in to an account with access to an Azure subscription (if you are not already logged in)

![Toolkit signin](./images/02-02-Toolkit-signin.png)

After signing in, you will need to select a subscription and either create a new resource group or select an existing one within that subscription.

![Select Subscription](./images/02-02-Select-Subscription.png)

![Select resource](./images/02-02-Select-resource.png)

if you want to use a different browser, open the Run and Debug panel from the Primary Side Bar, select a different debug profile in the dropdown list and press F5 or select the Start Debugging button.

![Run application locally](./images/02-02-Run-Project-01.png)

Eventually, a browser window will open and invite you to log in.

![Browser window opens with a login form](./images/02-02-Run-Project-03.png)

Once you're in, Microsoft Teams should open up and display a dialog offering to install your application.
Take note of the information displayed; which is from the [app manifest](../appPackage/manifest.json).

Click "Add" to add Northwind Inventory as a personal application.

![App installation screen with large Add button](./images/02-02-Run-Project-04.png)

You should be directed to a chat within the application, however you could use the app in any chat.

## Step 2 - Test in Microsoft Teams

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

## Step 3 - Advanced queries

Back in Visual Studio Code, have a look at the app manifest, which is in a file called **manifest.json** in the **appPackage** directory. You'll notice that the app information that was displayed when you installed the app is all here.

Scroll down a bit to `composeExtensions:`.
Compose extensions is the historical name for a message extension; the Northwind Inventory message extension is defined here.

Here's an abbreviated version of that JSON for easy reference.

~~~json
"composeExtensions": [
    {
        "botId": "${{AAD_APP_CLIENT_ID}}",
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
            }
        ]
    }
],
~~~

Notice first that there's a  AAD_APP_CLIENT_ID, which is the bot ID. That's because Microsoft Teams uses the Azure bot channel to exchange secure, real-time messages with your application. Teams Toolkit will register the bot and fill in the ID for you.

Then there's a collection of commands. These correspond to the tabs in the search dialog in Teams. In this application, the commands are really intended for Copilot more than regular users!

You already ran the first command when you searched for a product by name. You can try the other one now.

Enter "Beverages", "Dairy", or "Produce" into the "Discounts" tab, and you'll see the products within those categories that are discounted. Copilot will use this to answer questions about discounted products.

![Searching for beverages under the discount tab](./images/02-03-Test-Multi-02.png)

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

Read the descriptions in the JSON above carefully when entering a query. Try entering these, and as you do, keep an eye on the debug console tab in Visual Studio Code, where you'll see each query as it's run.

* "chai" - find products with names that begin with "chai
* "c,bev" - find products in categories beginning with "bev" and names that begin with "c
* ",,out" - find products that are out of stock
* ",,on,london" - find products that are on order from suppliers in London
* "tofu,produce,,osaka" - find products in the "produce" category with suppliers in Osaka and names that begin with "tofu"

Each query term filters the list of products down. The format of each query term is arbitrary - just be sure to explain it to Copilot in the description of each parameter.

## Step 4 - Test in Microsoft Outlook (Optional)

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

## Step 5 (optional) - View Northwind database in Azure Storage Explorer

The Northwind Database isn't fancy but it's real! If you want to peek at or even modify the data, open the Azure Storage Explorer while Azurite is running. (Running the app starts Azurite automatically).

To view the Northwind data, open "Emulator & Attached", "Storage Accounts", "Emulator - Default Ports", and "Tables". There you'll see the old Northwind database tables, which aren't nearly as handy in a NO SQL environment, but there they are!

![Azure Storage Explorer showing the Northwind database tables](./images/02-06-AzureStorageExplorer-01.png)

The code reads the Products table on each query, but the other tables are only accessed when the app starts. So if you want to add a new category, you'll need to restart the app for it to appear.

## Congratulations

You have completed Exercise 2.
Please proceed to [Exercise 3](./Exercise%2003%20-%20Run%20in%20Copilot.md) in which you will run the sample application in Microsoft Copilot for Microsoft 365!
