namespace FoodAppWebApi.Models.AuthModels
{
    public class JwtConfig
    {
        public string AccessTokenSecretKey { get; set; }
        public double AccessTokenExpirationMinutes { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string RefreshTokenSecretKey { get; set; }
        public double RefreshTokenExpirationMinutes { get; set; }
    }
}
