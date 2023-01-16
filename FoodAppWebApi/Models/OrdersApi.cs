namespace FoodAppWebApi.Models
{
    public class OrdersApi
    {
        public int InVoiceNo { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public DateTime OrderTime { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string CardNo { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public int CVV { get; set; }
    }
}
