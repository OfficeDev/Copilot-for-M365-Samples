from botbuilder.schema import AdaptiveCardInvokeResponse, InvokeResponse

def create_invoke_response(status: int, body: dict = None) -> InvokeResponse:
    return InvokeResponse(status=status, body=body)

def create_adaptive_card_invoke_response(status_code: int, body: dict = None) -> AdaptiveCardInvokeResponse:
    return AdaptiveCardInvokeResponse(
        status_code=status_code,
        type='application/vnd.microsoft.card.adaptive',
        value=body
    )

def create_action_error_response(status_code: int, error_code: int = -1, error_message: str = 'Unknown error') -> AdaptiveCardInvokeResponse:
    return AdaptiveCardInvokeResponse(
        status_code=status_code,
        type='application/vnd.microsoft.error',
        value={
            'error': {
                'code': error_code,
                'message': error_message,
            }
        }
    )

def get_inventory_status(product: dict) -> str:
    units_in_stock = int(product.get('UnitsInStock', 0))
    reorder_level = int(product.get('ReorderLevel', 0))
    units_on_order = int(product.get('UnitsOnOrder', 0))

    if units_in_stock >= reorder_level:
        return "In stock"
    elif units_in_stock < reorder_level and units_on_order == 0:
        return "Low stock"
    elif units_in_stock < reorder_level and units_on_order > 0:
        return "On order"
    elif units_in_stock == 0:
        return "Out of stock"
    else:
        return "Unknown"
