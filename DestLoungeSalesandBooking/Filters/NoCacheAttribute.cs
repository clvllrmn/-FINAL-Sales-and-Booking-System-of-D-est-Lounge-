// Filters/NoCacheAttribute.cs
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Filters
{
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(System.DateTime.UtcNow.AddDays(-1));
            response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);
            response.Cookies.Remove(System.Web.Helpers.AntiForgeryConfig.CookieName);
            base.OnActionExecuting(filterContext);
        }
    }
}