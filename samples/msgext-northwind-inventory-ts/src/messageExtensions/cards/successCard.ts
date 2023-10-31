export const successCard =
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.5",
  "refresh": {
    "userIds": [],
    "action": {
      "type": "Action.Execute",
      "verb": "refresh",
      "title": "Refresh",
      "data": {
        "productName": "${productName}",
        "unitsInStock": "${unitsInStock}",
        "productId": "${productId}",
        "categoryId": "${categoryId}",
        "imageUrl": "${imageUrl}",
        "supplierName": "${supplierName}",
        "supplierCity": "${supplierCity}",
        "categoryName": "${categoryName}",
        "inventoryStatus": "${inventoryStatus}",
        "unitPrice": "${unitPrice}",
        "quantityPerUnit": "${quantityPerUnit}",
        "unitsOnOrder": "${unitsOnOrder}",
        "reorderLevel": "${reorderLevel}",
        "unitSales": "${unitSales}",
        "inventoryValue": "${inventoryValue}",
        "revenue": "${revenue}",
        "averageDiscount": "${averageDiscount}"
      }
    }
  },
  "body": [
    {
      "type": "Container",
      "style": "good",
      "separator": true,
      "items": [
        {
          "type": "TextBlock",
          "text": "${message}",
          "weight": "Bolder",
          "size": "Medium",
          "color": "Good"
        }]
    },
    {
      "type": "Container",
      "separator": true,
      "items": [
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "items": [
                {
                  "type": "TextBlock",
                  "size": "large",
                  "weight": "bolder",
                  "text": "📦 ${productName}",
                  "wrap": true,
                  "style": "heading"
                }
              ],
              "width": "60"
            },
            {
              "type": "Column",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "${inventoryStatus}",
                  "wrap": true,
                  "horizontalAlignment": "Right",
                  "isSubtle": true,
                  "color": "${if(inventoryStatus == 'In stock', 'good', if(inventoryStatus == 'low stock', 'warning', 'attention'))}"
                }
              ],
              "width": "40"
            }
          ]
        }

      ],
      "bleed": true
    },
    {
      "type": "Container",
      "style": "emphasis",
      "items": [
        {
          "type": "TextBlock",
          "weight": "Bolder",
          "text": "**📍Supplier information**",
          "wrap": true,

          "size": "Medium",
          "isSubtle": false
        },
        {
          "type": "ColumnSet",
          "separator": true,
          "columns": [
            {
              "type": "Column",
              "width": "stretch",

              "items": [
                {
                  "type": "FactSet",
                  "spacing": "Large",
                  "facts": [
                    {
                      "title": "Name",
                      "value": "${supplierName}"
                    },
                    {
                      "title": "City",
                      "value": "${supplierCity}"
                    }

                  ],
                  "separator": true
                }
              ]
            }

          ]
        },
        {
          "type": "TextBlock",
          "weight": "Bolder",
          "text": "**🛒 Stock information**",
          "wrap": true,

          "size": "Medium",
          "isSubtle": false
        },
        {
          "type": "ColumnSet",
          "separator": true,
          "columns": [
            {
              "type": "Column",
              "width": "stretch",

                            "items": [
                                {
                                    "type": "FactSet",
                                    "spacing": "Large",
                                    "facts": [
                                        {
                                            "title": "Category",
                                            "value": "${categoryName}"
                                        },
                                       
                                        {
                                            "title": "Unit price",
                                            "value": "${unitPrice} USD"
                                        },
                                        {
                                            "title": "Avg discount",
                                            "value": "${averageDiscount}"
                                        }
                                        
                                    ],
                                    "separator": true
                                }
                            ]
                        },
                        {
                            "type": "Column",
                            "width": "stretch",

                            "items": [
                                {
                                    "type": "FactSet",
                                    "spacing": "Large",
                                    "facts": [
                                       
                                        {
                                            "title": "Units in stock",
                                            "value": "${unitsInStock}"
                                        },                                      
                                        {
                                            "title": "Units on order",
                                            "value": "${unitsOnOrder}"
                                        },
                                        {
                                            "title": "Reorder Level",
                                            "value": "${reorderLevel}"
                                        }
                                    ],
                                    "separator": true
                                }
                            ]
                        }
                     

          ]
        }]
    },
    {
      "type": "Container",
      "items": [
        {
          "type": "ActionSet",
          "actions": [
            {
              "type": "Action.ShowCard",
              "title": "Take action",
              "card": {
                "type": "AdaptiveCard",
                "body": [
                  {
                    "type": "Input.Text",
                    "id": "txtStock",
                    "label": "Quantity",
                    "min": 0,
                    "max": 9999,
                    "errorMessage": "Invalid input, use whole positive number",
                    "style": "Tel"
                  }
                ],
                "actions": [
                  {
                    "type": "Action.Execute",
                    "title": "Update stock ✅",
                    "verb": "ok",
                    "data": {
                      "productName": "${productName}",
                      "unitsInStock": "${unitsInStock}",
                      "productId": "${productId}",
                      "categoryId": "${categoryId}",
                      "imageUrl": "${imageUrl}",
                      "supplierName": "${supplierName}",
                      "supplierCity": "${supplierCity}",
                      "categoryName": "${categoryName}",
                      "inventoryStatus": "${inventoryStatus}",
                      "unitPrice": "${unitPrice}",
                      "quantityPerUnit": "${quantityPerUnit}",
                      "unitsOnOrder": "${unitsOnOrder}",
                      "reorderLevel": "${reorderLevel}",
                      "unitSales": "${unitSales}",
                      "inventoryValue": "${inventoryValue}",
                      "revenue": "${revenue}",
                      "averageDiscount": "${averageDiscount}"
                    }
                  },
                  {
                    "type": "Action.Execute",
                    "title": "Restock 📦",
                    "verb": "restock",
                    "data": {
                      "productName": "${productName}",
                      "unitsInStock": "${unitsInStock}",
                      "productId": "${productId}",
                      "categoryId": "${categoryId}",
                      "imageUrl": "${imageUrl}",
                      "supplierName": "${supplierName}",
                      "supplierCity": "${supplierCity}",
                      "categoryName": "${categoryName}",
                      "inventoryStatus": "${inventoryStatus}",
                      "unitPrice": "${unitPrice}",
                      "quantityPerUnit": "${quantityPerUnit}",
                      "unitsOnOrder": "${unitsOnOrder}",
                      "reorderLevel": "${reorderLevel}",
                      "unitSales": "${unitSales}",
                      "inventoryValue": "${inventoryValue}",
                      "revenue": "${revenue}",
                      "averageDiscount": "${averageDiscount}"
                    }
                  },
                  {
                    "type": "Action.Execute",
                    "title": "Cancel restock ❌",
                    "verb": "cancel",
                    "data": {
                      "productName": "${productName}",
                      "unitsInStock": "${unitsInStock}",
                      "productId": "${productId}",
                      "categoryId": "${categoryId}",
                      "imageUrl": "${imageUrl}",
                      "supplierName": "${supplierName}",
                      "supplierCity": "${supplierCity}",
                      "categoryName": "${categoryName}",
                      "inventoryStatus": "${inventoryStatus}",
                      "unitPrice": "${unitPrice}",
                      "quantityPerUnit": "${quantityPerUnit}",
                      "unitsOnOrder": "${unitsOnOrder}",
                      "reorderLevel": "${reorderLevel}",
                      "unitSales": "${unitSales}",
                      "inventoryValue": "${inventoryValue}",
                      "revenue": "${revenue}",
                      "averageDiscount": "${averageDiscount}"
                    }
                  }
                ]
              }
            }
          ]
        }
      ]
    }
  ]
}