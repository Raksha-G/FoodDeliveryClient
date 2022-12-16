namespace FoodAppWebApi.Models
{
    public class CartItems
    {
        public string UserName { get; set; }
        public string FoodItem { get; set; }
        public int Quantity { get; set; }
        public int RestaurantId { get; set; }
        public int FoodId { get; set; }
        public int Price { get; set; }

       
    }
}
