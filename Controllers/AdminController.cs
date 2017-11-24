using System;
using System.Web.Mvc;
using GLAA.Services.Admin;
using GLAA.ViewModels;
using GLAA.ViewModels.Admin;
using GLAA.Web.Helpers;

namespace GLAA.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ISessionHelper session;
        private readonly IAdminHomeViewModelBuilder homeBuilder;
        private readonly IAdminLicenceListViewModelBuilder listBuilder;
        private readonly IAdminLicenceViewModelBuilder licenceBuilder;
        private readonly IAdminLicencePostDataHandler postDataHandler;

        public AdminController(ISessionHelper session, IAdminHomeViewModelBuilder homeBuilder,
            IAdminLicenceListViewModelBuilder listBuilder,
            IAdminLicenceViewModelBuilder licenceBuilder, IAdminLicencePostDataHandler postDataHandler)
        {
            this.session = session;
            this.homeBuilder = homeBuilder;
            this.listBuilder = listBuilder;
            this.licenceBuilder = licenceBuilder;
            this.postDataHandler = postDataHandler;
        }

        public ActionResult Index()
        {
            var model = homeBuilder.New();
            return View(model);
        }

        public ActionResult ApplicationList()
        {
            var type = Convert.ToBoolean(Request.QueryString["isApplication"]) ? LicenceOrApplication.Application : LicenceOrApplication.Licence;
            var model = listBuilder.Build(type);
            return View("ApplicationList", model);
        }

        [Route("Admin/Licence/{id}")]
        public ActionResult Licence(int id)
        {
            session.SetCurrentUserIsAdmin(true);
            session.SetCurrentLicenceId(id);
            var model = licenceBuilder.Build(id);
            return View("Application", model);
        }

        [HttpPost]
        [Route("Admin/Licence/{id}")]
        public ActionResult Licence(AdminLicenceViewModel model)
        {
            postDataHandler.UpdateStatus(model);

            model = licenceBuilder.Build(model.Licence.Id);

            return View("Application", model);
        }

        public ActionResult ApplicationPerson()
        {
            return View();
        }
    }
}