using System.ComponentModel.DataAnnotations;

namespace FoodAppWebApi.Models
{
    public class CustomerDetails
    {
        [Required(ErrorMessage = "Address is Required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone No is Required")]
        public string PhoneNo { get; set; }

       /* public CustomerDetails(string address, string phoneNo)
        {
            Address = address;
            PhoneNo = phoneNo;
        }*/
    }
}
