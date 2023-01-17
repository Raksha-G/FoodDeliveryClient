using FoodAppWebApi.Models;
using FoodAppWebApi.Models.AuthModels;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodAppWebApi.Services
{
    public class AuthenticationService
    {
        readonly JwtConfig _config;
        public AuthenticationService(JwtConfig config)
        {
            _config = config;
        }


        public SignUp? LoginUser(LoginRequest userCredential)
        {
            List<SignUp> userList = new();
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand("select * from Users", conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            while (sr.Read())
            {
                SignUp u = new SignUp(sr["UserName"].ToString(), sr["Email"].ToString(), sr["Password"].ToString());
                userList.Add(u);
            }
            conn.Close();

            //getting user's data from the userName
            var user = userList.FirstOrDefault(u => u.UserName == userCredential.UserName);

            //if user is null then no user is found of given username

            if (user != null)
            {
                var result = user.Password == userCredential.Password;
                if (result)
                {
                    return user;
                }
                return user;
            }

            return null;
        }



        public SuccessfullAuthenticationResponce GenerateLoginResponce(SignUp user)
        {
            string token = GenerateToken(user);
            string refreshToken = GenerateRefreshToke();

            //get from db
            DemoRefreshDto.refreshTokenDtos.Add(new RefreshTokenDto()
            {
                RefreshToken = refreshToken,
                UserName = user.UserName
            });

            //=------

            return new SuccessfullAuthenticationResponce()
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };

        }

        private string GenerateRefreshToke()
        {
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.RefreshTokenSecretKey));

            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                claims: null,
                notBefore: DateTime.UtcNow,
                expires: DateTime.Now.AddSeconds(100000),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateToken(SignUp user)
        {

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier , user.UserName.ToString()),
                new Claim(ClaimTypes.GivenName, user.UserName),
                new Claim(ClaimTypes.Role, "Customer"),

            };

            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.AccessTokenSecretKey));

            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.Now.AddSeconds(500),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);


        }

        public static string? GetCurrentUserId(ClaimsIdentity identity)
        {

            if (identity != null)
            {
                var id =
                    identity.Claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value;

                return id;
            }
            return null;

        }

    }
}
