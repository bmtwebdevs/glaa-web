using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace GLAA.Web.Attributes
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = HttpContext.Current;
            if (context.Session != null)
            {
                if (context.Session.IsNewSession)
                {
                    var sessionCookie = context.Request.Headers["Cookie"];
                    if (sessionCookie != null && sessionCookie.IndexOf("ASP.NET_SessionId", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        if (context.Request.IsAuthenticated)
                        {
                            FormsAuthentication.SignOut();
                        }

                        var redirectTo = "~/Home/SessionTimeout";
                        filterContext.Result = new RedirectResult(redirectTo);
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}