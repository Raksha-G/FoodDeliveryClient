using FoodDeliveryApplication.Models;
using FoodDeliveryApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Data.SqlClient;


namespace FoodDeliveryApplication.Controllers
{
    public class FoodSiteController : Controller
    {

        public List<SignUp> userList = new List<SignUp>();
        public string NotExist = "User Not Exist";
        public List<FoodItems> FoodItemsSelected =  new List<FoodItems>();
       
        public FoodSiteController()
        {

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
        public IActionResult CreateAccount(SignUp signup)
        {
                var user = userList.Find(e => e.UserName == signup.UserName);
                if (user != null)
                {
                    ViewBag.UserName = "UserName already Exist";
                    return View();
                }

                SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=True;");
                SqlCommand cmd = new SqlCommand(String.Format("insert into Users values('{0}','{1}','{2}')", signup.UserName, signup.Email, signup.Password), conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Log.Information(String.Format("A new Account Created with UserName {0}", signup.UserName));
            
            return View("Login");

        }

        public IActionResult Login()
        {
            return View();
        }
        
        
        [HttpPost]
        public IActionResult Login(LoginDetails login)
        {
           
                var user = userList.Find(e => e.UserName == login.UserName);
                if (user == null)
                {
                    ViewBag.NotExist = NotExist;
                    return View();
                }
                foreach (var i in userList)
                {
                    if (i.UserName == login.UserName && i.Password == login.Password)
                    {
                        Log.Information(String.Format("{0} Logged in", login.UserName));
                        HttpContext.Session.SetString("UserName", login.UserName);
                        return RedirectToAction("Restaurants");
                    }
                }

                ViewBag.IncorrectPassword = "Incorrect Password";
       
            return View();

        }

        public IActionResult Logout()
        {
            return View("Login");
        }

     

        public IActionResult Menu()
        {
            
            return View();
        }

        
       



      

        //[HttpPost]
        public IActionResult Restaurants()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True; ");
            SqlCommand cmd = new SqlCommand("select * from Restaurants", conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Restaurants> res = new List<Restaurants>();
            while(sr.Read())
            {
               Restaurants restaurant = new Restaurants((int)sr["Restaurant_Id"], sr["Restaurant_Name"].ToString(), sr["Restaurant_Image"].ToString());
                res.Add(restaurant);
            }
           
            return View("Restaurants",res);
        }




        public IActionResult RestaurantMenu(int Id)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }
            Console.WriteLine("ResId" + Id);

            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand cmd = new SqlCommand(String.Format("select *  from Food where Restaurant_Id={0}", Id), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();


            //string str;
            HttpContext.Session.SetInt32("ResId", Id);
         
            var model = new List<Menu>();

            while (sr.Read())
            {
                Menu menu = new Menu((int)sr["Id"], sr["Food_Image"].ToString(), sr["Food_Item"].ToString(), (int)sr["Price"], (int)sr["Restaurant_Id"]);
                int id = (int)sr["Id"];
                Console.WriteLine(id);
                model.Add(menu);
            }
           
           

           

            return View("Menu", model);
        }



        public IActionResult Order()
        {
            return Content(HttpContext.Session.GetString("UserName"));

        }

        [HttpPost]
        public IActionResult AddToCart(IFormCollection col)
        {

            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }
            Console.WriteLine(col["Food_Item"]);
            //Console.WriteLine("Food Id :" +col["Food_Id"]);
            var Food_Item = col["Food_Item"];
            var Quantity = col["Quantity"];
            var Restaurant_Id = col["RestaurantId"];
            var Food_Id = Convert.ToInt32(col["Food_Id"]);
            var Price = Convert.ToInt32(col["Price"]);
            

            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand cmd = new SqlCommand(String.Format("insert into AddItemToCart values('{0}','{1}','{2}','{3}','{4}')", HttpContext.Session.GetString("UserName"), Food_Item, Quantity,Restaurant_Id,Price), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();



            return RedirectToAction("RestaurantMenu",new { Id = Restaurant_Id });

          

        }


        public IActionResult Cart()
        {
            if(HttpContext.Session.GetString("UserName")==null)
            {
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand cmd = new SqlCommand(String.Format("select A.FoodItem, A.Quantity, F.Food_Image,F.Price,F.Id,F.Restaurant_Id from AddItemToCart A inner join Food F on F.Food_Item = A.FoodItem where A.UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            List<Cart> cart = new List<Cart>();

            while(sr.Read())
            {
                Cart cartItem = new Cart(HttpContext.Session.GetString("UserName"), sr["FoodItem"].ToString(), (int)sr["Quantity"], sr["Food_Image"].ToString(), (int)sr["Price"], (int)sr["Id"], (int)sr["Restaurant_Id"]);
                cart.Add(cartItem);
            }
            if(cart.Count == 0)
            {
                ViewBag.Cart = "Empty";
            }


            return View("Cart",cart);
        }


        public IActionResult PlaceOrder()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from AddItemToCart where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
           
            var orderList = new List<Order>();
            while(sr.Read())
            {
                Order order = new Order(sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Price"], (int)sr["Quantity"], (int)sr["RestaurantId"]);
                orderList.Add(order);
            }
            conn.Close();

            foreach(var obj in orderList)
            {
                SqlCommand sqlCommand = new SqlCommand(String.Format("insert into ConfirmOrder values('{0}','{1}','{2}','{3}','{4}')",obj.UserName,obj.FoodItem,obj.Price,obj.Quantity,obj.RestaurantId), conn);
                conn.Open();
                sqlCommand.ExecuteNonQuery();
                conn.Close();
            }

            Log.Information(String.Format("Order placed with username {0} ",HttpContext.Session.GetString("UserName")));

            SqlCommand cmd1 = new SqlCommand(String.Format("delete from AddItemToCart where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            cmd1.ExecuteNonQuery();
            conn.Close();
            return View();
        }


       public IActionResult CancelOrder()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from ConfirmOrder where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Order> CancelOrderList = new List<Order>();
            while(sr.Read())
            {
                Order order = new Order(sr["UserName"].ToString(), sr["FoodItem"].ToString(),(int)sr["Price"], (int)sr["Quantity"], (int)sr["RestaurantId"]);
                CancelOrderList.Add(order);
            }
            conn.Close();

           foreach(var obj in CancelOrderList)
            {
                SqlCommand cmd1 = new SqlCommand(String.Format("insert into CancelOrder values('{0}','{1}','{2}','{3}','{4}')", obj.UserName,obj.FoodItem,obj.Price,obj.Quantity,obj.RestaurantId), conn);
                conn.Open();
                cmd1.ExecuteNonQuery();
                conn.Close();
            }

            SqlCommand cmd2 = new SqlCommand(String.Format("delete from ConfirmOrder where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            cmd2.ExecuteNonQuery();
            conn.Close();

            return View();



        }

        public IActionResult OrderStatus()
        {
            return View();
        }

        //[HttpPost]
        public IActionResult Status(int Id)
        {
            Console.WriteLine("Id : " + Id);
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }

            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");

            ViewBag.ObjectPassed = "True";
            var OrderList = new List<Order>();
            
            if(Id == 1)
            {
                SqlCommand cmd = new SqlCommand(String.Format("Select * from ConfirmOrder where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
                conn.Open();
                SqlDataReader sr = cmd.ExecuteReader();


                while (sr.Read())
                {
                    Order order = new Order(sr["FoodItem"].ToString(), (int)sr["Price"], (int)sr["Quantity"], "Pending");
                    OrderList.Add(order);
                }
                conn.Close();
                return View("OrderStatus",OrderList);
            }

            if(Id==2)
            {
                SqlCommand cmd2 = new SqlCommand(String.Format("Select * from CancelOrder where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
                conn.Open();
                SqlDataReader sr2 = cmd2.ExecuteReader();


                while (sr2.Read())
                {
                    Order order = new Order(sr2["FoodItem"].ToString(), (int)sr2["Price"], (int)sr2["Quantity"], "Cancelled");
                    OrderList.Add(order);
                }
                conn.Close();
                return View("OrderStatus", OrderList);

            }

            if(Id==3)
            {
                SqlCommand cmd1 = new SqlCommand(String.Format("Select * from CompletedOrders where CustomerName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
                conn.Open();
                SqlDataReader sr1 = cmd1.ExecuteReader();
                while (sr1.Read())
                {
                    Order order = new Order(sr1["FoodItem"].ToString(), (int)sr1["Price"], (int)sr1["Quantity"], "Delivered");
                    OrderList.Add(order);
                }
                conn.Close();
                return View("OrderStatus", OrderList);

            }

           


            return View("OrderStatus", OrderList);


        }


    }
}
