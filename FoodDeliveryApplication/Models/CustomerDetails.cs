using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryApplication.Models
{
    public class CustomerDetails
    {
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "Phone No is Required")]
        public string PhoneNo { get; set; }
        
        [Required(ErrorMessage = "Address is Required")]
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string CardNo { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public int CVV { get; set; }

  /*      public CustomerDetails(string userName, string phoneNo, string address, string city, string state, string zipcode, string cardNo, string expMonth, string expYear, int cVV)
        {
            UserName = userName;
            PhoneNo = phoneNo;
            Address = address;
            City = city;
            State = state;
            Zipcode = zipcode;
            CardNo = cardNo;
            ExpMonth = expMonth;
            ExpYear = expYear;
            CVV = cVV;
        }
*/






        /* public CustomerDetails(string address, string phoneNo)
         {
             Address = address;
             PhoneNo = phoneNo;
         }*/

    }
}
