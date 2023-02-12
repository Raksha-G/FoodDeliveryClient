namespace FoodDeliveryApplication.Models
{
    public class OrderedItems
    {
        public string Item { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public OrderedItems(string item, int quantity, int price)
        {
            Item = item;
            Quantity = quantity;
            Price = price;

        }
    }
}
