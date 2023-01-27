namespace FoodDeliveryApplication.Models
{
    public class Restaurants
    {
        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; }

        public string RestaurantImage { get; set; }

        public string Cuisine { get; set; }

        public Restaurants()
        {

        }


        public Restaurants(int restaurantId, string restaurantName, string restaurantImage)
        {
            RestaurantId = restaurantId;
            RestaurantName = restaurantName;
            RestaurantImage = restaurantImage;
        }
        public Restaurants(int restaurantId, string restaurantName, string restaurantImage,string cuisine)
        {
            RestaurantId = restaurantId;
            RestaurantName = restaurantName;
            RestaurantImage = restaurantImage;
            Cuisine = cuisine;
        }
    }
}
