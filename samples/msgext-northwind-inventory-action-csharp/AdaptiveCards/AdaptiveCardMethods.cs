using AdaptiveCards;
using Microsoft.Bot.Schema;

namespace NorthwindInventory.AdaptiveCardMethods
{
    public class AdaptiveCardMethods
    {
        public static Attachment CreateAddProductCard(List<Choice> categoryChoices, List<Choice> supplierChoices, string productName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5))
            {
                Body = new List<AdaptiveElement>
        {
            new AdaptiveTextBlock
            {
                Text = "Enter Product Details",
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium
            },
            new AdaptiveTextInput
            {
                Id = "productName",
                Placeholder = "Product Name",
                Value = productName
            },
            new AdaptiveChoiceSetInput
            {
                Id = "categoryID",
                Label = "Category",
                IsRequired = true,
                ErrorMessage = "Category is required",
                Choices = categoryChoices.Select(c => new AdaptiveChoice
                {
                    Title = c.Title,
                    Value = c.Value
                }).ToList()
            },
            new AdaptiveChoiceSetInput
            {
                Id = "supplierID",
                Label = "Supplier",
                IsRequired = true,
                ErrorMessage = "Supplier is required",
                Choices = supplierChoices.Select(s => new AdaptiveChoice
                {
                    Title = s.Title,
                    Value = s.Value
                }).ToList()
            },
            new AdaptiveNumberInput
            {
                Id = "UnitPrice",
                Label = "Unit Price",
                Min = 0
            },
            new AdaptiveTextInput
            {
                Id = "qtyPerUnit",
                Label = "Quantity per unit"
            },
            new AdaptiveNumberInput
            {
                Id = "unitsInStock",
                Label = "Units in stock",
                Min = 0
            },
            new AdaptiveNumberInput
            {
                Id = "unitsOnOrder",
                Label = "Units on order",
                Min = 0
            },
            new AdaptiveNumberInput
            {
                Id = "reorderLevel",
                Label = "Reorder level",
                Min = 0
            },
            new AdaptiveToggleInput
            {
                Id = "discontinued",
                Title = "Discontinued",
                ValueOn = "true",
                ValueOff = "false"
            }
        },
                Actions = new List<AdaptiveAction>
        {
            new AdaptiveSubmitAction
        {
            Title = "Submit",
            Data = new Dictionary<string, string>
            {
                { "action", "submit" }
            }
        },
        new AdaptiveSubmitAction
        {
            Title = "Cancel",
            Data = new Dictionary<string, string>
            {
                { "action", "cancel" }
            }
        }
        }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }
    }

    // Helper class to match the Choice structure from the JSON
    public class Choice
    {
        public string Title { get; set; }
        public string Value { get; set; }
    }
}