using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GLAA.ViewModels.LicenceApplication;
using GLAA.Web.Models;

namespace GLAA.Web.Controllers
{
    public class WireframeLicenceController : Controller
    {
        // GET: WireframeLicence
        public ActionResult Index()
        {
            return View();
        }

        [Route("WireframeLicence/Apply")]
        public ActionResult Apply()
        {
            var licenceApplicationModel = new LicenceApplicationViewModel();
            return View(licenceApplicationModel);
        }

        [Route("WireframeLicence/Apply/OrganisationDetails/Part/{id}")]

        public ActionResult OrganisationDetails(int id, LicenceApplicationViewModel model)
        {
            return View($"OrganisationDetails.{id}", model);
        }

        [Route("WireframeLicence/Apply/PrincipalAuthority/Part/{id}")]
        public ActionResult PrincipalAuthority(int id, LicenceApplicationViewModel model)
        {
            return View($"PrincipalAuthority.{id}", model);
        }

        [Route("WireframeLicence/Apply/AlternativeBusinessRepresentative/Part/{id}")]
        public ActionResult AlternativeBusinessRepresentative(int id, LicenceApplicationViewModel model)
        {
            return View($"AlternativeBusinessRepresentative.{id}", model);
        }

        [Route("WireframeLicence/Apply/DirectorsOrPartners/Part/{id}")]
        public ActionResult DirectorsOrPartners(int id, LicenceApplicationViewModel model)
        {
            return View($"DirectorsOrPartners.{id}", model);
        }

        [Route("WireframeLicence/Apply/NamedIndividuals/Part/{id}")]
        public ActionResult NamedIndividuals(int id, LicenceApplicationViewModel model)
        {
            return View($"NamedIndividuals.{id}", model);
        }

        [Route("WireframeLicence/Apply/SecurityQuestions/Part/{id}")]
        public ActionResult SecurityQuestions(int id, LicenceApplicationViewModel model)
        {
            return View($"SecurityQuestions.{id}", model);
        }

        [Route("WireframeLicence/Apply/Organisation/Part/{id}")]
        public ActionResult Organisation(int id)
        {
            return View($"Organisation.{id}");
        }

        [Route("WireframeLicence/Apply/Fees/Part/{id}")]
        public ActionResult Fees(int id)
        {
            return View($"Fees.{id}");
        }

        [Route("WireframeLicence/Apply/Declaration/Part/{id}")]
        public ActionResult Declaration(int id)
        {
            return View($"Declaration.{id}");
        }

        [Route("WireframeLicence/Apply/Summary/Part/{id}")]
        public ActionResult Summary(int id)
        {
            return View($"Summary.{id}");
        }
    }
}