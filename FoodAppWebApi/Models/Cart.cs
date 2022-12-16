namespace FoodAppWebApi.Models
{
    public class Cart
    {
        public string UserName { get; set; }

        public string FoodItem { get; set; }

        public int Quantity { get; set; }

        public string FoodImage { get; set; }

        public int Price { get; set; }

        public int FoodId { get; set; }

        public int RestaurantId { get; set; }

        public int ItemNo { get; set; }

        //public string RestaurantName { get; set; }

        public Cart()
        {

        }

        public Cart(string userName, string foodItem, int quantity, string foodImage, int price, int foodId, int restaurantId, int itemNo)
        {
            this.UserName = userName;
            this.FoodItem = foodItem;
            this.Quantity = quantity;
            this.FoodImage = foodImage;
            this.Price = price;
            this.FoodId = foodId;
            this.RestaurantId = restaurantId;
            ItemNo = itemNo;
            //this.RestaurantName = restaurantName;
        }
    }
}
