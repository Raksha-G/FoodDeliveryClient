namespace FoodDeliveryApplication.Models
{
    public class FoodItems
    {

        public string OrderId { get; set; }

        public string ItemName { get; set; }

        public int Quantity { get; set; }

        public string RestaurantName { get; set; }

        public int Price { get; set; }

        public string Type { get; set; }

        public FoodItems(string orderId, string itemName, int quantity, string restaurantName, int price)
        {
            OrderId = orderId;
            ItemName = itemName;
            Quantity = quantity;
            RestaurantName = restaurantName;
            Price = price;
            
        }

        public FoodItems(string orderId, string itemName, int quantity, string restaurantName, int price, string type)
        {
            OrderId = orderId;
            ItemName = itemName;
            Quantity = quantity;
            RestaurantName = restaurantName;
            Price = price;
            Type = type;

        }
    }
}
