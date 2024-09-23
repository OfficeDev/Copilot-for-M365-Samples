def edit_card(query):
    return {
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
                    "productId": f"{query.get('ProductID')}"
                }
            }
        },
        "body": [
            {
                "type": "Container",
                "separator": "true",
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
                                        "text": f"📦 {query.get('ProductName')}",
                                        "wrap": "true",
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
                                        "text": f"{query.get('InventoryStatus')}",
                                        "wrap": "true",
                                        "horizontalAlignment": "Right",
                                        "isSubtle": "true",
                                        "color": 'good' if query.get('InventoryStatus') == 'In stock' else 'warning' if query.get('InventoryStatus') == 'low stock' else 'attention'
                                    }
                                ],
                                "width": "40"
                            }
                        ]
                    }
                ],
                "bleed": "true"
            },
            {
                "type": "Container",
                "style": "emphasis",
                "items": [
                    {
                        "type": "TextBlock",
                        "weight": "Bolder",
                        "text": "**📍Supplier information**",
                        "wrap": "true",
                        "size": "Medium",
                        "isSubtle": "false"
                    },
                    {
                        "type": "ColumnSet",
                        "separator": "true",
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
                                                "value": f"{query.get('SupplierName')}"
                                            },
                                            {
                                                "title": "City",
                                                "value": f"{query.get('SupplierCity')}"
                                            }
                                        ],
                                        "separator": "true"
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        "type": "TextBlock",
                        "weight": "Bolder",
                        "text": "**🛒 Stock information**",
                        "wrap": "true",
                        "size": "Medium",
                        "isSubtle": "false"
                    },
                    {
                        "type": "ColumnSet",
                        "separator": "true",
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
                                                "value": f"{query.get('CategoryName')}"
                                            },
                                            {
                                                "title": "Unit price",
                                                "value": f"{query.get('UnitPrice')} USD"
                                            },
                                            {
                                                "title": "Avg discount",
                                                "value": f"{query.get('AverageDiscount')} %"
                                            },
                                            {
                                                "title": "Inventory valuation",
                                                "value": f"{query.get('InventoryValue')} USD"
                                            }
                                        ],
                                        "separator": "true"
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
                                                "value": f"{query.get('UnitsInStock')}"
                                            },
                                            {
                                                "title": "Units on order",
                                                "value": f"{query.get('UnitsOnOrder')}"
                                            },
                                            {
                                                "title": "Reorder Level",
                                                "value": f"{query.get('ReorderLevel')}"
                                            },
                                            {
                                                "title": "Revenue this period",
                                                "value": f"{query.get('Revenue')} USD"
                                            }
                                        ],
                                        "separator": "true"
                                    }
                                ]
                            }
                        ]
                    }
                ]
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
                                                "productId": f"{query.get('ProductID')}"
                                            }
                                        },
                                        {
                                            "type": "Action.Execute",
                                            "title": "Restock 📦",
                                            "verb": "restock",
                                            "data": {
                                                "productId": f"{query.get('ProductID')}"
                                            }
                                        },
                                        {
                                            "type": "Action.Execute",
                                            "title": "Cancel restock ❌",
                                            "verb": "cancel",
                                            "data": {
                                                "productId": f"{query.get('ProductID')}"
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

def success_card(query, message):
    return {
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
                "productId": f"{query.get('ProductID')}"
            }
            }
        },
        "body": [
            {
            "type": "Container",
            "style": "good",
            "separator": "true",
            "items": [
                {
                "type": "TextBlock",
                "text": f"{message}",
                "weight": "Bolder",
                "size": "Medium",
                "color": "Good"
                }]
            },
            {
            "type": "Container",
            "separator": "true",
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
                        "text": f"📦 {query.get('ProductName')}",
                        "wrap": "true",
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
                        "text": f"{query.get('InventoryStatus')}",
                        "wrap": "true",
                        "horizontalAlignment": "Right",
                        "isSubtle": "true",
                        "color": 'good' if query.get('InventoryStatus') == 'In stock' else 'warning' if query.get('InventoryStatus') == 'low stock' else 'attention'
                        }
                    ],
                    "width": "40"
                    }
                ]
                }

            ],
            "bleed": "true"
            },
            {
            "type": "Container",
            "style": "emphasis",
            "items": [
                {
                "type": "TextBlock",
                "weight": "Bolder",
                "text": "**📍Supplier information**",
                "wrap": "true",

                "size": "Medium",
                "isSubtle": "false"
                },
                {
                "type": "ColumnSet",
                "separator": "true",
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
                            "value": f"{query.get('SupplierName')}"
                            },
                            {
                            "title": "City",
                            "value": f"{query.get('SupplierCity')}"
                            }

                        ],
                        "separator": "true"
                        }
                    ]
                    }

                ]
                },
                {
                "type": "TextBlock",
                "weight": "Bolder",
                "text": "**🛒 Stock information**",
                "wrap": "true",
                "size": "Medium",
                "isSubtle": "false"
                },
                {
                "type": "ColumnSet",
                "separator": "true",
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
                                                    "value": f"{query.get('CategoryName')}"
                                                },
                                            
                                                {
                                                    "title": "Unit price",
                                                    "value": f"{query.get('UnitPrice')} USD"
                                                },
                                                {
                                                    "title": "Avg discount",
                                                    "value": f"{query.get('AverageDiscount')} %"
                                                },
                                                {
                                                    "title": "Inventory valuation",
                                                    "value": f"{query.get('InventoryValue')} USD"
                                                }                                        
                                            ],
                                            "separator": "true"
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
                                                    "value": f"{query.get('UnitsInStock')}"
                                                },                                      
                                                {
                                                    "title": "Units on order",
                                                    "value": f"{query.get('UnitsOnOrder')}"
                                                },
                                                {
                                                    "title": "Reorder Level",
                                                    "value": f"{query.get('ReorderLevel')}"
                                                },
                                                {
                                                    "title": "Revenue this period",
                                                    "value": f"{query.get('Revenue')} USD"
                                                }
                                            ],
                                            "separator": "true"
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
                            "productId": f"{query.get('ProductID')}"
                            }
                        },
                        {
                            "type": "Action.Execute",
                            "title": "Restock 📦",
                            "verb": "restock",
                            "data": {
                            "productId": f"{query.get('ProductID')}"
                            }
                        },
                        {
                            "type": "Action.Execute",
                            "title": "Cancel restock ❌",
                            "verb": "cancel",
                            "data": {
                            "productId": f"{query.get('ProductID')}"
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
