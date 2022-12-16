namespace FoodAppWebApi.Models
{
    public class Restaurants
    {
        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; }

        public string RestaurantImage { get; set; }

        public Restaurants()
        {

        }


        public Restaurants(int restaurantId, string restaurantName, string restaurantImage)
        {
            RestaurantId = restaurantId;
            RestaurantName = restaurantName;
            RestaurantImage = restaurantImage;
        }
    }
}
