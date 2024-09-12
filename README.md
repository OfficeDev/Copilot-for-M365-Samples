# Copilot for Microsoft 365 samples

This repository contains samples that show how to write [agents and plugins for Microsoft 365 Copilot](https://learn.microsoft.com/microsoft-365-copilot/extensibility/).

> [!IMPORTANT]
> These samples are intended for instructive and demonstration purposes and are not intended for use in production. Do not put them into production without upgrading them to production quality.

> [!IMPORTANT]  
> To run these samples , you must ensure that your development environment meets the [requirements](https://learn.microsoft.com/microsoft-365-copilot/extensibility/prerequisites).

## Samples

| Name    | Type |Language | Description
| -------- | --- | :-------: | ------- |
| Geo-Locator Game | Declarative Agent | [JSON](./samples/cext-geolocator-game) | Copilot plays a guessing game about geography |
| Trey Research | Declarative Agent with API Plugin | [TypeScript (no auth)](./samples//cext-trey-research/) [TypeScript (OAuth 2.0)](./samples/cext-trey-research-auth/) | Handles billing and project assignments for a hypothetical consulting company |
| Northwind Inventory | Teams Message Exension | [.NET](./samples/msgext-northwind-inventory-csharp), [Python](./samples/msgext-northwind-inventory-python), [TypeScript](./samples/msgext-northwind-inventory-ts) | Plugin that allows users to query the Northwind Database
| Document Search | Teams Message Exension | [.NET](./samples/msgext-doc-search-csharp), [JavaScript](./samples/msgext-doc-search-js) | Plugin that enables Hybrid Search (Vector + Semantic)
| Product support | Teams Message Exension | [.NET](./samples/msgext-product-support-sso-csharp), [TypeScript](./samples/msgext-product-support-sso-ts) | Plugin that allows users to query the Products held in SharePoint Online team site via Microsoft Graph
| Multi Parameters | Teams Message Exension | [.NET](./samples/msgext-multiparam-csharp), [JavaScript](./samples/msgext-multiparam-js), [TypeScript](./samples/msgext-multiparam-ts) | Plugin that demonstrates how to implement complex utterances and support deep retrieval

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
