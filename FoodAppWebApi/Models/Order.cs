using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodAppWebApi.Models
{
    public class Order
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        public string UserName { get; set; }

        public string FoodItem { get; set; }

        public int Price { get; set; }

        public int Quantity { get; set; }

        public int RestaurantId { get; set; }

        public string Status { get; set; }

        public Order()
        {

        }

        public Order(string userName, string foodItem, int price, int quantity, int restaurantId)
        {
   
            UserName = userName;
            FoodItem = foodItem;
            Price = price;
            Quantity = quantity;
            RestaurantId = restaurantId;
        }

        public Order(string foodItem, int price, int quantity, string status)
        {
            FoodItem = foodItem;
            Price = price;
            Quantity = quantity;
            Status = status;
        }
    }
}
