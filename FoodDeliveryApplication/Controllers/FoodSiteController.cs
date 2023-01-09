using FoodDeliveryApplication.Models;
using FoodDeliveryApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.Data.SqlClient;


namespace FoodDeliveryApplication.Controllers
{
    [CustomExceptionFilter]
    public class FoodSiteController : Controller
    {

        public List<SignUp> userList = new List<SignUp>();
        public string NotExist = "User Not Exist";
        static string CurrUser;
        public List<FoodItems> FoodItemsSelected = new List<FoodItems>();
        private readonly ILogger<FoodSiteController> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;


        public FoodSiteController(ILogger<FoodSiteController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this._httpContextAccessor = httpContextAccessor;

            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=True;");
            SqlCommand cmd = new SqlCommand("select * from Users", conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            while (sr.Read())
            {
                SignUp user = new SignUp(sr["UserName"].ToString(), sr["Email"].ToString(), sr["Password"].ToString());
                userList.Add(user);
            }
        }
        public IActionResult Index()
        {

            return View();
        }


        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(SignUp signup)
        {
            var user = userList.Find(e => e.UserName == signup.UserName);
            if (user != null)
            {
                ViewBag.UserName = "UserName already Exist";
                _logger.LogInformation("User:{0} already Exist, unable to create new Account", signup.UserName);
                return View();
            }

            var httpClient = new HttpClient();

            JsonContent content = JsonContent.Create(signup);
            using (var apiRespoce = await httpClient.PostAsync("https://localhost:7021/api/Food/SignUp", content))
            {
                if (apiRespoce.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    _logger.LogInformation(String.Format("A new Account Created with UserName {0}", signup.UserName));
                    return RedirectToAction("Login");

                }
                else
                {
                    return Content("Error: " + await apiRespoce.Content.ReadAsStringAsync());
                }
            }


        }

        public IActionResult Login()
        {
            _logger.LogInformation("Login Triggered");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginDetails login)
        {
            var httpClient = new HttpClient();

            JsonContent content = JsonContent.Create(login);
            using (var apiRespoce = await httpClient.PostAsync("https://localhost:7021/api/NewAuthentication/login", content))
            {
                if (apiRespoce.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string res = await apiRespoce.Content.ReadAsStringAsync();
                    Console.WriteLine(res);
                    var userRes = JsonConvert.DeserializeObject<SuccessfullAuthenticationResponce>(res);

                    //_httpContextAccessor.HttpContext.Session.SetString("UserName", login.UserName);
                    //_httpContextAccessor.HttpContext.Session.SetString("AccessToken", userRes.AccessToken);
                    //_httpContextAccessor.HttpContext.Session.SetString("RefreshToken", userRes.RefreshToken); 
                    
                    _httpContextAccessor.HttpContext.Session.SetString("UserName", login.UserName);
                    _httpContextAccessor.HttpContext.Session.SetString("AccessToken", userRes.AccessToken);
                    _httpContextAccessor.HttpContext.Session.SetString("RefreshToken", userRes.RefreshToken);

                    return RedirectToAction("Restaurants");

                }
                else
                {
                    return Content("Error: " + await apiRespoce.Content.ReadAsStringAsync());
                }

            }
        }


        public async Task<IActionResult> Logout()
        {

            HttpClient httpClient = new HttpClient();

            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login","FoodSite");
            }

            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            var apiResponce = await httpClient.DeleteAsync("https://localhost:7021/api/NewAuthentication/logout");

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("{0} Logged Out", _httpContextAccessor.HttpContext.Session.GetString("UserName"));
                _httpContextAccessor.HttpContext.Session.Clear();
                return RedirectToAction("Index","Home");
            }
            else
            {
                return Content("Api error " + apiResponce.StatusCode);
            }

        }



        public IActionResult Menu()
        {

            return View();
        }


        public async Task<IActionResult> Restaurants()
        {


            List<Restaurants> res = new List<Restaurants>();
            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

            HttpClient httpClient = new HttpClient();
            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });



            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            Console.WriteLine(AccessToken);

            using (var apiResponce = await httpClient.GetAsync("https://localhost:7021/api/Food/GetAllRestaurants"))
            {

                if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string res1 = await apiResponce.Content.ReadAsStringAsync();
                    res = JsonConvert.DeserializeObject<List<Restaurants>>(res1);
                    return View("Restaurants", res);
                }
                else
                {
                    return Content("Api error" + apiResponce.StatusCode);
                }

            }

        }




        public async Task<IActionResult> RestaurantMenu(int Id)
        {

            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

            HttpClient httpClient = new HttpClient();
            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            List<Menu> res = new List<Menu>();
            //HttpClient httpClient = new HttpClient();
            var apiResponce = await httpClient.GetAsync("https://localhost:7021/api/Food/GetRestaurantMenuById/" + Id);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res1 = await apiResponce.Content.ReadAsStringAsync();
                res = JsonConvert.DeserializeObject<List<Menu>>(res1);
                return View("Menu", res);
            }
            else
            {
                return Content("Api error" + apiResponce.StatusCode);
            }

        }

        public IActionResult Order()
        {
            return Content(_httpContextAccessor.HttpContext.Session.GetString("UserName"));

        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(IFormCollection col)
        {

            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

            HttpClient httpClient = new HttpClient();
            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });


            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            string? UserName = _httpContextAccessor.HttpContext.Session.GetString("UserName");

            Console.WriteLine(col["Food_Item"]);
            //Console.WriteLine("Food Id :" +col["Food_Id"]);
            var Food_Item = col["Food_Item"];
            int Quantity = Convert.ToInt32(col["Quantity"]);
            int Restaurant_Id = Convert.ToInt32(col["RestaurantId"]);
            var Food_Id = Convert.ToInt32(col["Food_Id"]);
            int Price = Convert.ToInt32(col["Price"]);

            CartItems cart = new CartItems();
            cart.UserName = UserName;
            cart.FoodItem = Food_Item;
            cart.RestaurantId = Restaurant_Id;
            cart.Quantity = Quantity;
            cart.Price = Price;

            _logger.LogInformation("Item:{0} added to cart by the user:{1} of Quantity:{2}", Food_Item, _httpContextAccessor.HttpContext.Session.GetString("UserName"), Quantity);


            JsonContent content1 = JsonContent.Create(cart);
            using (var apiRespoce = await httpClient.PostAsync("https://localhost:7021/api/Food/AddToCart", content1))
            {
                if (apiRespoce.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    TempData["success"] = "Item Added to Cart ";

                    return RedirectToAction("RestaurantMenu", new { Id = Restaurant_Id });

                }
                else
                {
                    return Content("Error: " + apiRespoce.StatusCode);
                }





            }
        }


        public async Task<IActionResult> DeleteItemFromCart(int Id)
        {
            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

            HttpClient httpClient = new HttpClient();
            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });



            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);
            Console.WriteLine("Idddd" + Id);



            //HttpClient httpClient = new HttpClient();
            var apiResponce = await httpClient.DeleteAsync("https://localhost:7021/api/Food/DeleteCartItemById/" + Id);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TempData["success"] = "Item Removed From Cart";
                return RedirectToAction("Cart");
            }
            else
            {
                return Content("Api error " + apiResponce.StatusCode);
            }

        }


        public async Task<IActionResult> Cart()
        {
            string? UserName = _httpContextAccessor.HttpContext.Session.GetString("UserName");
            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

            HttpClient httpClient = new HttpClient();
            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });


            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);



            List<Cart> res = new List<Cart>();
            var apiResponce = await httpClient.GetAsync("https://localhost:7021/api/Food/GetCartByUserName?UserName=" + UserName);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res1 = await apiResponce.Content.ReadAsStringAsync();
                res = JsonConvert.DeserializeObject<List<Cart>>(res1);

                return View("Cart", res);
            }
            else
            {
                ViewBag.Cart = "Empty";
                return View("Cart", null);
                //return Content("Api error" + apiResponce.StatusCode);
            }

        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CustomerDetails details)
        {
            string? UserName = _httpContextAccessor.HttpContext.Session.GetString("UserName");
            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

            HttpClient httpClient = new HttpClient();
            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });


            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            Random rnd = new Random();
            int inVoiceNo = rnd.Next(10000, 10000000);

            DateTime OrderTime = DateTime.Now;


            string address = details.Address;
            string phoneNo = details.PhoneNo;

            Console.WriteLine(OrderTime);

            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");


            OrderPlaced order = new OrderPlaced();
            order.Address = address;
            order.InVoiceNo = inVoiceNo;
            order.PhoneNo = phoneNo;
            order.UserName = UserName;
            order.OrderTime = OrderTime;

            JsonContent content1 = JsonContent.Create(order);
            using (var apiRespoce = await httpClient.PostAsync("https://localhost:7021/api/Food/Orders", content1))
            {
                if (apiRespoce.StatusCode == System.Net.HttpStatusCode.OK)
                {

                }
                else
                {
                    return Content("Error: " + apiRespoce.StatusCode);
                }


            }




            SqlConnection conn1 = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand sqlcmd = new SqlCommand(String.Format(
                "select * from AddItemToCart where UserName = '{0}'",
                _httpContextAccessor.HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr = sqlcmd.ExecuteReader();

            var orderList = new List<OrderDetails>();
            while (sr.Read())
            {
                OrderDetails orderDetails = new OrderDetails(inVoiceNo, sr["UserName"].ToString(), (int)sr["RestaurantId"], sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], OrderTime);
                orderList.Add(orderDetails);
            }
            conn.Close();

            string items = "";
            string resId = "";

            var httpClient1 = new HttpClient();
            httpClient1.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            JsonContent content2 = JsonContent.Create(orderList);
            using (var apiRespoce = await httpClient1.PostAsync("https://localhost:7021/api/Food/OrderDetails", content2))
            {
                if (apiRespoce.StatusCode == System.Net.HttpStatusCode.OK)
                {

                }
                else
                {
                    return Content("Error: " + apiRespoce.StatusCode);
                }


            }

            _logger.LogDebug(String.Format("Order placed by user {0} of Items: {1} from restaurant Id : {2}", _httpContextAccessor.HttpContext.Session.GetString("UserName"), items, resId));

            HttpClient httpClient2 = new HttpClient();
            httpClient2.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);
            var apiResponce = await httpClient.DeleteAsync("https://localhost:7021/api/Food/DeleteCartItemsByUserName/" + UserName);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return View();
            }
            else
            {
                return Content("Api error " + apiResponce.StatusCode);
            }


        }

        public IActionResult CustomerDetails()
        {
            return View();
        }

        public IActionResult CancelOrder()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from ConfirmOrder where UserName = '{0}'", _httpContextAccessor.HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Order> CancelOrderList = new List<Order>();
            while (sr.Read())
            {
                Order order = new Order(sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Price"], (int)sr["Quantity"], (int)sr["RestaurantId"]);
                CancelOrderList.Add(order);
            }
            conn.Close();

            foreach (var obj in CancelOrderList)
            {
                SqlCommand cmd1 = new SqlCommand(String.Format("insert into CancelOrder values('{0}','{1}','{2}','{3}','{4}')", obj.UserName, obj.FoodItem, obj.Price, obj.Quantity, obj.RestaurantId), conn);
                conn.Open();
                cmd1.ExecuteNonQuery();
                conn.Close();
            }

            SqlCommand cmd2 = new SqlCommand(String.Format("delete from ConfirmOrder where UserName = '{0}'", _httpContextAccessor.HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            cmd2.ExecuteNonQuery();
            conn.Close();

            return View();



        }

        public IActionResult OrderStatus()
        {
            return View();
        }

        public async Task<IActionResult> Status(int Id)
        {
            string? UserName = _httpContextAccessor.HttpContext.Session.GetString("UserName");
            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

            HttpClient httpClient = new HttpClient();
            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });


            /* using(var validationResponce = await httpClient.PostAsync("https://localhost:7021/api/NewAuthentication/validate", content))
             {
                 if(validationResponce.StatusCode== System.Net.HttpStatusCode.BadRequest)
                 {
                     //handle invalid token
                     _logger.LogInformation("Invalid Token");
                     Console.WriteLine("Invalid Token");
                     return RedirectToAction("Login");
                 }

             }*/

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            Console.WriteLine("Id : " + Id);




            if (Id == 1)
            {
                ViewBag.ObjectPassed = "Pending";
            }
            else if (Id == 3)
            {
                ViewBag.ObjectPassed = "Completed";
            }

            List<OrderDetails> res = new List<OrderDetails>();
            //HttpClient httpClient = new HttpClient();
            var apiResponce = await httpClient.GetAsync("https://localhost:7021/api/Food/OrderStatus/" + Id + "/" + UserName);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res1 = await apiResponce.Content.ReadAsStringAsync();
                try
                {
                    res = JsonConvert.DeserializeObject<List<OrderDetails>>(res1);

                }
                catch (Exception e)
                {
                    return Content(e.Message);
                };
                Console.WriteLine(res.Count);
                return View("OrderStatus", res);
            }
            else
            {
                return Content("Api error" + apiResponce.Content.ToString);
            }

        }


        [HttpPost]
        public async Task<IActionResult> Search(IFormCollection col)
        {
            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

            HttpClient httpClient = new HttpClient();
            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });



            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);
            string FoodItem = col["SearchedfoodItem"];
            List<Menu> res = new List<Menu>();
            //HttpClient httpClient = new HttpClient();
            var apiResponce = await httpClient.GetAsync("https://localhost:7021/api/Food/SearchMenuByName/" + FoodItem);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res1 = await apiResponce.Content.ReadAsStringAsync();
                res = JsonConvert.DeserializeObject<List<Menu>>(res1);
                return View("Menu", res);
            }
            else
            {
                return Content("Api error" + apiResponce.StatusCode);
            }
        }




        public IActionResult Dummy()
        {
            return View();
        }



    }
}
