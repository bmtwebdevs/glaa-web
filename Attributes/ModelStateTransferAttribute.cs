using System.Web.Mvc;

namespace GLAA.Web.Attributes
{
    // https://www.exceptionnotfound.net/the-post-redirect-get-pattern-in-asp-net-mvc/
    public abstract class ModelStateTransferAttribute : ActionFilterAttribute
    {
        protected static readonly string Key = typeof(ModelStateTransferAttribute).FullName;
    }

    public class ExportModelStateAttribute : ModelStateTransferAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!filterContext.Controller.ViewData.ModelState.IsValid)
            {
                if (filterContext.Result is RedirectResult || filterContext.Result is RedirectToRouteResult)
                {
                    filterContext.Controller.TempData[Key] = filterContext.Controller.ViewData.ModelState;
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }

    public class ImportModelStateAttribute : ModelStateTransferAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var modelState = filterContext.Controller.TempData[Key] as ModelStateDictionary;

            if (modelState != null)
            {
                if (filterContext.Result is ViewResult)
                {
                    filterContext.Controller.ViewData.ModelState.Merge(modelState);
                }
                else
                {
                    filterContext.Controller.TempData.Remove(Key);
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}