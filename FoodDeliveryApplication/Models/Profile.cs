namespace FoodDeliveryApplication.Models
{
    public class Profile
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string CreditCard { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public int Cvv { get; set; }

        public Profile(string name, string phone, string email, string address, string city, string state, string zipcode, string creditCard, string expMonth, string expYear, int cvv)
        {
            Name = name;
            Phone = phone;
            Email = email;
            Address = address;
            City = city;
            State = state;
            Zipcode = zipcode;
            CreditCard = creditCard;
            ExpMonth = expMonth;
            ExpYear = expYear;
            Cvv = cvv;
        }

        
    }
}
