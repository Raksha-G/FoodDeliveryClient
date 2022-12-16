namespace FoodAppWebApi.Models
{
    public class SuccessfullAuthenticationResponce
    {

        public string AccessToken { get; set; }


        //public DateTime AccessTokenExpirationTime { get; set; }
        
        public string RefreshToken { get; set; }

    }
}
