using FoodAppWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FoodAppWebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    //[CustomAttribute.TokenValidator]
    [ApiController]
    public class FoodController : ControllerBase
    {
        [HttpGet]
        [CustomAttribute.TokenValidator]
        public IActionResult GetAllRestaurants()
        {
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997; ");
            SqlCommand cmd = new SqlCommand("select * from Restaurants", conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Restaurants> res = new List<Restaurants>();
            while (sr.Read())
            {
                Restaurants restaurant = new Restaurants((int)sr["Restaurant_Id"], sr["Restaurant_Name"].ToString(), sr["Restaurant_Image"].ToString());
                res.Add(restaurant);
            }
            if (res.Count() == 0)
            {
                return NotFound();
            }

            return Ok(res);

        }


        [HttpGet("{Id}")]
        [CustomAttribute.TokenValidator]
        public IActionResult GetRestaurantMenuById(int Id)
        {

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select *  from Food where Restaurant_Id={0}", Id), conn);
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

            return Ok(model);


        }

        [HttpPost]
        [CustomAttribute.TokenValidator]
        public IActionResult AddToCart([FromBody] CartItems cart)
        {

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("insert into AddItemToCart values('{0}','{1}','{2}','{3}','{4}')",
                                                          cart.UserName,
                                                          cart.FoodItem,
                                                          cart.Quantity,
                                                          cart.RestaurantId,
                                                          cart.Price), conn);
            conn.Open();
            int res = cmd.ExecuteNonQuery();
            conn.Close();

            if (res == 0)
            {
                return BadRequest("No rows updated");
            }
            else
            {
                return Ok(res);
            }

        }


        [HttpDelete("{id}")]
        [CustomAttribute.TokenValidator]
        public IActionResult DeleteCartItemById(int id)
        {
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("delete from AddItemToCart where ItemNo = '{0}'", id), conn);
            conn.Open();
            int res = cmd.ExecuteNonQuery();
            conn.Close();
            if (res == 0)
            {
                return NotFound("Item Not Found");
            }
            else
            {
                return Ok(res);
            }


        }


        [HttpGet]
        [CustomAttribute.TokenValidator]
        public IActionResult GetCartByUserName(string UserName)
        {
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select A.FoodItem, A.Quantity,A.ItemNo, F.Food_Image,F.Price,F.Id,F.Restaurant_Id from AddItemToCart A inner join Food F on F.Food_Item = A.FoodItem where A.UserName = '{0}'", UserName), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            List<Cart> cart = new List<Cart>();

            while (sr.Read())
            {
                Cart cartItem = new Cart(UserName, sr["FoodItem"].ToString(), (int)sr["Quantity"], sr["Food_Image"].ToString(), (int)sr["Price"], (int)sr["Id"], (int)sr["Restaurant_Id"], (int)sr["ItemNo"]);
                cart.Add(cartItem);
            }

            if (cart.Count <= 0)
            {
                return NotFound("Cart is Empty");
            }
            else
            {
                return Ok(cart);
            }

        }



        [HttpDelete("{UserName}")]
        [CustomAttribute.TokenValidator]
        public IActionResult DeleteCartItemsByUserName(string UserName)
        {
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("delete from AddItemToCart where UserName = '{0}'", UserName), conn);
            conn.Open();
            int res = cmd.ExecuteNonQuery();
            conn.Close();
            if (res == 0)
            {
                return NotFound("Item Not Found");
            }
            else
            {
                return Ok();
            }
        }



        [HttpGet("{Id}/{UserName}")]
        [CustomAttribute.TokenValidator]
        public IActionResult OrderStatus(int Id, string UserName)
        {
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            var OrderList = new List<Order>();
            var orderlist = new List<OrderDetails>();

            if (Id == 1)
            {
                SqlCommand cmd = new SqlCommand(String.Format("Select * from PlacedOrderDetail where UserName = '{0}'", UserName), conn);
                conn.Open();
                SqlDataReader sr = cmd.ExecuteReader();

                while (sr.Read())
                {
                    string time = sr["OrderTime"].ToString();
                    DateTime orderTime = Convert.ToDateTime(time);
                    OrderDetails orderDetails = new OrderDetails((int)sr["InVoiceNo"], sr["UserName"].ToString(), (int)sr["RestaurantId"], sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], orderTime);
                    orderlist.Add(orderDetails);
                }
                conn.Close();
                return Ok(orderlist);
            }


            if (Id == 3)
            {
                SqlCommand cmd1 = new SqlCommand(String.Format("Select * from CompletedOrder where UserName = '{0}'", UserName), conn);
                conn.Open();
                SqlDataReader sr1 = cmd1.ExecuteReader();
                while (sr1.Read())
                {
                    OrderDetails order = new OrderDetails((int)sr1["InVoiceNo"], sr1["UserName"].ToString(), sr1["FoodItem"].ToString(), (int)sr1["Quantity"], (int)sr1["Price"], (DateTime)sr1["OrderCompletionTime"]);
                    orderlist.Add(order);
                }
                conn.Close();
                return Ok(orderlist);
            }
            return BadRequest();
        }



        [HttpPost]
        [CustomAttribute.TokenValidator]
        public IActionResult Orders([FromBody] OrdersApi order)
        {
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format(
                "insert into Orders values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')",
                order.InVoiceNo,
                order.UserName,
                order.Address,
                order.PhoneNo,
                order.OrderTime, order.City, order.State, order.Zipcode, order.CardNo, order.ExpMonth, order.ExpYear, order.CVV), conn);
            conn.Open();
            int res = cmd.ExecuteNonQuery();
            conn.Close();
            if (res == 0)
            {
                return NotFound("No rows updates");
            }
            else
            {
                return Ok(res);
            }
        }

        [HttpPost]
        [CustomAttribute.TokenValidator]
        public IActionResult OrderDetails([FromBody] List<OrderDetailsApi> order)
        {
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            int res = 0;
            foreach (var obj in order)
            {
                SqlCommand sqlCommand = new SqlCommand(String.Format(
                    "insert into PlacedOrderDetail values('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                    obj.InVoiceNo,
                    obj.UserName,
                    obj.RestaurantId,
                    obj.FoodItem,
                    obj.Quantity,
                    obj.Price,
                    obj.OrderTime), conn);

                conn.Open();
                res = res + sqlCommand.ExecuteNonQuery();
                conn.Close();
            }
            if (res == 0)
            {
                return NotFound("No rows updated");
            }
            else
            {
                return Ok(res);
            }

        }




        [HttpGet("{FoodItem}")]
        [CustomAttribute.TokenValidator]
        public IActionResult SearchMenuByName(string FoodItem)
        {
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("Select * from Food where Food_Item like '%{0}%' ", FoodItem), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            List<Menu> list = new List<Menu>();


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
                list.Add(menu);
            }

            return Ok(list);

        }


        [HttpPost]
        public IActionResult SignUp([FromBody] SignUp signup)
        {
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("insert into Users values('{0}','{1}','{2}')", signup.UserName, signup.Email, signup.Password), conn);
            conn.Open();
            int res = cmd.ExecuteNonQuery();
            conn.Close();

            if (res == 0)
            {
                return NotFound("No Rows Updated");
            }
            else
            {
                return Ok(res);
            }
        }


    }
}
