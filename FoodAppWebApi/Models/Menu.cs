namespace FoodAppWebApi.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string food_img { get; set; }

        public string food_item { get; set; }

        public int price { get; set; }

        public int RestaurantId { get; set; }

       
        public Menu()
        {

        }

        public Menu(int Id,string food_img, string food_item, int price, int restaurantId)
        {
            this.Id = this.Id;
            this.food_img = food_img;
            this.food_item = food_item;
            this.price = price;
            this.RestaurantId = restaurantId;
        }

        /* public Menu(string food_img)
         {
             //this.Id = this.Id;
             this.food_img = food_img;
             //this.food_item = food_item;
             //this.price = price;
         }*/
    }
}
