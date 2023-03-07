using FoodDeliveryApplication.Models;
using FoodDeliveryApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Stripe;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace FoodDeliveryApplication.Controllers
{
    [CustomExceptionFilter]
    public class FoodSiteController : Controller
    {

        public List<SignUp> userList = new List<SignUp>();
        public string NotExist = "User Not Exist";
        public string NoUser = "NoUser";
        static string CurrUser;
        public List<FoodItems> FoodItemsSelected = new List<FoodItems>();
        private readonly ILogger<FoodSiteController> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;


        public FoodSiteController(ILogger<FoodSiteController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this._httpContextAccessor = httpContextAccessor;

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
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
            using (var apiRespoce = await httpClient.PostAsync("http://13.233.109.55:8081/api/Food/SignUp", content))
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
            /*
                        var user = userList.Find(e => e.UserName == login.UserName);
                        if (user==null)
                        {
                            ViewBag.NotExist = "User Not Exist";
                            return View();
                        }*/

            JsonContent content = JsonContent.Create(login);
            using (var apiRespoce = await httpClient.PostAsync("http://13.233.109.55:8081/api/NewAuthentication/login", content))
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
                else if (apiRespoce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ViewBag.NotExist = "User Not Exist";
                    return View();
                    //return Content("Error: " + await apiRespoce.Content.ReadAsStringAsync());
                }
                else if (apiRespoce.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    ViewBag.EmptyCredential = "Empty Credential";
                    return View();
                }
                else if (apiRespoce.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ViewBag.IncorrectPassword = "Incorrect Password";
                    return View();
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

            string AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out session data null", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login", "FoodSite");
            }

            JsonContent content = JsonContent.Create(new TokenDto()
            {
                Token = AccessToken,
            });

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            var apiResponce = await httpClient.DeleteAsync("http://13.233.109.55:8081/api/NewAuthentication/logout");

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("{0} Logged Out", _httpContextAccessor.HttpContext.Session.GetString("UserName"));
                _httpContextAccessor.HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }
            else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login");
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

                using (var apiResponce = await httpClient.GetAsync("http://13.233.109.55:8081/api/Food/GetAllRestaurants"))
                {

                    if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string res1 = await apiResponce.Content.ReadAsStringAsync();
                        res = JsonConvert.DeserializeObject<List<Restaurants>>(res1);
                        return View("Restaurants", res);
                    }
                    else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        return Content("Api error" + apiResponce.StatusCode);
                    }

                }
            

        }

        public async Task<IActionResult> PureVegRestaurants()
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

            using (var apiResponce = await httpClient.GetAsync("http://13.233.109.55:8081/api/Food/GetAllVegRestaurants"))
            {

                if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string res1 = await apiResponce.Content.ReadAsStringAsync();
                    res = JsonConvert.DeserializeObject<List<Restaurants>>(res1);
                    return View("Restaurants", res);
                }
                else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    return Content("Api error" + apiResponce.StatusCode);
                }

            }

        }


        public async Task<IActionResult> GetRestaurantsByCuisine(string cuisine)
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

            using (var apiResponce = await httpClient.GetAsync("http://13.233.109.55:8081/api/Food/GetRestaurantByCuisine?cuisine=" + cuisine))
            {

                if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string res1 = await apiResponce.Content.ReadAsStringAsync();
                    res = JsonConvert.DeserializeObject<List<Restaurants>>(res1);
                    return View("Restaurants", res);
                }
                else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login");
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
            var apiResponce = await httpClient.GetAsync("http://13.233.109.55:8081/api/Food/GetRestaurantMenuById/" + Id);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res1 = await apiResponce.Content.ReadAsStringAsync();
                res = JsonConvert.DeserializeObject<List<Menu>>(res1);
                return View("Menu", res);
            }
            else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login");
            }
            else
            {
                return Content("Api error" + apiResponce.StatusCode);
            }

        }


        public IActionResult RestaurantMenuVeg(string type, int Id)
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

            //List<Menu> res = new List<Menu>();



            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select *  from Food where Restaurant_Id={0} and FoodType='{1}' ", Id, type), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            var model = new List<Menu>();

            while (sr.Read())
            {
                int id = (int)sr["Id"];
                Menu menu = new Menu(
                    4,
                    sr["Food_Image"].ToString(),
                    sr["Food_Item"].ToString(),
                    (int)sr["Price"],
                    (int)sr["Restaurant_Id"]);

                Console.WriteLine(id);
                menu.Id = id;
                model.Add(menu);
            }

            ViewBag.veg = "veg";

            return View("Menu", model);
        }


        public IActionResult Order()
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }
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
            cart.FoodId = Food_Id;

            _logger.LogInformation("Item:{0} added to cart by the user:{1} of Quantity:{2}", Food_Item, _httpContextAccessor.HttpContext.Session.GetString("UserName"), Quantity);


            JsonContent content1 = JsonContent.Create(cart);
            using (var apiRespoce = await httpClient.PostAsync("http://13.233.109.55:8081/api/Food/AddToCart", content1))
            {
                if (apiRespoce.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    TempData["success"] = "Item Added to Cart ";

                    return RedirectToAction("RestaurantMenu", new { Id = Restaurant_Id });

                }
                else if (apiRespoce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login");
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
            var apiResponce = await httpClient.DeleteAsync("http://13.233.109.55:8081/api/Food/DeleteCartItemById/" + Id);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TempData["success"] = "Item Removed From Cart";
                return RedirectToAction("Cart");
            }
            else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login");
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
            var apiResponce = await httpClient.GetAsync("http://13.233.109.55:8081/api/Food/GetCartByUserName?UserName=" + UserName);

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
        public async Task<IActionResult> PlaceOrder(IFormCollection col,string stripeEmail, string stripeToken,string mail)
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
            System.Diagnostics.Debug.WriteLine(mail);

            if (mail == "cardpayment")
            {
                StripeConfiguration.ApiKey = "sk_test_51McNZISCWI76a6DvS3FGhnCqMuvID4bIg4RgVwY2R3nCxmcCi5uAsbEPDW9mzaRYPehAM4lKAB4pLe4wWEqXfPSM00hJONI73E";
                SqlConnection con = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand sq = new SqlCommand(String.Format(
                    "select * from AddItemToCart where UserName = '{0}'",
                    _httpContextAccessor.HttpContext.Session.GetString("UserName")), con);
                con.Open();
                SqlDataReader s = sq.ExecuteReader();


                int totalPrice = 0;
                while (s.Read())
                {
                    totalPrice += Convert.ToInt32(s["Price"]) * Convert.ToInt32(s["Quantity"]);

                }
                con.Close();
                TokenService tokenService = new TokenService();
                Token token = tokenService.Get(stripeToken);
                string four = token.Card.Last4;
                string method="";
                switch (four)
                {
                    case "4242": 
                        method = "pm_card_visa";
                        break;
                    case "5556":
                        method = "pm_card_visa_debit";
                        break;
                    case "4444":
                        method = "pm_card_mastercard";
                        break;
                    case "8210":
                        method = "pm_card_mastercard_debit";
                        break;
                    case "5100":
                        method = "pm_card_mastercard_prepaid";
                        break;
                    case "0005":
                        method = "pm_card_amex";
                        break;
                    case "8431":
                        method = "pm_card_amex";
                        break;
                    case "1117":
                        method = "pm_card_discover";
                        break;
                    case "9424":
                        method = "pm_card_discover";
                        break;
                    case "0004":
                        method = "pm_card_diners";
                        break;
                    case "0505":
                        method = "pm_card_jcb";
                        break;
                   
                    default:
                        method = "pm_card_visa";
                        break;
                }

                var options1 = new CustomerListOptions
                {
                    Limit = 20,
                };
                var service1 = new CustomerService();
                StripeList<Customer> customers = service1.List(
                  options1);
                bool value = true;
                Customer customer1 = null;
                foreach (var i in customers)
                {
                    if (i.Name == _httpContextAccessor.HttpContext.Session.GetString("UserName") && i.Email == stripeEmail)
                    {
                        customer1 = i;
                        value = false;
                    }
                }
                if (value)
                {


                    var optionCust = new CustomerCreateOptions
                    {
                        Email = stripeEmail,
                        Name = _httpContextAccessor.HttpContext.Session.GetString("UserName"),
                        Phone= col["PhoneNo"]


                    };
                    var serviceCust = new CustomerService();
                    customer1 = serviceCust.Create(optionCust);
                    
                }
                var options = new PaymentIntentCreateOptions
                {
                    Customer = customer1.Id,
                    PaymentMethod =method,
                    PaymentMethodTypes = new List<string> { "card" },  
                    Amount = Convert.ToInt64(totalPrice*100),
                    Currency = "inr",
                    Description = "Stripe transaction",
                    Confirm = true,
                    OffSession = true,        
                    ReceiptEmail = stripeEmail
                };
                var service = new PaymentIntentService();
                
                var paymentIntent = service.Create(options);
            }

            Random rnd = new Random();
            int inVoiceNo = rnd.Next(10000, 10000000);
            DateTime utc = DateTime.UtcNow;

            DateTime temp = new DateTime(utc.Ticks, DateTimeKind.Utc);
            DateTime OrderTime = TimeZoneInfo.ConvertTimeFromUtc(temp, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            //DateTime OrderTime = DateTime.Now;


            /*   string address = details.Address;
               string phoneNo = details.PhoneNo;*/

            string address = col["add"];
            string phoneNo = col["PhoneNo"];



            Console.WriteLine(OrderTime);

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            System.Diagnostics.Debug.WriteLine(col["city"]);
            System.Diagnostics.Debug.WriteLine(col["state"]);
            System.Diagnostics.Debug.WriteLine(col["zip"]);
            OrderPlaced order = new OrderPlaced();
            order.Address = address;
            order.InVoiceNo = inVoiceNo;
            order.PhoneNo = phoneNo;
            order.UserName = UserName;
            order.OrderTime = OrderTime;
            /*order.City = details.City;
            order.State = details.State;
            order.Zipcode = details.Zipcode;
            order.CardNo = details.CardNo;
            order.ExpMonth = details.ExpMonth;
            order.ExpYear = details.ExpYear;
            order.CVV = details.CVV;*/

            order.City = col["city"];
            order.State = col["state"];
            order.Zipcode = col["zip"];
            order.CardNo = col["card"];
            order.ExpMonth = col["expmonth"];
            order.ExpYear = col["expyear"];
            order.CVV = Convert.ToInt32(col["cvv"].ToString());
            order.status = "Placed";
            
            JsonContent content1 = JsonContent.Create(order);
           
            using (var apiRespoce = await httpClient.PostAsync("http://13.233.109.55:8081/api/Food/Orders", content1))
            {
                if (apiRespoce.StatusCode == System.Net.HttpStatusCode.OK)
                {


                    SqlConnection conn2 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                    SqlCommand cmd = new SqlCommand(String.Format("insert into User_Address values('{0}','{1}','{2}','{3}','{4}')", _httpContextAccessor.HttpContext.Session.GetString("UserName"), address, order.City, order.State, order.Zipcode), conn2);
                    conn2.Open();
                    cmd.ExecuteNonQuery();
                    conn2.Close();
                    if(order.CVV != 0)
                    {
                        SqlConnection conn3 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                        SqlCommand cmd1 = new SqlCommand(String.Format("insert into User_Cards values('{0}','{1}','{2}','{3}')", _httpContextAccessor.HttpContext.Session.GetString("UserName"), order.CardNo, order.ExpMonth, order.ExpYear), conn3);
                        conn3.Open();
                        cmd1.ExecuteNonQuery();
                        conn3.Close();
                    }
                    

                    if (order.CVV == 0)
                    {
                        TempData["success"] = "Order placed";
                    }
                    else
                    {
                        TempData["success"] = "Payment done successfully";
                    }

                }
                else if (apiRespoce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    return Content("Error:kk " + apiRespoce.StatusCode);
                }


            }




            SqlConnection conn1 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand sqlcmd = new SqlCommand(String.Format(
                "select * from AddItemToCart where UserName = '{0}'",
                _httpContextAccessor.HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr = sqlcmd.ExecuteReader();

            var orderList = new List<OrderDetails>();
            string item = "";
            string resId = "";
            string userName = "";
            HashSet<int> ResIds = new HashSet<int>();
            while (sr.Read())
            {
                OrderDetails orderDetails = new OrderDetails(inVoiceNo, sr["UserName"].ToString(), (int)sr["RestaurantId"], sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], OrderTime,"Placed");
                orderList.Add(orderDetails);
                ResIds.Add(orderDetails.RestaurantId);
                userName = orderDetails.UserName;
            }
            conn.Close();
            foreach(var r in ResIds)
            {
                List<OrderedItems> items = new List<OrderedItems>();
                foreach (var o in orderList)
                {
                    if (o.RestaurantId == r)
                    {
                        items.Add(new OrderedItems(o.FoodItem, o.Quantity, o.Price));
                    }
                }
                SqlConnection conn2 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd2 = new SqlCommand(string.Format("select Restaurant_Name from restaurants where Restaurant_Id='{0}'",r),conn2);
                conn2.Open();
                string RestaurantName = "";
                SqlDataReader sr2 = cmd2.ExecuteReader();
                while (sr2.Read())
                {
                    RestaurantName = sr2["Restaurant_Name"].ToString();
                }
               
                conn2.Close();

                SqlConnection conn3 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd3 = new SqlCommand(string.Format("select Email from RestaurantLoginDetails where RestaurantName='{0}'", RestaurantName), conn3);
                conn3.Open();
                SqlDataReader sr3 = cmd3.ExecuteReader();
                string RestaurantMail = "";
                while (sr3.Read())
                {
                    RestaurantMail = sr3["Email"].ToString();
                }
             
                     conn3.Close();

                string fromMail = "notify.justeat@gmail.com";
                string fromPassword = "rtezlcgmuwkossoz";
                
                string fp = string.Format("<h2 style=\"color:orange; text-align:center; font-size:25px;\">JustEat</h2><hr/><p>Dear <span style=\"font-weight:bold;\">{0}</span>,</p><p>This is to inform you that we have received an order.</p><p>Order Summary:</p><p>Invoice No: <span style=\"font-weight:bold;\">{1}</span></p><p>Order Placed at: <span style=\"font-weight:bold;\">{2}</span></p><p>Ordered By:</p><p><span style=\"font-weight:bold; text-transform: uppercase;\">{3}</span></p><p>{4}, {5}<br/> {6} {7}, India</p> </body></html>", RestaurantName, inVoiceNo, OrderTime, userName, address, col["city"], col["state"], col["zip"]);
                string lp1 = "<table style=\"border-collapse: collapse; width:65vw;\"><tr ><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Item Name</th><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Quantity</th><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Price</th></tr>";
                int totalPrice = 0;
                foreach(var i in items)
                {
                    totalPrice+=i.Price*i.Quantity;
                    lp1+=string.Format("<tr ><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">{0}</td><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">{1}</td><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">₹ {2}</td></tr>", i.Item,i.Quantity,i.Price);
                }
                string lp2 = "</table>";
                string lp3 = string.Format("<p style=\"width:65vw; color:#3c9961; text-align:end; background-color:#f0f5f1; padding:7px; font-weight:bold;\">Order Total :  <span style=\"padding-left:10px;\">₹ {0}</span></p><hr/><p style=\"padding:10px;\">Request you to take necessary action for the order above by login to the application. Thankyou</p><hr/>", totalPrice);
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                string n = "j\"fdf\"";
                message.Subject = String.Format("You Received an Order #{0}", inVoiceNo);
                message.To.Add(new MailAddress(RestaurantMail));
                message.Body = string.Format("<html><body>{0}{1}{2}{3} </body></html>", fp,lp1,lp2,lp3);
                message.IsBodyHtml = true;
                
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);

            }


            var httpClient1 = new HttpClient();
            httpClient1.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            JsonContent content2 = JsonContent.Create(orderList);
            using (var apiRespoce = await httpClient1.PostAsync("http://13.233.109.55:8081/api/Food/OrderDetails", content2))
            {
                if (apiRespoce.StatusCode == System.Net.HttpStatusCode.OK)
                {

                }
                else if (apiRespoce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    return Content("Error:mm " + apiRespoce.StatusCode);
                }


            }

            _logger.LogDebug(String.Format("Order placed by user {0} of Items: {1} from restaurant Id : {2}", _httpContextAccessor.HttpContext.Session.GetString("UserName"), item, resId));

            

            HttpClient httpClient2 = new HttpClient();
            httpClient2.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);
            var apiResponce = await httpClient.DeleteAsync("http://13.233.109.55:8081/api/Food/DeleteCartItemsByUserName/" + UserName);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {


                return View(orderList);



            }
            else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login");
            }
            else
            {
                return Content("Api error " + apiResponce.StatusCode);
            }


        }


        public IActionResult CustomerDetails()
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

            string name = _httpContextAccessor.HttpContext.Session.GetString("UserName");
            /* SqlConnection conn4 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
             SqlCommand cmd4 = new SqlCommand(String.Format("select U.Email, O.Address, O.PhoneNo, O.City, O.State, O.ZipCode, O.CreditCardNo, O.ExpMonth, O.ExpYear, O.cvv from Users U , Orders O where O.UserName = U.UserName and O.UserName = '{0}'", _httpContextAccessor.HttpContext.Session.GetString("UserName")), conn4);
             conn4.Open();
             SqlDataReader sr = cmd4.ExecuteReader();
             List<Profile> profile = new List<Profile>();
             while (sr.Read())
             {
                 profile.Add(new Profile(name, sr["PhoneNo"].ToString(), sr["Email"].ToString(), sr["Address"].ToString(), sr["City"].ToString(), sr["State"].ToString(), sr["ZipCode"].ToString(), sr["CreditCardNo"].ToString(), sr["ExpMonth"].ToString(), sr["ExpYear"].ToString(), (int)sr["cvv"]));
             }
             Console.WriteLine("count " + profile.Count());


             conn4.Close();
 */
            SqlConnection conn1 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand sqlcmd = new SqlCommand(String.Format(
                "select * from AddItemToCart where UserName = '{0}'",
                _httpContextAccessor.HttpContext.Session.GetString("UserName")), conn1);
            conn1.Open();
            SqlDataReader sr1 = sqlcmd.ExecuteReader();


            int total = 0;
            while (sr1.Read())
            {
                total += Convert.ToInt32(sr1["Price"]) * Convert.ToInt32(sr1["Quantity"]);

            }
            conn1.Close();
            ViewBag.Total = total;



            SqlConnection conn4 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd4 = new SqlCommand(String.Format("select A.UserName, A.Address, A.City, A.State, A.ZipCode, C.CardNo, C.ExpMonth, C.ExpYear from User_Address A , User_Cards C where A.UserName=C.UserName and A.UserName = '{0}'", _httpContextAccessor.HttpContext.Session.GetString("UserName")), conn4);
            conn4.Open();
            SqlDataReader sr = cmd4.ExecuteReader();
            List<Profile> profile = new List<Profile>();

            while (sr.Read())
            {
                profile.Add(new Profile(name, sr["Address"].ToString(), sr["City"].ToString(), sr["State"].ToString(), sr["ZipCode"].ToString(), sr["CardNo"].ToString(), sr["ExpMonth"].ToString(), sr["ExpYear"].ToString()));


            }
            Console.WriteLine("count " + profile.Count());


            conn4.Close();
            //return View(profile);

            return View("Payment",profile);
        }


        public IActionResult OrderStatus()
        {
            string? UserName = _httpContextAccessor.HttpContext.Session.GetString("UserName");
            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                return RedirectToAction("Login");
            }

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


            /* using(var validationResponce = await httpClient.PostAsync("http://13.233.109.55:8081/api/NewAuthentication/validate", content))
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
            var apiResponce = await httpClient.GetAsync("http://13.233.109.55:8081/api/Food/OrderStatus/" + Id + "/" + UserName);

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
                //return View("Dummy", res);
            }
            else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login");
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
                ViewBag.NoUser = "NoUser";
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
            var apiResponce = await httpClient.GetAsync("http://13.233.109.55:8081/api/Food/SearchMenuByName/" + FoodItem);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res1 = await apiResponce.Content.ReadAsStringAsync();
                res = JsonConvert.DeserializeObject<List<Menu>>(res1);
                return View("Menu", res);
            }
            else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login");
            }
            else
            {
                return Content("Api error" + apiResponce.StatusCode);
            }
        }

        public IActionResult ConfirmCancel(int id)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            
            var orderlist = new List<OrderDetails>();

            
                SqlCommand cmd = new SqlCommand(String.Format("Select * from PlacedOrderDetail where InVoiceNo = '{0}'", id), conn);
                conn.Open();
                SqlDataReader sr = cmd.ExecuteReader();

                while (sr.Read())
                {
                    string time = sr["OrderTime"].ToString();
                    DateTime orderTime = Convert.ToDateTime(time);
                    OrderDetails orderDetails = new OrderDetails((int)sr["InVoiceNo"], sr["UserName"].ToString(), (int)sr["RestaurantId"], sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], orderTime, sr["status"].ToString());
                    orderlist.Add(orderDetails);
                }
                conn.Close();
                
            
            ViewBag.cancel = id.ToString();
            return View(orderlist);
        }
        
        public IActionResult CancelOrder(int id)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }
            List<OrderDetails> details = new List<OrderDetails>();
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");           
            SqlCommand cmd = new SqlCommand(String.Format("select * from PlacedOrderDetail where InVoiceNo='{0}'", id), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            DateTime utc = DateTime.UtcNow;

            DateTime temp = new DateTime(utc.Ticks, DateTimeKind.Utc);
            DateTime ist = TimeZoneInfo.ConvertTimeFromUtc(temp, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            while (sr.Read())
            {
                details.Add(new OrderDetails((int)sr["InVoiceNo"],sr["UserName"].ToString(),(int)sr["RestaurantId"],sr["FoodItem"].ToString(),(int)sr["Quantity"],(int)sr["Price"],ist,sr["status"].ToString()));
            }
            conn.Close();
            HashSet<int> resId = new HashSet<int>();
            foreach (var obj in details)
            {
                resId.Add(obj.RestaurantId);
                SqlConnection conn1 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd1 = new SqlCommand(String.Format("insert into CompletedOrder values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", obj.InVoiceNo, obj.UserName, obj.FoodItem, obj.Quantity, obj.Price, ist.ToString("yyyy-MM-dd HH:mm:ss"), obj.RestaurantId, "Cancelled"), conn1);
                conn1.Open();

                cmd1.ExecuteNonQuery();
                conn1.Close();

                SqlCommand cmd2 = new SqlCommand(String.Format("delete from PlacedOrderDetail where InVoiceNo = '{0}'", id), conn);
                conn.Open();
                cmd2.ExecuteNonQuery();
                conn.Close();
            }

            foreach (var r in resId)
            {
                string userName = "";
                int inVoiceNo = 0;
                DateTime orderPlaced = DateTime.Now;
                List<OrderedItems> items = new List<OrderedItems>();
                foreach (var o in details)
                {
                    if (o.RestaurantId == r)
                    {
                        userName=o.UserName;
                        orderPlaced = o.OrderTime;
                        inVoiceNo=o.InVoiceNo;
                        items.Add(new OrderedItems(o.FoodItem, o.Quantity, o.Price));
                    }
                }
                SqlConnection conn2 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd2 = new SqlCommand(string.Format("select Restaurant_Name from restaurants where Restaurant_Id='{0}'", r), conn2);
                conn2.Open();
                string RestaurantName = "";
                SqlDataReader sr2 = cmd2.ExecuteReader();
                while (sr2.Read())
                {
                    RestaurantName = sr2["Restaurant_Name"].ToString();
                }

                conn2.Close();

                SqlConnection conn3 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd3 = new SqlCommand(string.Format("select Email from RestaurantLoginDetails where RestaurantName='{0}'", RestaurantName), conn3);
                conn3.Open();
                SqlDataReader sr3 = cmd3.ExecuteReader();
                string RestaurantMail = "";
                while (sr3.Read())
                {
                    RestaurantMail = sr3["Email"].ToString();
                }

                conn3.Close();

                SqlCommand cmd7 = new SqlCommand(string.Format("select * from Orders where InVoiceNo ='{0}'", inVoiceNo), conn3);
                conn3.Open();
                SqlDataReader sr5 = cmd7.ExecuteReader();
                string address = "";
                string city = "";
                string state = "";
                string zipcode = "";
                string orderTime = "";
                while (sr5.Read())
                {
                    orderTime = sr5["OrderTime"].ToString();
                    address = sr5["Address"].ToString();
                    city = sr5["City"].ToString();
                    state = sr5["State"].ToString();
                    zipcode = sr5["ZipCode"].ToString();
                }
                conn3.Close();

                string fromMail = "notify.justeat@gmail.com";
                string fromPassword = "rtezlcgmuwkossoz";

                string fp = string.Format("<h2 style=\"color:orange; text-align:center; font-size:25px;\">JustEat</h2><hr/><p>Dear <span style=\"font-weight:bold;\">{0}</span>,</p><p>This is to inform you that the customer cancelled the order.</p><p>Order Summary:</p><p>Invoice No: <span style=\"font-weight:bold;\">{1}</span></p><p>Order Placed at: <span style=\"font-weight:bold;\">{2}</span></p><p>Order Cancelled at: <span style=\"font-weight:bold;\">{3}</span></p><p>Order Status: <span style=\"font-weight:bold; color:red;\">Cancelled</span></p><p>Ordered By:</p><p><span style=\"font-weight:bold; text-transform: uppercase;\">{4}</span></p><p>{5}, {6}<br/> {7} {8}, India</p> </body></html>", RestaurantName, inVoiceNo, orderPlaced, ist.ToString("dd-MM-yyyy HH:mm:ss"), userName, address, city, state, zipcode);
                string lp1 = "<table style=\"border-collapse: collapse; width:65vw;\"><tr ><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Item Name</th><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Quantity</th><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Price</th></tr>";
                int totalPrice = 0;
                foreach (var i in items)
                {
                    totalPrice += i.Price * i.Quantity;
                    lp1 += string.Format("<tr ><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">{0}</td><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">{1}</td><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">₹ {2}</td></tr>", i.Item, i.Quantity, i.Price);
                }
                string lp2 = "</table>";
                string lp3 = string.Format("<p style=\"width:65vw; color:#3c9961; text-align:end; background-color:#f0f5f1; padding:7px; font-weight:bold;\">Order Total :  <span style=\"padding-left:10px;\">₹ {0}</span></p><hr/><p style=\"padding:10px;\">Thankyou for choosing JustEat. Have a nice day.</p><hr/>", totalPrice);
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                string n = "j\"fdf\"";
                message.Subject = String.Format("Order #{0} was cancelled", inVoiceNo);
                message.To.Add(new MailAddress(RestaurantMail));
                message.Body = string.Format("<html><body>{0}{1}{2}{3} </body></html>", fp, lp1, lp2, lp3);
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);
            }

                SqlConnection conn4 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd4 = new SqlCommand(String.Format("update Orders set status='Cancelled' where OrderId = '{0}'", id), conn4);
            conn4.Open();
            cmd4.ExecuteNonQuery();
            conn4.Close();

            return RedirectToAction("OrderStatus");


            
        }

        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }

            string name = _httpContextAccessor.HttpContext.Session.GetString("UserName");

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select Email from Users where UserName = '{0}'",_httpContextAccessor.HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr1 = cmd.ExecuteReader();
            string Email="";
            while(sr1.Read())
            {
                Email = sr1["Email"].ToString();
            }

            SqlConnection conn4 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd4 = new SqlCommand(String.Format("select A.UserName, A.Address, A.City, A.State, A.ZipCode, C.CardNo, C.ExpMonth, C.ExpYear from User_Address A , User_Cards C where A.UserName=C.UserName and A.UserName = '{0}'",_httpContextAccessor.HttpContext.Session.GetString("UserName")), conn4);
            conn4.Open();
            SqlDataReader sr = cmd4.ExecuteReader();
            List<Profile> profile = new List<Profile>();

                while(sr.Read())
                {
                        profile.Add(new Profile(name, sr["Address"].ToString(), sr["City"].ToString(), sr["State"].ToString(), sr["ZipCode"].ToString(), sr["CardNo"].ToString(), sr["ExpMonth"].ToString(), sr["ExpYear"].ToString()));
                        
  
                }
                Console.WriteLine("count " + profile.Count());


                conn4.Close();
            ViewBag.Email = Email;

            return View(profile);
           
        }

        public IActionResult Dummy()
        {
            string name = _httpContextAccessor.HttpContext.Session.GetString("UserName");
            SqlConnection conn4 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd4 = new SqlCommand(String.Format("select U.Email, O.Address, O.PhoneNo, O.City, O.State, O.ZipCode, O.CreditCardNo, O.ExpMonth, O.ExpYear, O.cvv from Users U , Orders O where O.UserName = U.UserName and O.UserName = '{0}'", _httpContextAccessor.HttpContext.Session.GetString("UserName")), conn4);
            conn4.Open();
            SqlDataReader sr = cmd4.ExecuteReader();
            List<Profile> profile = new List<Profile>();
            while (sr.Read())
            {
                profile.Add(new Profile(name, sr["PhoneNo"].ToString(), sr["Email"].ToString(), sr["Address"].ToString(), sr["City"].ToString(), sr["State"].ToString(), sr["ZipCode"].ToString(), sr["CreditCardNo"].ToString(), sr["ExpMonth"].ToString(), sr["ExpYear"].ToString(), (int)sr["cvv"]));
            }
            Console.WriteLine("count " + profile.Count());


            conn4.Close();

            return View(profile);
        }

        public IActionResult AddAddress()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddAddress(IFormCollection col)
        {
            string address = col["Address"];
            string city = col["City"];
            string state = col["State"];
            string zipcode = col["Zipcode"];
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("insert into User_Address values('{0}','{1}','{2}','{3}','{4}')",_httpContextAccessor.HttpContext.Session.GetString("UserName"),address,city,state,zipcode),conn);
            
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            TempData["success"] = "Address added successfully";
            return RedirectToAction("Profile");
        }

        public IActionResult AddCards()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCards(IFormCollection col)
        {
            string card = col["Card"];
            string month = col["Month"];
            string year = col["Year"];
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("insert into User_Cards values('{0}','{1}','{2}','{3}')", _httpContextAccessor.HttpContext.Session.GetString("UserName"), card, month, year), conn);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            TempData["success"] = "Card added successfully";
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> SearchByImage(string id)
        {
            string? AccessToken = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            if (_httpContextAccessor.HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                Console.WriteLine("Logout");
                ViewBag.NoUser = "NoUser";
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
            var apiResponce = await httpClient.GetAsync("http://13.233.109.55:8081/api/Food/SearchMenuByName/" + id);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res1 = await apiResponce.Content.ReadAsStringAsync();
                res = JsonConvert.DeserializeObject<List<Menu>>(res1);
                return View("Menu", res);
            }
            else if (apiResponce.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login");
            }
            else
            {
                return Content("Api error" + apiResponce.StatusCode);
            }
        }


        public IActionResult UpdateCart(IFormCollection col)
        {
            int quantity = Convert.ToInt32(col["Quantity"]);
            int itemNo = Convert.ToInt32(col["ItemNo"]);
          
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("update AddItemToCart set Quantity ='{0}' where ItemNo = '{1}' and UserName = '{2}'", quantity,itemNo, _httpContextAccessor.HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            return RedirectToAction("Cart");
        }



    }
}
