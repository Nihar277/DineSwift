namespace Repository.Model
{
    public class PlaceOrderItem
{
    public int FoodItemId { get; set; }
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Price * Quantity;
}

}