import { Product } from "../northwindDB/model";

export const CreateInvokeResponse = (body:any) => {
    return { status: 200, body }
};

export const getInventoryStatus = (product: Product) => {
  if (Number(product.UnitsInStock) >= Number(product.ReorderLevel)) {
    return "In stock";
  } else if (Number(product.UnitsInStock) < Number(product.ReorderLevel) && Number(product.UnitsOnOrder) === 0) {
    return "Low stock";
  } else if (Number(product.UnitsInStock) < Number(product.ReorderLevel) && Number(product.UnitsOnOrder) > 0) {
    return "On order";
  } else if (Number(product.UnitsInStock) === 0) {
    return "Out of stock";
  } else {
    return "Unknown"; //fall back
  }
}
