from botbuilder.core import TurnContext
from northwindDB.products import update_product, get_product_ex
from adaptive_cards.utils import create_action_error_response, create_adaptive_card_invoke_response
from adaptive_cards import cards

async def handle_teams_card_action_update_stock(context: TurnContext):
    request = context.activity.value
    data = request['action']['data']
    print(f'🎬 Handling update stock action, quantity={data["txtStock"]}')

    if data["txtStock"] and data["productId"]:
        product = await get_product_ex(data["productId"])
        product["UnitsInStock"] = int(data["txtStock"])
        await update_product(product)
        message = f"Stock updated for {product['ProductName']} to {product['UnitsInStock']}!"
        edit_card = cards.success_card(product, message)
        return create_adaptive_card_invoke_response(200, edit_card)
    else:
        return create_action_error_response(400, 0, "Invalid request")

async def handle_teams_card_action_cancel_restock(context: TurnContext):
    request = context.activity.value
    data = request['action']['data']
    print(f'🎬 Handling cancel restock action')

    if data["productId"]:
        product = await get_product_ex(data["productId"])
        product["UnitsOnOrder"] = 0
        await update_product(product)
        message = f"Restock cancelled for {product['ProductName']}."
        edit_card = cards.success_card(product, message)
        return create_adaptive_card_invoke_response(200, edit_card)
    else:
        return create_action_error_response(400, 0, "Invalid request")

async def handle_teams_card_action_restock(context: TurnContext):
    request = context.activity.value
    data = request['action']['data']
    print(f'🎬 Handling restock action, quantity={data["txtStock"]}')

    if data["productId"]:
        product = await get_product_ex(data["productId"])
        product["UnitsOnOrder"] = int(product["UnitsOnOrder"]) + int(data["txtStock"])
        await update_product(product)
        message = f"Restocking {product['ProductName']} placed order for {product['UnitsOnOrder']} units."
        edit_card = cards.success_card(product, message)
        return create_adaptive_card_invoke_response(200, edit_card)
    else:
        return create_action_error_response(400, 0, "Invalid request")