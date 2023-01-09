using FoodDeliveryApplication.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;

namespace FoodDeliveryApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }



        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;

        //    //Insert UserDetails in List

        //}

        public IActionResult Index()
        {
            if (String.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString("UserName"))){
                //return RedirectToAction("Login", "FoodSite");
                return View();
            }
            else
            {
                return RedirectToAction("Restaurants", "FoodSite");

            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //-------------------------------------------------
       


        //-------------------------------------------------

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}