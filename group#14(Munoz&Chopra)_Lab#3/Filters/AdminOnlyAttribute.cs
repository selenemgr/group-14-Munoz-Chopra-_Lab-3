using group_14_Munoz_Chopra__Lab_3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace group_14_Munoz_Chopra__Lab_3.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var http = context.HttpContext;
            var username = http.Session.GetString("Username");

            // If not logged in, redirect to login
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Fetch user info from DB
            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var user = db.Users.FirstOrDefault(u => u.Username == username);

            // If not admin → forbid access
            if (user == null || !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
