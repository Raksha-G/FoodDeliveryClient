namespace FoodAppWebApi.Models
{
    public class OrderDetailsApi
    {
        public int InVoiceNo { get; set; }
        public string UserName { get; set; }
        public string FoodItem { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int RestaurantId { get; set; }
        public DateTime OrderTime { get; set; }
    }
}
