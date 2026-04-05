using System.Web.Helpers;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Filters
{
    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public bool RequireAdmin { get; set; } = false;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var response = filterContext.HttpContext.Response;

            // ── Set no-cache headers ──
            response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(System.DateTime.UtcNow.AddDays(-1));
            response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);

            // ── Regenerate anti-forgery cookie so it always matches the form token ──
            AntiForgeryConfig.SuppressXFrameOptionsHeader = false;
            response.Cookies.Remove(AntiForgeryConfig.CookieName);

            var routeValues = filterContext.RouteData.Values;
            string controller = routeValues["controller"]?.ToString();
            string action = routeValues["action"]?.ToString();


            // allow public read of services
            if (controller == "Service" && action == "GetAllServices")
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (controller == "HomePageContent" && action == "GetAllContent")
            {
                base.OnActionExecuting(filterContext);
                return;
            }
            if (controller == "FAQ" && action == "GetAllFAQs")
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // ── Check session ──
            if (session["UserID"] == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { success = false, message = "Session expired" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }

                filterContext.Result = new RedirectResult("/Main/LoginPage");
                return;
            }

            // ── Check admin role if needed ──
            if (RequireAdmin && (int)session["RoleID"] != 1)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { success = false, message = "Access denied" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }

                filterContext.Result = new RedirectResult("/Main/Homepage");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}