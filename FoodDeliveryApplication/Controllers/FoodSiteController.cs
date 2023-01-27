using FoodDeliveryApplication.Models;
using FoodDeliveryApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.Data.SqlClient;
using System.Linq;

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
            using (var apiRespoce = await httpClient.PostAsync("http://52.66.208.30:8081/api/Food/SignUp", content))
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
            using (var apiRespoce = await httpClient.PostAsync("http://52.66.208.30:8081/api/NewAuthentication/login", content))
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

            var apiResponce = await httpClient.DeleteAsync("http://52.66.208.30:8081/api/NewAuthentication/logout");

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("{0} Logged Out", _httpContextAccessor.HttpContext.Session.GetString("UserName"));
                _httpContextAccessor.HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
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

                using (var apiResponce = await httpClient.GetAsync("http://52.66.208.30:8081/api/Food/GetAllRestaurants"))
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

            using (var apiResponce = await httpClient.GetAsync("http://52.66.208.30:8081/api/Food/GetAllVegRestaurants"))
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
            var apiResponce = await httpClient.GetAsync("http://52.66.208.30:8081/api/Food/GetRestaurantMenuById/" + Id);

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
            using (var apiRespoce = await httpClient.PostAsync("http://52.66.208.30:8081/api/Food/AddToCart", content1))
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
            var apiResponce = await httpClient.DeleteAsync("http://52.66.208.30:8081/api/Food/DeleteCartItemById/" + Id);

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
            var apiResponce = await httpClient.GetAsync("http://52.66.208.30:8081/api/Food/GetCartByUserName?UserName=" + UserName);

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

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");


            OrderPlaced order = new OrderPlaced();
            order.Address = address;
            order.InVoiceNo = inVoiceNo;
            order.PhoneNo = phoneNo;
            order.UserName = UserName;
            order.OrderTime = OrderTime;
            order.City = details.City;
            order.State = details.State;
            order.Zipcode = details.Zipcode;
            order.CardNo = details.CardNo;
            order.ExpMonth = details.ExpMonth;
            order.ExpYear = details.ExpYear;
            order.CVV = details.CVV;
            order.status = "Placed";

            JsonContent content1 = JsonContent.Create(order);
            using (var apiRespoce = await httpClient.PostAsync("http://52.66.208.30:8081/api/Food/Orders", content1))
            {
                if (apiRespoce.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (order.CVV == 0)
                    {
                        TempData["success"] = "Order placed";
                    }
                    else
                    {
                        TempData["success"] = "Payment done successfully";
                    }

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
            while (sr.Read())
            {
                OrderDetails orderDetails = new OrderDetails(inVoiceNo, sr["UserName"].ToString(), (int)sr["RestaurantId"], sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], OrderTime,"Placed");
                orderList.Add(orderDetails);
            }
            conn.Close();

            string items = "";
            string resId = "";

            var httpClient1 = new HttpClient();
            httpClient1.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", AccessToken);

            JsonContent content2 = JsonContent.Create(orderList);
            using (var apiRespoce = await httpClient1.PostAsync("http://52.66.208.30:8081/api/Food/OrderDetails", content2))
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
            var apiResponce = await httpClient.DeleteAsync("http://52.66.208.30:8081/api/Food/DeleteCartItemsByUserName/" + UserName);

            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {


                return View(orderList);



            }
            else
            {
                return Content("Api error " + apiResponce.StatusCode);
            }


        }




        public IActionResult CustomerDetails()
        {
            return View("Payment");
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


            /* using(var validationResponce = await httpClient.PostAsync("http://52.66.208.30:8081/api/NewAuthentication/validate", content))
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
            var apiResponce = await httpClient.GetAsync("http://52.66.208.30:8081/api/Food/OrderStatus/" + Id + "/" + UserName);

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
            var apiResponce = await httpClient.GetAsync("http://52.66.208.30:8081/api/Food/SearchMenuByName/" + FoodItem);

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
            while(sr.Read())
            {
                details.Add(new OrderDetails((int)sr["InVoiceNo"],sr["UserName"].ToString(),(int)sr["RestaurantId"],sr["FoodItem"].ToString(),(int)sr["Quantity"],(int)sr["Price"],DateTime.Now,sr["status"].ToString()));
            }
            conn.Close();

            foreach (var obj in details)
            {
                SqlConnection conn1 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd1 = new SqlCommand(String.Format("insert into CompletedOrder values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", obj.InVoiceNo, obj.UserName, obj.FoodItem, obj.Quantity, obj.Price, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), obj.RestaurantId, "Cancelled"), conn1);
                conn1.Open();

                cmd1.ExecuteNonQuery();
                conn1.Close();

                SqlCommand cmd2 = new SqlCommand(String.Format("delete from PlacedOrderDetail where InVoiceNo = '{0}'", id), conn);
                conn.Open();
                cmd2.ExecuteNonQuery();
                conn.Close();
            }



            return RedirectToAction("OrderStatus");



        }

        public IActionResult Dummy()
        {
            return View();
        }



    }
}
