using System.ComponentModel.DataAnnotations;

namespace FoodAppWebApi.Models.AuthModels
{

    public static class DemoRefreshDto
    {
        public static List<RefreshTokenDto> refreshTokenDtos = new List<RefreshTokenDto>();

    }

    public class RefreshTokenDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string RefreshToken { get; set; }

    }
}
