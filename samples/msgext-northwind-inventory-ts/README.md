# Setup notes ---

## First time

1. To enhance some of the scenarios, copy the **sampleDocs** directory to your OneDrive. Choose a location where whatever account you're using to test can access the docs, but others can't (to avoid confusion with other testers).

2. Ensure you have installed node.js 18.x. If you're starting fresh, I suggest installing nvm (search "nvm for Windows" if you're on Windows, nvm is different there).
Then:

~~~sh
nvm install 18.12
nvm use 18.12
~~~

That way you can easily change node versions if you ever need to for another project.

3. Install VS Code and the VS Code extension "Teams Toolkit" latest version

4. Open VS Code in this folder - the NorthwindProducts folder must be at the root in VS Code.
The Teams Toolkit tab should light up with a list of accounts, Environment, etc.
Under Accounts, sign into your Microsoft 365 tenant and verify that Teams Toolkit shows "Sideloading Enabled".
Azure sign-in isn't needed unless you want to deploy the solution to Azure

5. Edit env\.env.local.user and add this line:

~~~text
SECRET_STORAGE_ACCOUNT_CONNECTION_STRING=UseDevelopmentStorage=true
~~~

## Run the app locally

1. Open the msgext-northwind-inventory-ts folder in VS Code. If you open a parent or child folder, Teams Toolkit won't detect the project and this won't work.

2. Hit F5 or click one of the various icons to start the debugger and select a profile for a browser you want to test with

> NOTE: Azurite will be automatically started and a task to create and populate the tables will run. You can use Azure Storage Explorer to ensure that the Northwind tables are populated. If a table already exists, the data population will be skipped.

Eventually, the browser should open and offer to install the "Northwind Orders" application. Just click the big "Add" button.

3. Now test in Teams or Outlook by going to a chat or new email and bringing up the "Northwind Inventory" message extension.

You should see 3 tabs corresponding to the 3 Message Extension commands in this application

Under the **Product Inventory** tab, you can enter a product name or a comma-separated list of search terms (all squished into one paramter for testing):

~~~text
name,category,inventoryStatus,supplierCity,supplierName
~~~

where inventoryStatus can be "in stock", "low stock", "on order", or "out of stock". Any blank parameter is ignored.
You should see the results, if any. For example, these queries are known to work:

   * chai - find products with names that begin with "chai
   * c,bev - find products in categories beginning with "bev" and names that begin with "c
   * ,,out - find products that are out of stock
   * ,,on,london - find products that are on order from suppliers in London
   * tofu,produce,,osaka - find products in the "produce" category with suppliers in Osaka and names that begin with "tofu"

Under the **Discounts** tab, enter a category name such as "beverages" or "dairy" to find products of that category that are discounted.

Under the **Revenue** tab, enter "high", "low", or a numeric range such as 0-10000 or 50000- (to find products with >$500K revenue). The code is simple and won't be able to handle things like &gt;50K or &lt;10,000.

4. Click a result to see the adaptive card. You should be able to update the inventory from this card.

5. When testing in early builds of Copilot, use this variation to enable multi-parameter support:

~~~text
-variants 3S.FetchOnlySearchMEFromIndex,3S.SKDS_MultiParamSupport
~~~

Here are some ideas for prompts to try. If you don't get the result you expect, try typing "new chat" and then trying again.

### Single parameter prompts

* *Find Chai in Northwind Inventory*

* *Who supplies discounted produce to Northwind?*

* *Find high revenue products in Northwind. Have there been any ad campaigns for these products?*

  (the ad campaign details are in the sample documents)

### Multi-parameter prompts

* *Find northwind dairy products that are low on stock. Show me a table with the product, supplier, units in stock and on order. Reference the details for each product.*

  (then)

  *OK can you draft an email to our procurement team asking them if we've had any delivery issues with these suppliers?*

* *Find Northwind beverages with more than 100 units in stock*

  (then)

  *What are the payment terms for these suppliers?*

  (the answer to the 2nd question is in the sample documents)

* *We’ve been receiving partial orders for Tofu. Find the supplier in Northwind and draft an email summarizing our inventory and reminding them they should stop sending partial orders per our MOQ policy.*

  (the MOQ policy is in one of the sample documents)

* *Northwind will have a booth at Microsoft Community Days  in London. Find products with local suppliers and write a LinkedIn post to promote the booth and products.*

  (then)

  *Emphasize how delicious the products are and encourage people to visit our booth at the conference*

* *What beverage is high in demand due to social media that is low stock in Northwind in London. Reference the product details to update stock.*

  (there is a document that discusses a social media campaign for one of the products)

## Resetting the local datbase

If you want to set the Northwind Database back to its starting state - based on the JSON files in the **scripts/db** folder:

 * Ensure Azurite is running (and shut it down after!)

~~~sh
npm run storage
~~~

 * In a different console, run this command:

 ~~~sh
 node .\scripts\db-setup.js "UseDevelopmentStorage=true" --reset
 ~~~

1. `npm run storage`
2. ``

## Resetting the Azure-based database

 * Obtain your Azure storage connection string. 
This value can be found in **env\env.dev.user**. You can use the `Decrypt secret` CodeLens feature shown above the entry in the file to view the decrypted value.

 * Run this script:

~~~sh
node .\scripts\db-setup.js "<SECRET_STORAGE_ACCOUNT_CONNECTION_STRING>" --reset
~~~

## Favorite Picsum images

The message extension previews include fake impages from picsum.photos. If you want to set a specific image, add this to the product in Products.json:

~~~json
"ImageUrl": "https://picsum.photos/id/xxx/200/300",
~~~

where xxx is

* 30 - Mug of coffee
* 42 - Row of cups
* 63 - Coffee mug
* 102 - Raspberries
* 112 - Field of grain
* 113 - Hot beverage (abstract)
* 165 - Field of Grain
* 225 - Tea
* 292 - Vegetables
* 312 - Honey
* 326 - Broth
* 425 - Coffee beans
* 429 - Raspberries
* 431 - Cappacino or chai
* 488 - Salsa or hot peppers
* 493 - Museli with strawberries
