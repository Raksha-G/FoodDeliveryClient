using System.ComponentModel.DataAnnotations;

namespace FoodAppWebApi.Models.AuthModels
{
    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
