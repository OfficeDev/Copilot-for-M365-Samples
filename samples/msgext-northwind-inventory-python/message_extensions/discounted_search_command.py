from botbuilder.core import TurnContext, CardFactory
from botbuilder.schema import HeroCard, CardImage, CardAction
from botbuilder.schema.teams import MessagingExtensionQuery, MessagingExtensionResponse,MessagingExtensionAttachment, MessagingExtensionResult
from northwindDB.products import get_discounted_products_by_category
from adaptive_cards import cards

COMMAND_ID = "discountSearch"
query_count = 0

async def handle_teams_messaging_extension_query(context: TurnContext, query: MessagingExtensionQuery) -> MessagingExtensionResponse:
    global query_count

    # Seek the parameter by name, don't assume it's in element 0 of the array
    category_name = cleanup_param(next((param.get("value") for param in query.parameters if param.get("name") == 'categoryName'), None))

    print(f'💰 Discount query #{query_count + 1}: Discounted products with categoryName={category_name}')
    query_count += 1

    products = await get_discounted_products_by_category(category_name)

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
    result = value.strip()
    result = result.split(',')[0]  
    result = result.replace('*', '')
    return result