using System.ComponentModel.DataAnnotations;

namespace FoodAppWebApi.Models
{
    public class SignUp
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }


        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }


        public SignUp()
        {

        }

        public SignUp(string userName, string email, string password)
        {
            UserName = userName;
            Email = email;
            Password = password;
        }
    }
}
