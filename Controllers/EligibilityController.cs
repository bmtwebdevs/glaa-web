using System.Web.Mvc;
using GLAA.Domain.Models;
using GLAA.Services.LicenceApplication;
using GLAA.ViewModels.LicenceApplication;
using GLAA.Web.Attributes;
using GLAA.Web.FormLogic;
using GLAA.Web.Helpers;

namespace GLAA.Web.Controllers
{
    public class EligibilityController : DefaultController
    {
        private readonly ISessionHelper session;
        private readonly ILicenceApplicationViewModelBuilder licenceApplicationViewModelBuilder;
        private readonly ILicenceApplicationPostDataHandler licenceApplicationPostDataHandler;

        public EligibilityController(ISessionHelper session, ILicenceApplicationViewModelBuilder licenceApplicationViewModelBuilder, IFormDefinition formDefinition, ILicenceApplicationPostDataHandler licenceApplicationPostDataHandler)
            : base(formDefinition)
        {
            this.session = session;
            this.licenceApplicationViewModelBuilder = licenceApplicationViewModelBuilder;            
            this.licenceApplicationPostDataHandler = licenceApplicationPostDataHandler;
        }

        protected override string GetViewPath(FormSection section, int id)
        {
            return $"{section.ToString()}.{id}";
        }

        [HttpGet]
        [ImportModelState]
        [Route("Eligibility/Part/{id}")]
        public ActionResult Eligibility(int id)
        {
            var licenceId = session.GetCurrentLicenceId();

            var model = licenceApplicationViewModelBuilder.Build<EligibilityViewModel>(licenceId);

            return GetNextView(id, FormSection.Eligibility, model);            
        }

        [HttpGet]
        public ActionResult WhatDoINeed()
        {
            return View();
        }

        [HttpGet]
        public ActionResult WhatIsCovered()
        {
            return View();
        }

        [HttpGet]
        public ActionResult OverseasBusiness()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Introduction()
        {
            var licenceApplicationModel = licenceApplicationViewModelBuilder.New();

            return View("Introduction", licenceApplicationModel);
        }

        [HttpPost]
        public ActionResult Introduction(LicenceApplicationViewModel model)
        {
            model.NewLicenceStatus = LicenceStatusEnum.NewApplication;

            var licenceId = licenceApplicationPostDataHandler.Insert(model);

            session.SetCurrentLicenceId(licenceId);

            return RedirectToAction($"Part/1");
        }

        [HttpPost]
        [Route("Eligibility/Part/1")]
        public ActionResult Part1(SuppliesWorkersViewModel model)
        {
            session.Set("LastSubmittedPageSection", "Part1");
            session.Set("LastSubmittedPageId", 1);

            if (!ModelState.IsValid)
            {
                return View("Eligibility.1", model);
            }

            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x, model);

            return RedirectToAction($"Part/2");            
        }

        [HttpPost]
        [Route("Eligibility/Part/2")]
        public ActionResult Part2(OperatingIndustriesViewModel model)
        {
            session.Set("LastSubmittedPageSection", "Part2");
            session.Set("LastSubmittedPageId", 2);

            if (!ModelState.IsValid)
            {
                return View("Eligibility.2", model);
            }

            var licenceId = session.GetCurrentLicenceId();
            
            licenceApplicationPostDataHandler.UpdateShellfishStatus(licenceId, model);

            licenceApplicationPostDataHandler.Update(licenceId, x => x.OperatingIndustries,
                model.OperatingIndustries);

            return RedirectToAction($"Part/3");
        }

        [HttpPost]
        [Route("Eligibility/Part/3")]
        public ActionResult Part3(TurnoverViewModel model)
        {
            session.Set("LastSubmittedPageSection", "Part3");
            session.Set("LastSubmittedPageId", 3);

            if (!ModelState.IsValid)
            {
                return View("Eligibility.3", model);
            }

            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x, model);

            return RedirectToAction($"Part/4");
        }

        [HttpPost]
        [Route("Eligibility/Part/4")]
        public ActionResult Part4()
        {
            session.Set("LastSubmittedPageSection", "Part4");
            session.Set("LastSubmittedPageId", 4);


            //if (!ModelState.IsValid)
            //{
            //    return View("Eligibility.3", model);
            //}

            //licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x, model);

            return RedirectToAction("TaskList", "Licence");
        }
    }
}