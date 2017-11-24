using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GLAA.ViewModels;
using GLAA.Web.FormLogic;

namespace GLAA.Web.Controllers
{
    public class DefaultController : Controller
    {
        private readonly IFormDefinition formDefinition;

        public DefaultController(IFormDefinition formDefinition)
        {
            this.formDefinition = formDefinition;
        }

        protected virtual string GetViewPath(FormSection section, int id)
        {
            return $"{section.ToString()}/{section.ToString()}.{id}";
        }

        protected virtual string GetLastViewPath(FormSection section)
        {
            return GetViewPath(section, formDefinition.GetSectionLength(section));
        }

        protected ActionResult GetNextView<T>(int id, FormSection section, T model) where T : IValidatable
        {
            if (!formDefinition.CanViewNextModel(section, id, model))
            {
                return RedirectToNextPossibleView(id, section, model);
            }

            var viewPath = GetViewPath(section, id);
            var viewModel = formDefinition.GetViewModel(section, id, model);

            return View(viewPath, viewModel);
        }

        protected ActionResult RedirectToNextPossibleView<T>(int id, FormSection section, T model) where T : IValidatable
        {
            while (!formDefinition.CanViewNextModel(section, id, model))
            {
                id++;
            }

            return RedirectToAction(section, id);
        }

        protected string GetActionPath(FormSection section, int id)
        {
            return $"Apply/{section.ToString()}/Part/{id}";
        }

        protected string GetLastActionPath(FormSection section)
        {
            return GetActionPath(section, formDefinition.GetSectionLength(section));
        }

        protected ActionResult RedirectToAction(FormSection section, int id)
        {
            return RedirectToAction(GetActionPath(section, id));
        }

        protected ActionResult RedirectToLastAction(FormSection section)
        {
            return RedirectToAction(GetActionPath(section, formDefinition.GetSectionLength(section)));
        }
    }
}