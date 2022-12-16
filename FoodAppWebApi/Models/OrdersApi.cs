namespace FoodAppWebApi.Models
{
    public class OrdersApi
    {
        public int InVoiceNo { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public DateTime OrderTime { get; set; }
    }
}
