# Copilot for Microsoft 365 samples

This repository contains samples that show how to [extend Copilot for Microsoft 365](https://learn.microsoft.com/microsoft-365-copilot/extensibility/).

<p style="align:center"><img src="https://learn.microsoft.com/en-gb/microsoft-365-copilot/extensibility/assets/images/m365-extensibility-types.png" alt="This illustration shows types of extensibility options, Graph connector, plugins, and declarative copilots" /></p>

> [!IMPORTANT]  
> To extend Copilot for Microsoft 365, you must ensure that your development environment meets the [requirements](https://learn.microsoft.com/microsoft-365-copilot/extensibility/prerequisites).

## Samples

| Name    | Language | Description
| -------- | :-------: | ------- |
| [Northwind Inventory](./samples/msgext-northwind-inventory-ts) | TypeScript | Plugin that allows users to query the Northwind Database |
| [Document Search](./samples/msgext-doc-search-js) | JavaScript | Plugin that enables Hybrid Search (Vector + Semantic) |
| [Document Search](./samples/msgext-doc-search-csharp) | .NET | Plugin that enables Hybrid Search (Vector + Semantic) |
| [Product support](./samples/msgext-product-support-sso-csharp) | .NET | Plugin that allows users to query the Products held in SharePoint Online team site via Microsoft Graph |
| [Multi Parameters](./samples/msgext-multiparam-csharp) | .NET | Plugin that demonstrates how to implement complex utterances and support deep retrieval |
| [Multi Parameters](./samples/msgext-multiparam-ts) | TypeScript | Plugin that demonstrates how to implement complex utterances and support deep retrieval |
| [Multi Parameters](./samples/msgext-multiparam-js) | JavaScript | Plugin that demonstrates how to implement complex utterances and support deep retrieval |

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
