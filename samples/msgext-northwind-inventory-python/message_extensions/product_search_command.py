from botbuilder.core import TurnContext, CardFactory
from botbuilder.schema import HeroCard, CardImage, CardAction
from botbuilder.schema.teams import MessagingExtensionQuery, MessagingExtensionResponse,MessagingExtensionAttachment, MessagingExtensionResult
from northwindDB.products import search_products
from adaptive_cards import cards

COMMAND_ID = 'inventorySearch'
query_count = 0

async def handle_teams_messaging_extension_query(context: TurnContext, query: MessagingExtensionQuery) -> MessagingExtensionResponse:
    global query_count

    product_name = category_name = inventory_status = supplier_city = stock_level = None

    # For now we have the ability to pass parameters comma separated for testing until the UI supports it.
    # So try to unpack the parameters but when issued from Copilot or the multi-param UI they will come
    # in the parameters array.
    if len(query.parameters) == 1 and query.parameters[0].get("name") == "productName":
        values = query.parameters[0].get("value").split(',')
        product_name, category_name, inventory_status, supplier_city, stock_level = (values + [None] * 5)[:5]
    else:
        product_name = cleanup_param(next((param.get("value") for param in query.parameters if param.get("name") == 'productName'), None))
        category_name = cleanup_param(next((param.get("value") for param in query.parameters if param.get("name") == 'categoryName'), None))
        inventory_status = cleanup_param(next((param.get("value") for param in query.parameters if param.get("name") == 'inventoryStatus'), None))
        supplier_city = cleanup_param(next((param.get("value") for param in query.parameters if param.get("name") == 'supplierCity'), None))
        stock_level = cleanup_param(next((param.get("value") for param in query.parameters if param.get("name") == 'stockQuery'), None))

    print(f'🔎 Query #{query_count + 1}:\nproductName={product_name}, categoryName={category_name}, inventoryStatus={inventory_status}, supplierCity={supplier_city}, stockLevel={stock_level}')
    query_count += 1

    products = await search_products(product_name, category_name, inventory_status, supplier_city, stock_level)

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
    else:
        result = value.strip()
        result = result.split(',')[0] 
        result = result.replace('*', '')  
        return result