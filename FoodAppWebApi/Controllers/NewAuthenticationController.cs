using FoodAppWebApi.Models;
using FoodAppWebApi.Models.AuthModels;
using FoodAppWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Claims;

namespace FoodAppWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewAuthenticationController : ControllerBase
    {

        private AuthenticationService _auth;
        private TokenValidator _tokenValidator;

        public NewAuthenticationController(JwtConfig config)
        {
            _auth = new AuthenticationService(config);
            _tokenValidator = new TokenValidator(config);
        }

        [HttpGet("Test")]
        [Authorize]
        public async Task<IActionResult> test()
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {

            if (loginRequest == null)
            {
                return BadRequest("empty login credentials");
            }

            SignUp user = _auth.LoginUser(loginRequest);

            if (user == null)
            {
                return Unauthorized("User Credentials Invalid");
             
            }
            if(user.Password!=loginRequest.Password)
            {
                return NotFound("Invalid Password");
            }
            SuccessfullAuthenticationResponce responce = _auth.GenerateLoginResponce(user);
            return Ok(responce);

        }


        [HttpPost("validate")]
        public IActionResult ValidateAccessToken(Token token)
        {
            if (_tokenValidator.ValidateAccessToken(token.token))
            {
                return Ok(_tokenValidator.ValidateAccessToken(token.token));
            }
            else
            {
                return BadRequest("Expired or invalid");
            }
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshToken)
        {

            //validate refreshToken

            string tokenString = refreshToken.RefreshToken;
            bool IsValidToken = _tokenValidator.ValidateRefreshToken(tokenString);
            if (!IsValidToken)
            {
                return BadRequest("Invalid Refresh token");
            }


            //authenticate user 
            List<SignUp> userList = new();
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=True;");
            SqlCommand cmd = new SqlCommand("select * from Users", conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            while (sr.Read())
            {
                SignUp u = new SignUp(sr["UserName"].ToString(), sr["Email"].ToString(), sr["Password"].ToString());
                userList.Add(u);
            }
            conn.Close();

            SignUp user = userList.FirstOrDefault(u => u.UserName == refreshToken.UserName);
            if (user == null)
            {
                return BadRequest("User with this refresh token not found");
            }

            SuccessfullAuthenticationResponce responce = _auth.GenerateLoginResponce(user);

            return Ok(responce);

        }


        [Authorize]
        [HttpDelete("logout")]
        public async Task<IActionResult> Logout()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            string CurrentUserName = AuthenticationService.GetCurrentUserId(identity);

            if (string.IsNullOrEmpty(CurrentUserName))
            {
                return Unauthorized("User not logged in");
            }

            DemoRefreshDto.refreshTokenDtos.RemoveAll(o => o.UserName == CurrentUserName);

            return Ok("Logged out");

        }


    }
}
