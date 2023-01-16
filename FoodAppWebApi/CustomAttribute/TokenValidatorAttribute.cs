using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace FoodAppWebApi.CustomAttribute
{
    public class TokenValidatorAttribute : ActionFilterAttribute
    {

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            string token = context.HttpContext.Request.Headers["Authorization"];
            Console.WriteLine(token);
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
            else
            {
                token = token.Replace("Bearer", "").Trim();
                token = token.Replace("bearer", "").Trim();
                Console.WriteLine("----------------"+token);
                if (!ValidateAccessToken(token))
                {
                    context.Result = new JsonResult(new { message = "Invalid Token" }) { StatusCode = StatusCodes.Status401Unauthorized };
                }
                else
                    base.OnResultExecuting(context);

            }

        }

        public bool ValidateAccessToken(string token)
        {
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("some-long-secret-key")),
                ValidIssuer = "localhost:7092",
                ValidAudience = "localhost:7092",
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            }
            catch (Exception e)
            {
                return false;
            }
            return true;

        }
    }
}
