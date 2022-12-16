using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodAppWebApi.Models
{
    public class OrderDetails
    {
       
        public int InVoiceNo { get; set; }

        public string UserName { get; set; }

        public int RestaurantId { get; set; }

        public string FoodItem { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; }

        public DateTime OrderTime { get; set; }

       public OrderDetails(int inVoiceNo, string userName, int restaurantId, string foodItem, int quantity, int price, DateTime orderTime)
        {
            InVoiceNo = inVoiceNo;
            UserName = userName;
            RestaurantId = restaurantId;
            FoodItem = foodItem;
            Quantity = quantity;
            Price = price;
            OrderTime = orderTime;
        }

        public OrderDetails(int inVoiceNo, string userName, int restaurantId, string foodItem, int quantity, int price)
        {
            InVoiceNo = inVoiceNo;
            UserName = userName;
            RestaurantId = restaurantId;
            FoodItem = foodItem;
            Quantity = quantity;
            Price = price;
            
        }

        public OrderDetails(int inVoiceNo, string userName, string foodItem, int quantity, int price, DateTime orderTime)
        {
            InVoiceNo = inVoiceNo;
            UserName = userName;
         
            FoodItem = foodItem;
            Quantity = quantity;
            Price = price;
            OrderTime = orderTime;

        }
    }
}
