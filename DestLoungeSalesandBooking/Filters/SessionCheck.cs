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
            var request = filterContext.HttpContext.Request;

            // ── Set no-cache headers ──
            response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(System.DateTime.UtcNow.AddDays(-1));
            response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);

            // ── Regenerate anti-forgery cookie so it always matches the form token ──
            // This prevents the token mismatch error when back button is used
            AntiForgeryConfig.SuppressXFrameOptionsHeader = false;
            response.Cookies.Remove(AntiForgeryConfig.CookieName);

            // ── Check session ──
            if (session["UserID"] == null)
            {
                filterContext.Result = new RedirectResult("/Main/LoginPage");
                return;
            }

            if (RequireAdmin && (int)session["RoleID"] != 1)
            {
                filterContext.Result = new RedirectResult("/Main/Homepage");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}