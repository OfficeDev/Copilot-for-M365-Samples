# Setup notes

## First time

1. Ensure you have installed node.js 18.x. If you're starting fresh, I suggest installing nvm (search "nvm for Windows" if you're on Windows, nvm is different there).
Then:

~~~sh
nvm install 18.12
nvm use 18.12
~~~

That way you can easily change node versions if you ever need to for another project.

2. Install VS Code and the VS Code extension "Teams Toolkit" latest version

3. Open VS Code in this folder - the NorthwindProducts folder must be at the root in VS Code.
The Teams Toolkit tab should light up with a list of accounts, Environment, etc.
Under Accounts, sign into your Microsoft 365 tenant and verify that Teams Toolkit shows "Sideloading Enabled".
Azure sign-in isn't needed unless you want to deploy the solution to Azure

4. Edit env\.env.local.user and add this line:

~~~text
SECRET_STORAGE_ACCOUNT_CONNECTION_STRING=UseDevelopmentStorage=true
~~~

## Run the app locally

1. Open the NorthwindProducs folder in VS Code

2. Hit F5 or click one of the various icons to start the debugger and select a profile for a browser you want to test with

> NOTE: Azurite will be automatically started and a task to create and populate the tables will run. You can use Azure Storage Explorer to ensure that the Northwind tables are populated. If a table already exists, the data population will be skipped.

Eventually, the browser should open and offer to install the "Northwind Orders" application. Just click the big "Add" button.

3. Now test in Teams or Outlook by going to a chat or new email and bringing up the "Northwind Inventory" message extension.
You can enter a product name or a comma-separated list of search terms (all squished into one paramter for testing):

~~~text
name,category,inventoryStatus,supplierCity,supplierName
~~~

where inventoryStatus can be "in stock", "low stock", "on order", or "out of stock". Any blank parameter is ignored.
You should see the results, if any. For example, these queries are known to work:

   chai - find products with names that begin with "chai"
   c,bev - find products in categories beginning with "bev" and names that begin with "c"
   ,,out - find products that are out of stock
   ,,on,london - find products that are on order from suppliers in London
   tofu,produce,,osaka - find products in the "produce" category with suppliers in Osaka and names that begin with "tofu"

4. Click a result to see the adaptive card. You should be able to update the inventory from this card.

5. When testing in Copilot, use this variation to enable multi-parameter support:

~~~text
-variants 3S.FetchOnlySearchMEFromIndex,3S.SKDS_MultiParamSupport
~~~

# To Do list



# Lab Writer Notes

This file is for notes about the lab instructions, to be used when writing the labs.

## Lab 2 - Northwind Products

Check the demo instructions above and include any that are not covered in Lab 1.
Here are some things to be sure we include when writing instructions from Lab 1 --> Lab 2:

Code changes - including additions to .vsCode/tasks.json for Azurite - will be determined using Beyond Compare.

```shell
npm install @azure/data-tables
npm install @microsoft/adaptivecards-tools
npm install azurite --save-dev
```

Add to .env.local.user:

~~~text
SECRET_STORAGE_ACCOUNT_CONNECTION_STRING=UseDevelopmentStorage=true
~~~

## Reset database

### Local

1. `npm run storage`
2. `node .\scripts\db-setup.js "UseDevelopmentStorage=true" --reset`

### Remote

1. `node .\scripts\db-setup.js "<SECRET_STORAGE_ACCOUNT_CONNECTION_STRING>" --reset`

Replace `<SECRET_STORAGE_ACCOUNT_CONNECTION_STRING>` with the decrypted value from `env\env.dev.user`. You can use the `Decrypt secret` CodeLens feature shown above the entry in the file to view the decrypted value.

## Favorite Picsum images

Add this to the product in Products.json:
"ImageUrl": "https://picsum.photos/id/xxx/200/300",

where xxx is
30 - Mug of coffee
42 - Row of cups
63 - Coffee mug
102 - Raspberries
112 - Field of grain
113 - Hot beverage (abstract)
165 - Field of Grain
225 - Tea
292 - Vegetables
312 - Honey
326 - Broth
425 - Coffee beans
429 - Raspberries
431 - Cappacino or chai
488 - Salsa or hot peppers
493 - Museli with strawberries
