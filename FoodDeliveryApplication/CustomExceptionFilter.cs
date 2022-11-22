using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoodDeliveryApplication
{
    public class CustomExceptionFilter : System.Web.Mvc.FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filtercontext)
        {
            if (filtercontext.Exception is NotImplementedException)
            {

            }
            else
            {
                filtercontext.Result = new ViewResult()
                {
                    ViewName = "Error"
                };
                filtercontext.ExceptionHandled = true;
            }
        }
    }
}
