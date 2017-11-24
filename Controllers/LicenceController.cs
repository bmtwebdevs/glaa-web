using System;
using GLAA.Domain.Models;
using GLAA.Web.Attributes;
using System.Linq;
using System.Web.Mvc;
using GLAA.Services;
using GLAA.Services.LicenceApplication;
using GLAA.ViewModels;
using GLAA.ViewModels.LicenceApplication;
using GLAA.Web.FormLogic;
using GLAA.Web.Helpers;

namespace GLAA.Web.Controllers
{
    [SessionTimeout]
    public class LicenceController : DefaultController
    {
        private readonly ISessionHelper session;
        private readonly ILicenceApplicationViewModelBuilder licenceApplicationViewModelBuilder;
        private readonly ILicenceApplicationPostDataHandler licenceApplicationPostDataHandler;
        private readonly ILicenceStatusViewModelBuilder licenceStatusViewModelBuilder;
        private readonly IFormDefinition formDefinition;

        public LicenceController(ISessionHelper session,
            ILicenceApplicationViewModelBuilder licenceApplicationViewModelBuilder,
            ILicenceApplicationPostDataHandler licenceApplicationPostDataHandler,
            ILicenceStatusViewModelBuilder licenceStatusViewModelBuilder,
            IFormDefinition formDefinition) : base(formDefinition)
        {
            this.session = session;
            this.licenceApplicationViewModelBuilder = licenceApplicationViewModelBuilder;
            this.licenceApplicationPostDataHandler = licenceApplicationPostDataHandler;
            this.licenceStatusViewModelBuilder = licenceStatusViewModelBuilder;
            this.formDefinition = formDefinition;
        }

        [Route("Licence/TaskList")]
        public ActionResult TaskList()
        {
            session.SetCurrentUserIsAdmin(false);
            session.ClearCurrentPaStatus();
            session.ClearCurrentAbrId();
            session.ClearCurrentDopStatus();

            var licenceId = session.GetCurrentLicenceId();
            var model = licenceApplicationViewModelBuilder.Build(licenceId);

            //TODO: It's feasible we could access this page with no licenceId where the model will be null
            //TODO: how should we handle this
            model.Declaration?.Validate();
            model.Eligibility?.Validate();
            model.OrganisationDetails?.Validate();
            model.PrincipalAuthority?.Validate();
            model.AlternativeBusinessRepresentatives?.Validate();
            model.DirectorOrPartner?.Validate();
            model.NamedIndividuals?.Validate();
            model.Organisation?.Validate();

            return View(model);
        }

        private ActionResult CheckParentValidityAndRedirect(FormSection section, int submittedPageId)
        {
            var licenceId = session.GetCurrentLicenceId();
            var sectionLength = formDefinition.GetSectionLength(section);
            var nextPageId = submittedPageId + 1;

            if (submittedPageId + 1 != sectionLength)
            {
                IValidatable parent;

                switch (section)
                {
                    case FormSection.OrganisationDetails:
                        parent = licenceApplicationViewModelBuilder.Build<OrganisationDetailsViewModel>(licenceId) ??
                                 new OrganisationDetailsViewModel();
                        break;
                    case FormSection.PrincipalAuthority:
                        parent = licenceApplicationViewModelBuilder
                            .Build<PrincipalAuthorityViewModel, PrincipalAuthority>(
                                licenceId,
                                l => l.PrincipalAuthorities.SingleOrDefault(p => p.Id == session.GetCurrentPaId()));
                        break;
                    case FormSection.AlternativeBusinessRepresentative:
                        parent = licenceApplicationViewModelBuilder
                            .Build<AlternativeBusinessRepresentativeViewModel, AlternativeBusinessRepresentative>(
                                licenceId,
                                l => l.AlternativeBusinessRepresentatives.SingleOrDefault(a =>
                                    a.Id == session.GetCurrentAbrId()));
                        break;
                    case FormSection.DirectorOrPartner:
                        parent =
                            licenceApplicationViewModelBuilder.Build<DirectorOrPartnerViewModel, DirectorOrPartner>(
                                licenceId,
                                l => l.DirectorOrPartners.FirstOrDefault(d => d.Id == session.GetCurrentDopId()));
                        break;
                    case FormSection.NamedIndividual:
                        parent = licenceApplicationViewModelBuilder.Build<NamedIndividualViewModel, NamedIndividual>(
                            licenceId,
                            l => l.NamedIndividuals.FirstOrDefault(n => n.Id == session.GetCurrentNamedIndividualId()));
                        break;
                    case FormSection.Organisation:
                        parent = licenceApplicationViewModelBuilder.Build<OrganisationViewModel>(licenceId)
                            ?? new OrganisationViewModel();
                        break;
                    default:
                        // Somehow we've saved a model without creating a parent
                        return RedirectToAction("TaskList");
                }

                return ValidateParentAndRedirect(parent, section, nextPageId);
            }

            return RedirectToLastAction(section);
        }

        private ActionResult ValidateParentAndRedirect(IValidatable parent, FormSection section, int nextPageId)
        {
            parent.Validate();

            if (parent.IsValid)
            {
                return RedirectToLastAction(section);
            }

            return RedirectToAction(section, nextPageId);
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/{id}")]
        public ActionResult OrganisationDetails(int id)
        {
            session.SetLoadedPage(id);
            var licenceId = session.GetCurrentLicenceId();
            var model = licenceApplicationViewModelBuilder.Build<OrganisationDetailsViewModel>(licenceId);
            return GetNextView(id, FormSection.OrganisationDetails, model);
        }

        #region OrganisationDetails POSTs

        private ActionResult OrganisationDetailsPost<T>(T model, int submittedPageId)
        {
            session.SetSubmittedPage(FormSection.OrganisationDetails, submittedPageId);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.OrganisationDetails, submittedPageId), model);
            }

            var licenceId = session.GetCurrentLicenceId();

            if (model is AddressViewModel)
            {
                licenceApplicationPostDataHandler.UpdateAddress(session.GetCurrentLicenceId(), x => x, model as AddressViewModel);
            }
            else
            {
                licenceApplicationPostDataHandler.Update(licenceId, x => x, model);
            }

            return CheckParentValidityAndRedirect(FormSection.OrganisationDetails, submittedPageId);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/2")]
        public ActionResult OrganisationDetails(OrganisationNameViewModel model)
        {
            return OrganisationDetailsPost(model, 2);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/3")]
        public ActionResult OrganisationDetails(TradingNameViewModel model)
        {
            return OrganisationDetailsPost(model, 3);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/4")]
        public ActionResult OrganisationDetails(AddressViewModel model)
        {
            return OrganisationDetailsPost(model, 4);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/5")]
        public ActionResult OrganisationDetails(BusinessPhoneNumberViewModel model)
        {
            return OrganisationDetailsPost(model, 5);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/6")]
        public ActionResult OrganisationDetails(BusinessMobileNumberViewModel model)
        {
            return OrganisationDetailsPost(model, 6);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/7")]
        public ActionResult OrganisationDetails(BusinessEmailAddressViewModel model)
        {
            return OrganisationDetailsPost(model, 7);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/8")]
        public ActionResult OrganisationDetails(BusinessWebsiteViewModel model)
        {
            return OrganisationDetailsPost(model, 8);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/9")]
        public ActionResult OrganisationDetails(CommunicationPreferenceViewModel model)
        {
            return OrganisationDetailsPost(model, 9);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/10")]
        public ActionResult OrganisationDetails(LegalStatusViewModel model)
        {
            return OrganisationDetailsPost(model, 10);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/11")]
        public ActionResult OrganisationDetails(PAYEERNStatusViewModel model)
        {
            return OrganisationDetailsPost(model, 11);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/12")]
        public ActionResult OrganisationDetails(VATStatusViewModel model)
        {
            return OrganisationDetailsPost(model, 12);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/13")]
        public ActionResult OrganisationDetails(TaxReferenceViewModel model)
        {
            return OrganisationDetailsPost(model, 13);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/14")]
        public ActionResult OrganisationDetails(OperatingIndustriesViewModel model)
        {
            session.SetSubmittedPage(FormSection.OrganisationDetails, 14);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.OrganisationDetails, 14), model);
            }

            var licenceId = session.GetCurrentLicenceId();

            // TODO: This could be a mapping
            licenceApplicationPostDataHandler.UpdateShellfishStatus(licenceId, model);

            licenceApplicationPostDataHandler.Update(licenceId, x => x.OperatingIndustries,
                model.OperatingIndustries);

            return CheckParentValidityAndRedirect(FormSection.OrganisationDetails, 14);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/15")]
        public ActionResult OrganisationDetails(TurnoverViewModel model)
        {
            return OrganisationDetailsPost(model, 15);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/OrganisationDetails/Part/16")]
        public ActionResult OrganisationDetails(OperatingCountriesViewModel model)
        {
            session.SetSubmittedPage(FormSection.OrganisationDetails, 16);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.OrganisationDetails, 16), model);
            }

            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x.OperatingCountries,
                model.OperatingCountries);

            return CheckParentValidityAndRedirect(FormSection.OrganisationDetails, 16);
        }

        #endregion

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/{id}")]
        public ActionResult PrincipalAuthority(int id)
        {
            var licenceId = session.GetCurrentLicenceId();
            var model = licenceApplicationViewModelBuilder.Build<PrincipalAuthorityViewModel, PrincipalAuthority>(licenceId,
                x => x.PrincipalAuthorities.FirstOrDefault());

            if (model.Id.HasValue)
            {
                session.SetCurrentPaStatus(model.Id.Value, model.IsDirector.IsDirector ?? false);
            }

            if (model.DirectorOrPartnerId.HasValue)
            {
                session.SetCurrentDopStatus(model.DirectorOrPartnerId.Value, model.IsDirector.IsDirector ?? false);
            }

            session.SetLoadedPage(id);

            return GetNextView(id, FormSection.PrincipalAuthority, model);
        }

        #region PrincipalAuthority POSTS

        private ActionResult PrincipalAuthorityPost<T>(T model, int submittedPageId, bool doDopLinking = true)
        {
            return PrincipalAuthorityPost(model, submittedPageId, doDopLinking, m => !ModelState.IsValid);
        }

        private ActionResult PrincipalAuthorityPost<T>(T model, int submittedPageId, bool doDopLinking, Func<T, bool> modelIsInvalid)
        {
            session.SetSubmittedPage(FormSection.PrincipalAuthority, submittedPageId);

            if (modelIsInvalid(model))
            {
                return View(GetViewPath(FormSection.PrincipalAuthority, submittedPageId), model);
            }

            if (session.GetCurrentPaIsDirector() && doDopLinking)
            {
                licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), l => l.DirectorOrPartners, model,
                    session.GetCurrentDopId());
            }

            var paId = licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x.PrincipalAuthorities, model,
                session.GetCurrentPaId());

            session.SetCurrentPaStatus(paId, doDopLinking);

            return CheckParentValidityAndRedirect(FormSection.PrincipalAuthority, submittedPageId);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/2")]
        public ActionResult PrincipalAuthority(IsDirectorViewModel model)
        {
            session.SetSubmittedPage(FormSection.PrincipalAuthority, 2);

            if (!ModelState.IsValid || !model.IsDirector.HasValue)
            {
                return View(GetViewPath(FormSection.PrincipalAuthority, 2), model);
            }

            var paId = licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x.PrincipalAuthorities,
                model, session.GetCurrentPaId());

            session.SetCurrentPaStatus(paId, model.IsDirector.Value);

            if (model.IsDirector.Value)
            {
                var licenceModel = licenceApplicationViewModelBuilder.Build(session.GetCurrentLicenceId());

                var existingDirectorOrPartnerId = licenceModel.PrincipalAuthority.DirectorOrPartnerId ?? 0;

                var dopId = licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), l => l.DirectorOrPartners,
                    model,
                    existingDirectorOrPartnerId);

                session.SetCurrentDopStatus(dopId, true);

                licenceApplicationPostDataHandler.UpsertDirectorOrPartnerAndLinkToPrincipalAuthority(
                    session.GetCurrentLicenceId(), paId, dopId, model);

                return RedirectToAction(FormSection.PrincipalAuthority, 4);
            }

            licenceApplicationPostDataHandler.UnlinkDirectorOrPartnerFromPrincipalAuthority(session.GetCurrentPaId());

            return CheckParentValidityAndRedirect(FormSection.PrincipalAuthority, 2);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/3")]
        public ActionResult PrincipalAuthority(PrincipalAuthorityConfirmationViewModel model)
        {
            return PrincipalAuthorityPost(model, 3, false, m => !ModelState.IsValid || !m.WillProvideConfirmation);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/4")]
        public ActionResult PrincipalAuthority(FullNameViewModel model)
        {
            return PrincipalAuthorityPost(model, 4);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/5")]
        public ActionResult PrincipalAuthority(AlternativeFullNameViewModel model)
        {
            return PrincipalAuthorityPost(model, 5);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/6")]
        public ActionResult PrincipalAuthority(DateOfBirthViewModel model)
        {
            return PrincipalAuthorityPost(model, 6);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/7")]
        public ActionResult PrincipalAuthority(TownOfBirthViewModel model)
        {
            return PrincipalAuthorityPost(model, 7);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/8")]
        public ActionResult PrincipalAuthority(CountryOfBirthViewModel model)
        {
            return PrincipalAuthorityPost(model, 8);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/9")]
        public ActionResult PrincipalAuthority(JobTitleViewModel model)
        {
            return PrincipalAuthorityPost(model, 9);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/10")]
        public ActionResult PrincipalAuthority(AddressViewModel model)
        {
            session.SetSubmittedPage(FormSection.PrincipalAuthority, 10);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.PrincipalAuthority, 10), model);
            }

            if (session.GetCurrentPaIsDirector())
            {
                licenceApplicationPostDataHandler.UpdateAddress(session.GetCurrentLicenceId(),
                    l => l.DirectorOrPartners.Single(dop => dop.Id == session.GetCurrentDopId()), model);
            }

            licenceApplicationPostDataHandler.UpdateAddress(session.GetCurrentLicenceId(),
                x => x.PrincipalAuthorities.SingleOrDefault(pa => pa.Id == session.GetCurrentPaId()), model);

            return CheckParentValidityAndRedirect(FormSection.PrincipalAuthority, 10);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/11")]
        public ActionResult PrincipalAuthority(BusinessPhoneNumberViewModel model)
        {
            return PrincipalAuthorityPost(model, 11);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/12")]
        public ActionResult PrincipalAuthority(BusinessExtensionViewModel model)
        {
            return PrincipalAuthorityPost(model, 12);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/13")]
        public ActionResult PrincipalAuthority(PersonalMobileNumberViewModel model)
        {
            return PrincipalAuthorityPost(model, 13);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/14")]
        public ActionResult PrincipalAuthority(PersonalEmailAddressViewModel model)
        {
            return PrincipalAuthorityPost(model, 14);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/15")]
        public ActionResult PrincipalAuthority(NationalInsuranceNumberViewModel model)
        {
            return PrincipalAuthorityPost(model, 15);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/16")]
        public ActionResult PrincipalAuthority(NationalityViewModel model)
        {
            return PrincipalAuthorityPost(model, 16);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/17")]
        public ActionResult PrincipalAuthority(PassportViewModel model)
        {
            return PrincipalAuthorityPost(model, 17);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/18")]
        public ActionResult PrincipalAuthority(PrincipalAuthorityRightToWorkViewModel model)
        {
            return PrincipalAuthorityPost(model, 18);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/19")]
        public ActionResult PrincipalAuthority(UndischargedBankruptViewModel model)
        {
            return PrincipalAuthorityPost(model, 19);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/20")]
        public ActionResult PrincipalAuthority(DisqualifiedDirectorViewModel model)
        {
            return PrincipalAuthorityPost(model, 20);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/21")]
        public ActionResult PrincipalAuthority(RestraintOrdersViewModel model)
        {
            return PrincipalAuthorityPost(model, 21);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/22")]
        public ActionResult ReviewPrincipalAuthorityRestraintOrders(RestraintOrdersViewModel model)
        {
            session.SetSubmittedPage(FormSection.PrincipalAuthority, 22);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<PrincipalAuthorityViewModel, PrincipalAuthority>(licenceId,
                    l => l.PrincipalAuthorities.SingleOrDefault(p => p.Id == session.GetCurrentPaId()));
            model = parent.RestraintOrdersViewModel;

            if ((model.HasRestraintOrders ?? false) && !model.RestraintOrders.Any())
            {
                ModelState.AddModelError(nameof(model.RestraintOrders), "Please enter details of the restraint or confiscation orders or civil recoveries that you have been the subject of.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.PrincipalAuthority, 22), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.PrincipalAuthority, 23);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/23")]
        public ActionResult PrincipalAuthority(UnspentConvictionsViewModel model)
        {
            return PrincipalAuthorityPost(model, 23);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/24")]
        public ActionResult ReviewPrincipalAuthorityUnspentConvictions(UnspentConvictionsViewModel model)
        {
            session.SetSubmittedPage(FormSection.PrincipalAuthority, 24);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<PrincipalAuthorityViewModel, PrincipalAuthority>(licenceId,
                    l => l.PrincipalAuthorities.SingleOrDefault(p => p.Id == session.GetCurrentPaId()));
            model = parent.UnspentConvictionsViewModel;

            if ((model.HasUnspentConvictions ?? false) && !model.UnspentConvictions.Any())
            {
                ModelState.AddModelError(nameof(model.UnspentConvictions), "Please enter details of the unspent criminal convictions, or alternative sanctions or penalties for proven offences you have.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.PrincipalAuthority, 24), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.PrincipalAuthority, 25);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/25")]
        public ActionResult PrincipalAuthority(OffencesAwaitingTrialViewModel model)
        {
            return PrincipalAuthorityPost(model, 25);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/26")]
        public ActionResult ReviewPrincipalAuthorityOffencesAwaitingTrial(OffencesAwaitingTrialViewModel model)
        {
            session.SetSubmittedPage(FormSection.PrincipalAuthority, 26);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<PrincipalAuthorityViewModel, PrincipalAuthority>(licenceId,
                    l => l.PrincipalAuthorities.SingleOrDefault(p => p.Id == session.GetCurrentPaId()));
            model = parent.OffencesAwaitingTrialViewModel;

            if ((model.HasOffencesAwaitingTrial ?? false) && !model.OffencesAwaitingTrial.Any())
            {
                ModelState.AddModelError(nameof(model.OffencesAwaitingTrial), "Please enter details of the unspent criminal convictions, or alternative sanctions or penalties for proven offences you have.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.PrincipalAuthority, 26), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.PrincipalAuthority, 27);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/27")]
        public ActionResult PrincipalAuthority(PreviousLicenceViewModel model)
        {
            return PrincipalAuthorityPost(model, 27);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/28")]
        public ActionResult PrincipalAuthority(PreviousTradingNamesViewModel model)
        {
            return PrincipalAuthorityPost(model, 28, false);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/29")]
        public ActionResult ReviewPrincipalAuthorityPreviousTradingNames(PreviousTradingNamesViewModel model)
        {
            session.SetSubmittedPage(FormSection.PrincipalAuthority, 29);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<PrincipalAuthorityViewModel, PrincipalAuthority>(licenceId,
                    l => l.PrincipalAuthorities.Single(p => p.Id == session.GetCurrentPaId()));
            model = parent.PreviousTradingNames;

            if ((model.HasPreviousTradingNames ?? false) && !model.PreviousTradingNames.Any())
            {
                ModelState.AddModelError(nameof(model.PreviousTradingNames), "Please enter details of the unspent criminal convictions, or alternative sanctions or penalties for proven offences you have.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.PrincipalAuthority, 29), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.PrincipalAuthority, 30);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/PrincipalAuthority/Part/30")]
        public ActionResult PrincipalAuthority(PreviousExperienceViewModel model)
        {
            return PrincipalAuthorityPost(model, 30, false);
        }

        #endregion

        [HttpGet]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentatives/Part/{id}")]
        public ActionResult AlternativeBusinessRepresentatives(int id)
        {
            session.ClearCurrentAbrId();

            var licenceId = session.GetCurrentLicenceId();

            var model =
                licenceApplicationViewModelBuilder
                    .Build<AlternativeBusinessRepresentativeCollectionViewModel>(licenceId);

            return GetNextView(id, FormSection.AlternativeBusinessRepresentatives, model);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentatives/Part/2")]
        public ActionResult AlternativeBusinessRepresentatives(AlternativeBusinessRepresentativeCollectionViewModel model)
        {
            session.SetSubmittedPage(FormSection.AlternativeBusinessRepresentatives, 2);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.AlternativeBusinessRepresentatives, 2), model);
            }

            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x, model);

            return RedirectToAction(FormSection.AlternativeBusinessRepresentatives, 3);
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/{id}")]
        public ActionResult AlternativeBusinessRepresentative(int id)
        {
            var licenceId = session.GetCurrentLicenceId();
            var abrId = session.GetCurrentAbrId();

            var model =
                licenceApplicationViewModelBuilder
                    .Build<AlternativeBusinessRepresentativeViewModel, AlternativeBusinessRepresentative>(licenceId,
                        x => x.AlternativeBusinessRepresentatives.FirstOrDefault(a => a.Id == abrId)) ??
                new AlternativeBusinessRepresentativeViewModel();

            if (model.Id.HasValue)
            {
                session.SetCurrentAbrId(model.Id.Value);
            }

            session.SetLoadedPage(id);

            return GetNextView(id, FormSection.AlternativeBusinessRepresentative, model);
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Review/{id}")]
        public ActionResult ReviewAlternativeBusinessRepresentative(int id)
        {
            var licenceId = session.GetCurrentLicenceId();

            var abrs =
                licenceApplicationViewModelBuilder
                    .Build<AlternativeBusinessRepresentativeCollectionViewModel, AlternativeBusinessRepresentative>(licenceId,
                        x => x.AlternativeBusinessRepresentatives);

            // TODO: A better defence against URL hacking?
            if (abrs.AlternativeBusinessRepresentatives.None(a => a.Id == id))
            {
                return RedirectToAction($"Apply/AlternativeBusinessRepresentatives");
            }

            session.SetCurrentAbrId(id);

            var model = abrs.AlternativeBusinessRepresentatives.Single(a => a.Id == id);

            return View(GetLastViewPath(FormSection.AlternativeBusinessRepresentative), model);
        }

        #region Alternative Business Representative POSTS

        private ActionResult AlternativeBusinessRepresentativePost<T>(T model, int submittedPageId)
        {
            return AlternativeBusinessRepresentativePost(model, submittedPageId, m => !ModelState.IsValid);
        }

        private ActionResult AlternativeBusinessRepresentativePost<T>(T model, int submittedPageId,
            Func<T, bool> modelIsInvalid)
        {
            session.SetSubmittedPage(FormSection.AlternativeBusinessRepresentative, submittedPageId);

            if (modelIsInvalid(model))
            {
                return View(GetViewPath(FormSection.AlternativeBusinessRepresentative, submittedPageId), model);
            }

            var id = licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x.AlternativeBusinessRepresentatives,
                model, session.GetCurrentAbrId());
            session.SetCurrentAbrId(id);

            return CheckParentValidityAndRedirect(FormSection.AlternativeBusinessRepresentative, submittedPageId);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/1")]
        public ActionResult AlternativeBusinessRepresentative(FullNameViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 1);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/2")]
        public ActionResult AlternativeBusinessRepresentative(AlternativeFullNameViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 2);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/3")]
        public ActionResult AlternativeBusinessRepresentative(DateOfBirthViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 3);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/4")]
        public ActionResult AlternativeBusinessRepresentative(TownOfBirthViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 4);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/5")]
        public ActionResult AlternativeBusinessRepresentative(CountryOfBirthViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 5);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/6")]
        public ActionResult AlternativeBusinessRepresentative(JobTitleViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 6);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/7")]
        public ActionResult AlternativeBusinessRepresentative(AddressViewModel model)
        {
            session.SetSubmittedPage(FormSection.AlternativeBusinessRepresentative, 7);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.AlternativeBusinessRepresentative, 7), model);
            }

            licenceApplicationPostDataHandler.UpdateAddress(session.GetCurrentLicenceId(),
                x => x.AlternativeBusinessRepresentatives.Single(abr => abr.Id == session.GetCurrentAbrId()), model);

            return CheckParentValidityAndRedirect(FormSection.AlternativeBusinessRepresentative, 7);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/8")]
        public ActionResult AlternativeBusinessRepresentative(BusinessPhoneNumberViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 8);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/9")]
        public ActionResult AlternativeBusinessRepresentative(BusinessExtensionViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 9);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/10")]
        public ActionResult AlternativeBusinessRepresentative(PersonalMobileNumberViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 10);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/11")]
        public ActionResult AlternativeBusinessRepresentative(PersonalEmailAddressViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 11);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/12")]
        public ActionResult AlternativeBusinessRepresentative(NationalInsuranceNumberViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 12);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/13")]
        public ActionResult AlternativeBusinessRepresentative(NationalityViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 13);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/14")]
        public ActionResult AlternativeBusinessRepresentative(PassportViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 14);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/15")]
        public ActionResult AlternativeBusinessRepresentative(RightToWorkViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 15);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/16")]
        public ActionResult AlternativeBusinessRepresentative(UndischargedBankruptViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 16);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/17")]
        public ActionResult AlternativeBusinessRepresentative(DisqualifiedDirectorViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 17);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/18")]
        public ActionResult AlternativeBusinessRepresentative(RestraintOrdersViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 18);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/19")]
        public ActionResult ReviewAlternativeBusinessRepresentativeRestraintOrders(RestraintOrdersViewModel model)
        {
            session.SetSubmittedPage(FormSection.AlternativeBusinessRepresentative, 19);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<AlternativeBusinessRepresentativeViewModel, AlternativeBusinessRepresentative>(licenceId,
                    l => l.AlternativeBusinessRepresentatives.Single(p => p.Id == session.GetCurrentAbrId()));
            model = parent.RestraintOrdersViewModel;

            if ((model.HasRestraintOrders ?? false) && !model.RestraintOrders.Any())
            {
                ModelState.AddModelError(nameof(model.RestraintOrders), "Please enter details of the restraint or confiscation orders or civil recoveries that you have been the subject of.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.AlternativeBusinessRepresentative, 19), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.AlternativeBusinessRepresentative, 20);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/20")]
        public ActionResult AlternativeBusinessRepresentative(UnspentConvictionsViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 20);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/21")]
        public ActionResult ReviewAlternativeBusinessRepresentativeUnspentConvictions(UnspentConvictionsViewModel model)
        {
            session.SetSubmittedPage(FormSection.AlternativeBusinessRepresentative, 21);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<AlternativeBusinessRepresentativeViewModel, AlternativeBusinessRepresentative>(licenceId,
                    l => l.AlternativeBusinessRepresentatives.Single(p => p.Id == session.GetCurrentAbrId()));
            model = parent.UnspentConvictionsViewModel;

            if ((model.HasUnspentConvictions ?? false) && !model.UnspentConvictions.Any())
            {
                ModelState.AddModelError(nameof(model.UnspentConvictions), "Please enter details of the unspent criminal convictions, or alternative sanctions or penalties for proven offences you have.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.AlternativeBusinessRepresentative, 21), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.AlternativeBusinessRepresentative, 22);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/22")]
        public ActionResult AlternativeBusinessRepresentative(OffencesAwaitingTrialViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 22);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/23")]
        public ActionResult ReviewAlternativeBusinessRepresentativeOffencesAwaitingTrial(OffencesAwaitingTrialViewModel model)
        {
            session.SetSubmittedPage(FormSection.AlternativeBusinessRepresentative, 23);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<AlternativeBusinessRepresentativeViewModel, AlternativeBusinessRepresentative>(licenceId,
                    l => l.AlternativeBusinessRepresentatives.Single(p => p.Id == session.GetCurrentAbrId()));
            model = parent.OffencesAwaitingTrialViewModel;

            if ((model.HasOffencesAwaitingTrial ?? false) && !model.OffencesAwaitingTrial.Any())
            {
                ModelState.AddModelError(nameof(model.OffencesAwaitingTrial), "Please enter details of the unspent criminal convictions, or alternative sanctions or penalties for proven offences you have.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.AlternativeBusinessRepresentative, 23), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.AlternativeBusinessRepresentative, 24);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Part/24")]
        public ActionResult AlternativeBusinessRepresentative(PreviousLicenceViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 24);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/AlternativeBusinessRepresentative/Review/{id}")]
        public ActionResult DeleteAlternativeBusinessRepresentative(AlternativeBusinessRepresentativeViewModel model)
        {
            var id = session.GetCurrentAbrId();

            licenceApplicationPostDataHandler.Delete<AlternativeBusinessRepresentative>(id);

            return RedirectToLastAction(FormSection.AlternativeBusinessRepresentatives);
        }
        
        #endregion

        [HttpGet]
        [ExportModelState]
        [Route("Licence/Apply/DirectorsOrPartners/Part/{id}")]
        public ActionResult DirectorsOrPartners(int id)
        {
            session.ClearCurrentDopStatus();
            session.ClearCurrentPaStatus();

            var licenceId = session.GetCurrentLicenceId();

            var model = licenceApplicationViewModelBuilder.Build<DirectorOrPartnerCollectionViewModel>(licenceId);

            return GetNextView(id, FormSection.DirectorsOrPartners, model);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorsOrPartners/Part/2")]
        public ActionResult DirectorsOrPartners(DirectorOrPartnerCollectionViewModel model)
        {
            session.SetSubmittedPage(FormSection.DirectorsOrPartners, 2);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.DirectorsOrPartners, 2), model);
            }

            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x, model);

            return RedirectToAction(FormSection.DirectorsOrPartners, 3);
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Review/{id}")]
        public ActionResult ReviewDirectorOrPartner(int id)
        {
            var licenceId = session.GetCurrentLicenceId();

            var dops = licenceApplicationViewModelBuilder.Build<DirectorOrPartnerCollectionViewModel>(licenceId);

            // TODO: A better defence against URL hacking?
            if (dops.DirectorsOrPartners.None(a => a.Id == id))
            {
                return RedirectToAction(FormSection.DirectorsOrPartners, 2);
            }

            var model = dops.DirectorsOrPartners.Single(a => a.Id == id);

            session.SetCurrentDopStatus(id, model.IsPreviousPrincipalAuthority.IsPreviousPrincipalAuthority ?? false);

            if ((model.IsPreviousPrincipalAuthority.IsPreviousPrincipalAuthority ?? false) && model.PrincipalAuthorityId.HasValue)
            {
                session.SetCurrentPaStatus(model.PrincipalAuthorityId.Value, true);
            }

            return View(GetLastViewPath(FormSection.DirectorOrPartner), model);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Review/{id}")]
        public ActionResult DeleteDirectorOrPartner(DirectorOrPartnerViewModel model)
        {
            licenceApplicationPostDataHandler.Delete<DirectorOrPartner>(session.GetCurrentDopId());

            if (session.GetCurrentDopIsPa())
            {
                licenceApplicationPostDataHandler.Delete<PrincipalAuthority>(session.GetCurrentPaId());
            }

            return RedirectToLastAction(FormSection.DirectorsOrPartners);
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/{id}")]
        public ActionResult DirectorOrPartner(int id)
        {
            var licenceId = session.GetCurrentLicenceId();
            var dopId = session.GetCurrentDopId();

            var model = licenceApplicationViewModelBuilder.Build<DirectorOrPartnerViewModel, DirectorOrPartner>(
                licenceId, l => l.DirectorOrPartners.SingleOrDefault(d => d.Id == dopId));

            if (model.Id.HasValue)
            {
                session.SetCurrentDopStatus(model.Id.Value, model.IsPreviousPrincipalAuthority.IsPreviousPrincipalAuthority ?? false);
            }

            if (model.PrincipalAuthorityId.HasValue)
            {
                session.SetCurrentPaStatus(model.PrincipalAuthorityId.Value, model.IsPreviousPrincipalAuthority.IsPreviousPrincipalAuthority ?? false);
            }

            session.SetLoadedPage(id);

            return GetNextView(id, FormSection.DirectorOrPartner, model);
        }

        #region Director Or Partner POSTS

        private ActionResult DirectorOrPartnerPost<T>(T model, int submittedPageId, bool doPaLinking = true)
        {
            return DirectorOrPartnerPost(model, submittedPageId, doPaLinking, m => !ModelState.IsValid);
        }

        private ActionResult DirectorOrPartnerPost<T>(T model, int submittedPageId, bool doPaLinking, Func<T, bool> modelIsInvalid)
        {
            session.SetSubmittedPage(FormSection.DirectorOrPartner, submittedPageId);

            if (modelIsInvalid(model))
            {
                return View(GetViewPath(FormSection.DirectorOrPartner, submittedPageId), model);
            }

            if (session.GetCurrentDopIsPa() && doPaLinking)
            {
                licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), l => l.PrincipalAuthorities, model,
                    session.GetCurrentPaId());
            }

            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), l => l.DirectorOrPartners, model,
                session.GetCurrentDopId());

            return CheckParentValidityAndRedirect(FormSection.DirectorOrPartner, submittedPageId);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/1")]
        public ActionResult DirectorOrPartner(IsPreviousPrincipalAuthorityViewModel model)
        {
            session.SetSubmittedPage(FormSection.DirectorOrPartner, 1);

            if (!ModelState.IsValid || !model.IsPreviousPrincipalAuthority.HasValue)
            {
                return View(GetViewPath(FormSection.DirectorOrPartner, 1), model);
            }

            var dopId = licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), l => l.DirectorOrPartners, model,
                session.GetCurrentDopId());

            session.SetCurrentDopStatus(dopId, model.IsPreviousPrincipalAuthority.Value);

            if (model.IsPreviousPrincipalAuthority.Value)
            {
                var paId = licenceApplicationPostDataHandler.UpsertPrincipalAuthorityAndLinkToDirectorOrPartner(
                    session.GetCurrentLicenceId(), dopId, session.GetCurrentPaId(), model);
                session.SetCurrentPaStatus(paId, true);
            }
            else
            {
                licenceApplicationPostDataHandler.UnlinkPrincipalAuthorityFromDirectorOrPartner(session.GetCurrentDopId());
            }

            return CheckParentValidityAndRedirect(FormSection.DirectorOrPartner, 1);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/2")]
        public ActionResult DirectorOrPartner(FullNameViewModel model)
        {
            return DirectorOrPartnerPost(model, 2);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/3")]
        public ActionResult DirectorOrPartner(AlternativeFullNameViewModel model)
        {
            return DirectorOrPartnerPost(model, 3);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/4")]
        public ActionResult DirectorOrPartner(DateOfBirthViewModel model)
        {
            return DirectorOrPartnerPost(model, 4);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/5")]
        public ActionResult DirectorOrPartner(TownOfBirthViewModel model)
        {
            return DirectorOrPartnerPost(model, 5);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/6")]
        public ActionResult DirectorOrPartner(CountryOfBirthViewModel model)
        {
            return DirectorOrPartnerPost(model, 6);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/7")]
        public ActionResult DirectorOrPartner(JobTitleViewModel model)
        {
            return DirectorOrPartnerPost(model, 7);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/8")]
        public ActionResult DirectorOrPartner(AddressViewModel model)
        {
            session.SetSubmittedPage(FormSection.DirectorOrPartner, 8);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.DirectorOrPartner, 8), model);
            }

            if (session.GetCurrentDopIsPa())
            {
                licenceApplicationPostDataHandler.UpdateAddress(session.GetCurrentLicenceId(), l => l.PrincipalAuthorities.Single(pa => pa.Id == session.GetCurrentPaId()), model);
            }

            licenceApplicationPostDataHandler.UpdateAddress(session.GetCurrentLicenceId(), l => l.DirectorOrPartners.Single(dop => dop.Id == session.GetCurrentDopId()), model);

            return CheckParentValidityAndRedirect(FormSection.DirectorOrPartner, 8);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/9")]
        public ActionResult DirectorOrPartner(BusinessPhoneNumberViewModel model)
        {
            return DirectorOrPartnerPost(model, 9);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/10")]
        public ActionResult DirectorOrPartner(BusinessExtensionViewModel model)
        {
            return DirectorOrPartnerPost(model, 10);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/11")]
        public ActionResult DirectorOrPartner(PersonalMobileNumberViewModel model)
        {
            return DirectorOrPartnerPost(model, 11);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/12")]
        public ActionResult DirectorOrPartner(PersonalEmailAddressViewModel model)
        {
            return DirectorOrPartnerPost(model, 12);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/13")]
        public ActionResult DirectorOrPartner(NationalInsuranceNumberViewModel model)
        {
            return DirectorOrPartnerPost(model, 13);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/14")]
        public ActionResult DirectorOrPartner(NationalityViewModel model)
        {
            return DirectorOrPartnerPost(model, 14);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/15")]
        public ActionResult DirectorOrPartner(PassportViewModel model)
        {
            return DirectorOrPartnerPost(model, 15);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/16")]
        public ActionResult DirectorOrPartner(RightToWorkViewModel model)
        {
            return DirectorOrPartnerPost(model, 16);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/17")]
        public ActionResult DirectorOrPartner(UndischargedBankruptViewModel model)
        {
            return DirectorOrPartnerPost(model, 17);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/18")]
        public ActionResult DirectorOrPartner(DisqualifiedDirectorViewModel model)
        {
            return DirectorOrPartnerPost(model, 18);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/19")]
        public ActionResult DirectorOrPartner(RestraintOrdersViewModel model)
        {
            return DirectorOrPartnerPost(model, 19);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/20")]
        public ActionResult ReviewDirectorOrPartnerRestraintOrders(RestraintOrdersViewModel model)
        {
            session.SetSubmittedPage(FormSection.DirectorOrPartner, 20);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<DirectorOrPartnerViewModel, DirectorOrPartner>(licenceId,
                    l => l.DirectorOrPartners.Single(p => p.Id == session.GetCurrentDopId()));
            model = parent.RestraintOrdersViewModel;

            if ((model.HasRestraintOrders ?? false) && !model.RestraintOrders.Any())
            {
                ModelState.AddModelError(nameof(model.RestraintOrders), "Please enter details of the restraint or confiscation orders or civil recoveries that you have been the subject of.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.DirectorOrPartner, 20), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.DirectorOrPartner, 21);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/21")]
        public ActionResult DirectorOrPartner(UnspentConvictionsViewModel model)
        {
            return DirectorOrPartnerPost(model, 21);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/22")]
        public ActionResult ReviewDirectorOrPartnerUnspentConvictions(UnspentConvictionsViewModel model)
        {
            session.SetSubmittedPage(FormSection.DirectorOrPartner, 22);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<DirectorOrPartnerViewModel, DirectorOrPartner>(licenceId,
                    l => l.DirectorOrPartners.Single(p => p.Id == session.GetCurrentDopId()));
            model = parent.UnspentConvictionsViewModel;

            if ((model.HasUnspentConvictions ?? false) && !model.UnspentConvictions.Any())
            {
                ModelState.AddModelError(nameof(model.UnspentConvictions), "Please enter details of the unspent criminal convictions, or alternative sanctions or penalties for proven offences you have.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.DirectorOrPartner, 22), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.DirectorOrPartner, 23);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/23")]
        public ActionResult DirectorOrPartner(OffencesAwaitingTrialViewModel model)
        {
            return DirectorOrPartnerPost(model, 23);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/24")]
        public ActionResult ReviewDirectorOrPartnerOffencesAwaitingTrial(OffencesAwaitingTrialViewModel model)
        {
            session.SetSubmittedPage(FormSection.DirectorOrPartner, 24);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<DirectorOrPartnerViewModel, DirectorOrPartner>(licenceId,
                    l => l.DirectorOrPartners.Single(p => p.Id == session.GetCurrentDopId()));
            model = parent.OffencesAwaitingTrialViewModel;

            if ((model.HasOffencesAwaitingTrial ?? false) && !model.OffencesAwaitingTrial.Any())
            {
                ModelState.AddModelError(nameof(model.OffencesAwaitingTrial), "Please enter details of the unspent criminal convictions, or alternative sanctions or penalties for proven offences you have.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.DirectorOrPartner, 24), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.DirectorOrPartner, 24);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/DirectorOrPartner/Part/25")]
        public ActionResult DirectorOrPartner(PreviousLicenceViewModel model)
        {
            return AlternativeBusinessRepresentativePost(model, 25);
        }

        #endregion

        [HttpGet]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividuals/Part/{id}")]
        public ActionResult NamedIndividuals(int id)
        {
            var licenceId = session.GetCurrentLicenceId();

            session.ClearCurrentNamedIndividualId();

            var model = licenceApplicationViewModelBuilder.Build<NamedIndividualCollectionViewModel>(licenceId);

            return GetNextView(id, FormSection.NamedIndividuals, model);
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/{id}")]
        public ActionResult NamedIndividual(int id)
        {
            var licenceId = session.GetCurrentLicenceId();
            var namedIndividualId = session.GetCurrentNamedIndividualId();

            var model = licenceApplicationViewModelBuilder
                .Build<NamedIndividualViewModel, NamedIndividual>(licenceId,
                    x => x.NamedIndividuals.FirstOrDefault(y => y.Id == namedIndividualId));

            if (model.Id.HasValue)
            {
                session.SetCurrentNamedIndividualId(model.Id.Value);
            }

            session.SetLoadedPage(id);

            return GetNextView(id, FormSection.NamedIndividual, model);
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/JobTitle/Part/{id}")]
        public ActionResult JobTitle(int id)
        {
            var licenceId = session.GetCurrentLicenceId();

            var namedIndividualId = session.GetCurrentNamedIndividualId();

            var model = licenceApplicationViewModelBuilder
                .Build<NamedJobTitleViewModel, NamedJobTitle>(licenceId,
                    x => x.NamedJobTitles.FirstOrDefault(y => y.Id == namedIndividualId));

            return GetNextView(id, FormSection.JobTitle, model);
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/NamedIndividual/Review/{id}")]
        public ActionResult ReviewNamedIndividual(int id)
        {
            var licenceId = session.GetCurrentLicenceId();

            var models =
                licenceApplicationViewModelBuilder
                    .Build<NamedIndividualCollectionViewModel, NamedIndividual>(licenceId,
                        x => x.NamedIndividuals);

            // TODO: A better defence against URL hacking?
            if (models.NamedIndividuals.All(ni => ni.Id != id))
            {
                return RedirectToAction($"Apply/NamedIndividuals");
            }

            session.SetCurrentNamedIndividualId(id);

            var model = models.NamedIndividuals.Single(a => a.Id == id);

            return View(GetLastViewPath(FormSection.NamedIndividual), model);
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/JobTitle/Review/{id}")]
        public ActionResult ReviewJobTitle(int id)
        {
            var licenceId = session.GetCurrentLicenceId();

            var models =
                licenceApplicationViewModelBuilder
                    .Build<NamedIndividualCollectionViewModel, NamedJobTitle>(licenceId,
                        x => x.NamedJobTitles);

            // TODO: A better defence against URL hacking?
            if (models.NamedJobTitles.All(ni => ni.Id != id))
            {
                return RedirectToAction($"Apply/NamedIndividuals");
            }

            session.SetCurrentNamedIndividualId(id);

            var model = models.NamedJobTitles.Single(a => a.Id == id);

            return View(GetLastViewPath(FormSection.JobTitle), model);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Review/{id}")]
        public ActionResult DeleteNamedIndividual(NamedIndividualViewModel model)
        {
            var id = session.GetCurrentNamedIndividualId();

            licenceApplicationPostDataHandler.Delete<NamedIndividual>(id);

            return RedirectToLastAction(FormSection.NamedIndividuals);
        }

        #region Named Individual POSTS

        private ActionResult NamedIndividualPost<T>(T model, int submittedPageId)
        {
            return NamedIndividualPost(model, submittedPageId, m => !ModelState.IsValid);
        }

        private ActionResult NamedIndividualPost<T>(T model, int submittedPageId, Func<T, bool> modelIsInvalid)
        {
            session.SetSubmittedPage(FormSection.NamedIndividual, submittedPageId);

            if (modelIsInvalid(model))
            {
                return View(GetViewPath(FormSection.NamedIndividual, submittedPageId), model);
            }

            var id = licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x.NamedIndividuals, model, session.GetCurrentNamedIndividualId());
            session.SetCurrentNamedIndividualId(id);

            return CheckParentValidityAndRedirect(FormSection.NamedIndividual, submittedPageId);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividuals/Part/2")]
        public ActionResult NamedIndividuals(NamedIndividualCollectionViewModel model)
        {
            session.SetSubmittedPage(FormSection.NamedIndividuals, 2);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.NamedIndividuals, 2), model);
            }

            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x, model);

            return RedirectToAction(FormSection.NamedIndividuals, 3);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/JobTitle/Part/1")]
        public ActionResult JobTitle(NamedJobTitleViewModel model)
        {
            session.SetSubmittedPage(FormSection.JobTitle, 1);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.JobTitle, 1), model);
            }

            var id = licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x.NamedJobTitles, model, session.GetCurrentNamedIndividualId());
            session.SetCurrentNamedIndividualId(id);

            return RedirectToAction(FormSection.JobTitle, 2);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/JobTitle/Review/{id}")]
        public ActionResult DeleteNamedJobTitle(NamedJobTitleViewModel model)
        {
            var id = session.GetCurrentNamedIndividualId();

            licenceApplicationPostDataHandler.Delete<NamedJobTitle>(id);

            return RedirectToLastAction(FormSection.NamedIndividuals);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/1")]
        public ActionResult NamedIndividual(FullNameViewModel model)
        {
            return NamedIndividualPost(model, 1);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/2")]
        public ActionResult NamedIndividual(DateOfBirthViewModel model)
        {
            return NamedIndividualPost(model, 2);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/3")]
        public ActionResult NamedIndividual(BusinessPhoneNumberViewModel model)
        {
            return NamedIndividualPost(model, 3);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/4")]
        public ActionResult NamedIndividual(BusinessExtensionViewModel model)
        {
            return NamedIndividualPost(model, 4);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/5")]
        public ActionResult NamedIndividual(RightToWorkViewModel model)
        {
            return NamedIndividualPost(model, 5);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/6")]
        public ActionResult NamedIndividual(UndischargedBankruptViewModel model)
        {
            return NamedIndividualPost(model, 6);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/7")]
        public ActionResult NamedIndividual(DisqualifiedDirectorViewModel model)
        {
            return NamedIndividualPost(model, 7);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/8")]
        public ActionResult NamedIndividual(RestraintOrdersViewModel model)
        {
            return NamedIndividualPost(model, 8);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/9")]
        public ActionResult ReviewNamedIndividualRestraintOrders(RestraintOrdersViewModel model)
        {
            session.SetSubmittedPage(FormSection.NamedIndividual, 9);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<NamedIndividualViewModel, NamedIndividual>(licenceId,
                    l => l.NamedIndividuals.Single(p => p.Id == session.GetCurrentNamedIndividualId()));
            model = parent.RestraintOrdersViewModel;

            if ((model.HasRestraintOrders ?? false) && !model.RestraintOrders.Any())
            {
                ModelState.AddModelError(nameof(model.RestraintOrders), "Please enter details of the restraint or confiscation orders or civil recoveries that you have been the subject of.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.NamedIndividual, 9), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.NamedIndividual, 10);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/10")]
        public ActionResult NamedIndividual(UnspentConvictionsViewModel model)
        {
            return NamedIndividualPost(model, 10);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/11")]
        public ActionResult ReviewNamedIndividualUnspentConvictions(UnspentConvictionsViewModel model)
        {
            session.SetSubmittedPage(FormSection.NamedIndividual, 11);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<NamedIndividualViewModel, NamedIndividual>(licenceId,
                    l => l.NamedIndividuals.Single(p => p.Id == session.GetCurrentNamedIndividualId()));
            model = parent.UnspentConvictionsViewModel;

            if ((model.HasUnspentConvictions ?? false) && !model.UnspentConvictions.Any())
            {
                ModelState.AddModelError(nameof(model.UnspentConvictions), "Please enter details of the unspent criminal convictions, or alternative sanctions or penalties for proven offences you have.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.NamedIndividual, 11), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.NamedIndividual, 12);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/12")]
        public ActionResult NamedIndividual(OffencesAwaitingTrialViewModel model)
        {
            return NamedIndividualPost(model, 12);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/13")]
        public ActionResult ReviewNamedIndividualOffencesAwaitingTrial(OffencesAwaitingTrialViewModel model)
        {
            session.SetSubmittedPage(FormSection.NamedIndividual, 13);

            var licenceId = session.GetCurrentLicenceId();
            var parent =
                licenceApplicationViewModelBuilder.Build<NamedIndividualViewModel, NamedIndividual>(licenceId,
                    l => l.NamedIndividuals.Single(p => p.Id == session.GetCurrentNamedIndividualId()));
            model = parent.OffencesAwaitingTrialViewModel;

            if ((model.HasOffencesAwaitingTrial ?? false) && !model.OffencesAwaitingTrial.Any())
            {
                ModelState.AddModelError(nameof(model.OffencesAwaitingTrial), "Please enter details of the unspent criminal convictions, or alternative sanctions or penalties for proven offences you have.");
                ViewData.Add("doOverride", true);
                return View(GetViewPath(FormSection.NamedIndividual, 13), model);
            }

            return ValidateParentAndRedirect(parent, FormSection.NamedIndividual, 14);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/NamedIndividual/Part/14")]
        public ActionResult NamedIndividual(PreviousLicenceViewModel model)
        {
            return NamedIndividualPost(model, 14);
        }

        #endregion

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/Organisation/Part/{id}")]
        public ActionResult Organisation(int id)
        {
            var licenceId = session.GetCurrentLicenceId();
            var model = licenceApplicationViewModelBuilder.Build<OrganisationViewModel>(licenceId);
            return GetNextView(id, FormSection.Organisation, model);
        }

        #region Organisation POSTS

        private ActionResult OrganisationPost<T>(T model, int submittedPageId)
        {
            session.SetSubmittedPage(FormSection.Organisation, submittedPageId);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.Organisation, submittedPageId), model);
            }

            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x, model);

            return CheckParentValidityAndRedirect(FormSection.Organisation, submittedPageId);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/2")]
        public ActionResult Organisation(OutsideSectorsViewModel model)
        {
            session.SetSubmittedPage(FormSection.Organisation, 2);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.Organisation, 2), model);
            }

            // update the non collection part
            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x, model);
            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x.SelectedSectors, model.SelectedSectors);

            return CheckParentValidityAndRedirect(FormSection.Organisation, 2);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/3")]
        public ActionResult Organisation(WrittenAgreementViewModel model)
        {
            return OrganisationPost(model, 3);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/4")]
        public ActionResult Organisation(PSCControlledViewModel model)
        {
            return OrganisationPost(model, 4);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/5")]
        public ActionResult Organisation(MultipleBranchViewModel model)
        {
            session.SetSubmittedPage(FormSection.Organisation, 5);

            if (!ModelState.IsValid)
            {
                return View(GetViewPath(FormSection.Organisation, 5), model);
            }

            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x, model);
            licenceApplicationPostDataHandler.Update(session.GetCurrentLicenceId(), x => x.SelectedMultiples, model.SelectedMultiples);

            return CheckParentValidityAndRedirect(FormSection.Organisation, 5);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/6")]
        public ActionResult Organisation(TransportingWorkersViewModel model)
        {
            return OrganisationPost(model, 6);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/7")]
        public ActionResult Organisation(AccommodatingWorkersViewModel model)
        {
            return OrganisationPost(model, 7);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/8")]
        public ActionResult Organisation(SourcingWorkersViewModel model)
        {
            return OrganisationPost(model, 8);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/9")]
        public ActionResult Organisation(WorkerSupplyMethodViewModel model)
        {
            return OrganisationPost(model, 9);
        }


        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/10")]
        public ActionResult Organisation(WorkerContractViewModel model)
        {
            return OrganisationPost(model, 10);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/11")]
        public ActionResult Organisation(BannedFromTradingViewModel model)
        {
            return OrganisationPost(model, 11);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/12")]
        public ActionResult Organisation(SubcontractorViewModel model)
        {
            return OrganisationPost(model, 12);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/13")]
        public ActionResult Organisation(ShellfishWorkerNumberViewModel model)
        {
            return OrganisationPost(model, 13);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/14")]
        public ActionResult Organisation(ShellfishWorkerNationalityViewModel model)
        {
            return OrganisationPost(model, 14);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/Organisation/Part/15")]
        public ActionResult Organisation(PreviouslyWorkedInShellfishViewModel model)
        {
            return OrganisationPost(model, 15);
        }

        #endregion

        #region Generic security questions

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/{section}/RestraintOrder/{roId}")]
        public ActionResult AddRestraintOrder(string section, int roId)
        {
            Func<Licence, RestraintOrder> selector;

            switch (section)
            {
                case "PrincipalAuthority":
                    selector = l =>
                        l.PrincipalAuthorities.Single(pa => pa.Id == session.GetCurrentPaId()).RestraintOrders
                            .SingleOrDefault(ro => ro.Id == roId);
                    break;
                case "AlternativeBusinessRepresentative":
                    selector = l =>
                        l.AlternativeBusinessRepresentatives.Single(abr => abr.Id == session.GetCurrentAbrId()).RestraintOrders
                            .SingleOrDefault(ro => ro.Id == roId);
                    break;
                case "DirectorOrPartner":
                    selector = l =>
                        l.DirectorOrPartners.Single(dop => dop.Id == session.GetCurrentDopId()).RestraintOrders
                            .SingleOrDefault(ro => ro.Id == roId);
                    break;
                case "NamedIndividual":
                    selector = l =>
                        l.NamedIndividuals.Single(ni => ni.Id == session.GetCurrentNamedIndividualId()).RestraintOrders
                            .SingleOrDefault(ro => ro.Id == roId);
                    break;
                default:
                    selector = l => null;
                    break;
            }

            var model = licenceApplicationViewModelBuilder.Build<RestraintOrderViewModel, RestraintOrder>(
                session.GetCurrentLicenceId(), selector) ?? new RestraintOrderViewModel();

            return View("SecurityQuestions/RestraintOrder", model);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/{section}/RestraintOrder/{roId}")]
        public ActionResult AddRestraintOrder(RestraintOrderViewModel model, string section, int roId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddRestraintOrder", new { section, roId });
            }

            if (section == "PrincipalAuthority")
            {
                var id = licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<RestraintOrderViewModel, RestraintOrder, PrincipalAuthority>(
                        session.GetCurrentPaId(), roId, model, pa => pa.RestraintOrders, ro => ro.PrincipalAuthority);

                if (session.GetCurrentPaIsDirector())
                {
                    licenceApplicationPostDataHandler
                        .UpsertSecurityAnswerAndLinkToParent<RestraintOrderViewModel, RestraintOrder, DirectorOrPartner>(
                            session.GetCurrentDopId(), id, model, dop => dop.RestraintOrders,
                            ro => ro.DirectorOrPartner);
                }
            }
            else if (section == "AlternativeBusinessRepresentative")
            {
                licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<RestraintOrderViewModel, RestraintOrder,
                        AlternativeBusinessRepresentative>(
                        session.GetCurrentAbrId(), roId, model, abr => abr.RestraintOrders,
                        ro => ro.AlternativeBusinessRepresentative);
            }
            else if (section == "DirectorOrPartner")
            {
                var id = licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<RestraintOrderViewModel, RestraintOrder, DirectorOrPartner>(
                        session.GetCurrentDopId(), roId, model, dop => dop.RestraintOrders, ro => ro.DirectorOrPartner);

                if (session.GetCurrentDopIsPa())
                {
                    licenceApplicationPostDataHandler
                        .UpsertSecurityAnswerAndLinkToParent<RestraintOrderViewModel, RestraintOrder, PrincipalAuthority>(
                            session.GetCurrentPaId(), id, model, pa => pa.RestraintOrders, ro => ro.PrincipalAuthority);
                }
            }
            else if (section == "NamedIndividual")
            {
                licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<RestraintOrderViewModel, RestraintOrder,
                        NamedIndividual>(
                        session.GetCurrentNamedIndividualId(), roId, model, ni => ni.RestraintOrders,
                        ro => ro.NamedIndividual);
            }

            var lastLoaded = session.GetLoadedPage();

            return RedirectToAction($"Apply/{section}/Part/{lastLoaded}");
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/{section}/RestraintOrder/Remove/{roId}/")]
        public ActionResult RemoveRestraintOrder(RestraintOrderViewModel model, string section, int roId)
        {
            licenceApplicationPostDataHandler.Delete<RestraintOrder>(roId);

            var lastLoaded = session.GetLoadedPage();

            return RedirectToAction($"Apply/{section}/Part/{lastLoaded}");
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/{section}/UnspentConviction/{cId}")]
        public ActionResult AddConviction(string section, int cId)
        {
            Func<Licence, Conviction> selector;

            switch (section)
            {
                case "PrincipalAuthority":
                    selector = l =>
                        l.PrincipalAuthorities.Single(pa => pa.Id == session.GetCurrentPaId()).UnspentConvictions
                            .SingleOrDefault(uc => uc.Id == cId);
                    break;
                case "AlternativeBusinessRepresentative":
                    selector = l =>
                        l.AlternativeBusinessRepresentatives.Single(abr => abr.Id == session.GetCurrentAbrId()).UnspentConvictions
                            .SingleOrDefault(uc => uc.Id == cId);
                    break;
                case "DirectorOrPartner":
                    selector = l =>
                        l.DirectorOrPartners.Single(dop => dop.Id == session.GetCurrentDopId()).UnspentConvictions
                            .SingleOrDefault(uc => uc.Id == cId);
                    break;
                case "NamedIndividual":
                    selector = l =>
                        l.NamedIndividuals.Single(ni => ni.Id == session.GetCurrentNamedIndividualId()).UnspentConvictions
                            .SingleOrDefault(uc => uc.Id == cId);
                    break;
                default:
                    selector = l => null;
                    break;
            }

            var model =
                licenceApplicationViewModelBuilder.Build<UnspentConvictionViewModel, Conviction>(
                    session.GetCurrentLicenceId(), selector) ?? new UnspentConvictionViewModel();

            return View("SecurityQuestions/UnspentConviction", model);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/{section}/UnspentConviction/{cId}")]
        public ActionResult AddConviction(UnspentConvictionViewModel model, string section, int cId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddConviction", new { section, cId });
            }

            if (section == "PrincipalAuthority")
            {
                var id = licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<UnspentConvictionViewModel, Conviction, PrincipalAuthority>(
                        session.GetCurrentPaId(), cId, model, pa => pa.UnspentConvictions, uc => uc.PrincipalAuthority);

                if (session.GetCurrentPaIsDirector())
                {
                    licenceApplicationPostDataHandler
                        .UpsertSecurityAnswerAndLinkToParent<UnspentConvictionViewModel, Conviction, DirectorOrPartner>(
                            session.GetCurrentDopId(), id, model, dop => dop.UnspentConvictions,
                            uc => uc.DirectorOrPartner);
                }
            }
            else if (section == "AlternativeBusinessRepresentative")
            {
                licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<UnspentConvictionViewModel, Conviction,
                        AlternativeBusinessRepresentative>(
                        session.GetCurrentAbrId(), cId, model, abr => abr.UnspentConvictions,
                        uc => uc.AlternativeBusinessRepresentative);
            }
            else if (section == "DirectorOrPartner")
            {
                var id = licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<UnspentConvictionViewModel, Conviction, DirectorOrPartner>(
                        session.GetCurrentDopId(), cId, model, dop => dop.UnspentConvictions, uc => uc.DirectorOrPartner);

                if (session.GetCurrentDopIsPa())
                {
                    licenceApplicationPostDataHandler
                        .UpsertSecurityAnswerAndLinkToParent<UnspentConvictionViewModel, Conviction, PrincipalAuthority>(
                            session.GetCurrentPaId(), id, model, pa => pa.UnspentConvictions,
                            uc => uc.PrincipalAuthority);
                }
            }
            else if (section == "NamedIndividual")
            {
                licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<UnspentConvictionViewModel, Conviction,
                        NamedIndividual>(
                        session.GetCurrentNamedIndividualId(), cId, model, ni => ni.UnspentConvictions,
                        uc => uc.NamedIndividual);
            }

            var lastLoaded = session.GetLoadedPage();

            return RedirectToAction($"Apply/{section}/Part/{lastLoaded}");
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/{section}/UnspentConviction/Remove/{cId}/")]
        public ActionResult RemoveConviction(UnspentConvictionViewModel model, string section, int cId)
        {
            licenceApplicationPostDataHandler.Delete<Conviction>(cId);

            var lastLoaded = session.GetLoadedPage();

            return RedirectToAction($"Apply/{section}/Part/{lastLoaded}");
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/{section}/OffenceAwaitingTrial/{oId}")]
        public ActionResult AddOffenceAwaitingTrial(string section, int oId)
        {
            Func<Licence, OffenceAwaitingTrial> selector;

            switch (section)
            {
                case "PrincipalAuthority":
                    selector = l =>
                        l.PrincipalAuthorities.Single(pa => pa.Id == session.GetCurrentPaId()).OffencesAwaitingTrial
                            .SingleOrDefault(o => o.Id == oId);
                    break;
                case "AlternativeBusinessRepresentative":
                    selector = l =>
                        l.AlternativeBusinessRepresentatives.Single(abr => abr.Id == session.GetCurrentAbrId()).OffencesAwaitingTrial
                            .SingleOrDefault(o => o.Id == oId);
                    break;
                case "DirectorOrPartner":
                    selector = l =>
                        l.DirectorOrPartners.Single(dop => dop.Id == session.GetCurrentDopId()).OffencesAwaitingTrial
                            .SingleOrDefault(o => o.Id == oId);
                    break;
                case "NamedIndividual":
                    selector = l =>
                        l.NamedIndividuals.Single(ni => ni.Id == session.GetCurrentNamedIndividualId()).OffencesAwaitingTrial
                            .SingleOrDefault(o => o.Id == oId);
                    break;
                default:
                    selector = l => null;
                    break;
            }

            var model = licenceApplicationViewModelBuilder.Build<OffenceAwaitingTrialViewModel, OffenceAwaitingTrial>(
                            session.GetCurrentLicenceId(), selector) ?? new OffenceAwaitingTrialViewModel();

            return View("SecurityQuestions/OffenceAwaitingTrial", model);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/{section}/OffenceAwaitingTrial/{oId}")]
        public ActionResult AddOffenceAwaitingTrial(OffenceAwaitingTrialViewModel model, string section, int oId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddOffenceAwaitingTrial", new { section, oId });
            }

            if (section == "PrincipalAuthority")
            {
                var id = licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<OffenceAwaitingTrialViewModel, OffenceAwaitingTrial,
                        PrincipalAuthority>(
                        session.GetCurrentPaId(), oId, model, pa => pa.OffencesAwaitingTrial,
                        o => o.PrincipalAuthority);

                if (session.GetCurrentPaIsDirector())
                {
                    licenceApplicationPostDataHandler
                        .UpsertSecurityAnswerAndLinkToParent<OffenceAwaitingTrialViewModel, OffenceAwaitingTrial,
                            DirectorOrPartner>(
                            session.GetCurrentDopId(), id, model, dop => dop.OffencesAwaitingTrial,
                            o => o.DirectorOrPartner);
                }
            }
            else if (section == "AlternativeBusinessRepresentative")
            {
                licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<OffenceAwaitingTrialViewModel, OffenceAwaitingTrial,
                        AlternativeBusinessRepresentative>(
                        session.GetCurrentAbrId(), oId, model, abr => abr.OffencesAwaitingTrial,
                        o => o.AlternativeBusinessRepresentative);
            }
            else if (section == "DirectorOrPartner")
            {
                var id = licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<OffenceAwaitingTrialViewModel, OffenceAwaitingTrial,
                        DirectorOrPartner>(
                        session.GetCurrentDopId(), oId, model, dop => dop.OffencesAwaitingTrial,
                        o => o.DirectorOrPartner);

                if (session.GetCurrentDopIsPa())
                {
                    licenceApplicationPostDataHandler
                        .UpsertSecurityAnswerAndLinkToParent<OffenceAwaitingTrialViewModel, OffenceAwaitingTrial,
                            PrincipalAuthority>(
                            session.GetCurrentPaId(), id, model, pa => pa.OffencesAwaitingTrial,
                            o => o.PrincipalAuthority);
                }
            }
            else if (section == "NamedIndividual")
            {
                licenceApplicationPostDataHandler
                    .UpsertSecurityAnswerAndLinkToParent<OffenceAwaitingTrialViewModel, OffenceAwaitingTrial,
                        NamedIndividual>(
                        session.GetCurrentNamedIndividualId(), oId, model, ni => ni.OffencesAwaitingTrial,
                        o => o.NamedIndividual);
            }

            var lastLoaded = session.GetLoadedPage();

            return RedirectToAction($"Apply/{section}/Part/{lastLoaded}");
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/{section}/OffenceAwaitingTrial/Remove/{cId}/")]
        public ActionResult RemoveOffenceAwaitingTrial(OffenceAwaitingTrialViewModel model, string section, int cId)
        {
            licenceApplicationPostDataHandler.Delete<OffenceAwaitingTrial>(cId);

            var lastLoaded = session.GetLoadedPage();

            return RedirectToAction($"Apply/{section}/Part/{lastLoaded}");
        }

        [HttpGet]
        [ImportModelState]
        [Route("Licence/Apply/{section}/PreviousTradingName/{pId}")]
        public ActionResult AddPreviousTradingName(string section, int pId)
        {
            var model = licenceApplicationViewModelBuilder.Build<PreviousTradingNameViewModel, PreviousTradingName>(
                            session.GetCurrentLicenceId(),
                            l => l.PrincipalAuthorities.Single(p => p.Id == session.GetCurrentPaId()).PreviousTradingNames
                                .SingleOrDefault(p => p.Id == pId)) ?? new PreviousTradingNameViewModel();

            return View("SecurityQuestions/PreviousTradingName", model);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/{section}/PreviousTradingName/{pId}")]
        public ActionResult AddPreviousTradingName(PreviousTradingNameViewModel model, string section, int pId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddPreviousTradingName", new { section, pId });
            }

            licenceApplicationPostDataHandler
                .UpsertSecurityAnswerAndLinkToParent<PreviousTradingNameViewModel, PreviousTradingName,
                    PrincipalAuthority>(
                    session.GetCurrentPaId(), pId, model, pa => pa.PreviousTradingNames, p => p.PrincipalAuthority);

            var lastLoaded = session.GetLoadedPage();

            return RedirectToAction($"Apply/{section}/Part/{lastLoaded}");
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/Apply/{section}/PreviousTradingName/Remove/{pId}/")]
        public ActionResult RemovePreviousTradingName(PreviousTradingNameViewModel model, string section, int pId)
        {
            licenceApplicationPostDataHandler.Delete<PreviousTradingName>(pId);

            var lastLoaded = session.GetLoadedPage();

            return RedirectToAction($"Apply/{section}/Part/{lastLoaded}");
        }

        #endregion

        [Route("Licence/Apply/Fees/Part/{id}")]
        public ActionResult Fees(int id)
        {
            return View($"Fees.{id}");
        }

        [Route("Licence/Apply/Declaration/Part/{id}")]
        public ActionResult Declaration(int id)
        {
            return View($"Declaration.{id}");
        }

        [Route("Licence/Apply/Summary/Part/{id}")]
        public ActionResult Summary(int id)
        {
            return View($"Summary.{id}");
        }

        [Route("Licence/Declaration")]
        [HttpGet]
        public ActionResult Declaration()
        {
            var licenceApplicationModel = new LicenceApplicationViewModel();
            return View(licenceApplicationModel);
        }

        [HttpPost]
        public ActionResult SaveDeclaration()
        {
            return View("TaskList");
        }

        [Route("Licence/SubmitApplication")]
        [HttpGet]
        [ImportModelState]
        public ActionResult SubmitApplication()
        {
            var licenceId = session.GetCurrentLicenceId();
            var model = licenceApplicationViewModelBuilder.Build(licenceId);

            model.Validate();

            return View(model);
        }

        [HttpPost]
        [ExportModelState]
        [Route("Licence/SubmitApplication")]
        public ActionResult SubmitApplication(LicenceApplicationViewModel model)
        {
            var licenceId = session.GetCurrentLicenceId();

            if (!model.AgreedToTermsAndConditions)
            {
                ModelState.AddModelError("AgreedToTermsAndConditions", "You must agree to the terms and conditions in order to submit your application.");

                var dbModel = licenceApplicationViewModelBuilder.Build(licenceId);
                model.OrganisationDetails = dbModel.OrganisationDetails;
                model.PrincipalAuthority = dbModel.PrincipalAuthority;
                model.AlternativeBusinessRepresentatives = dbModel.AlternativeBusinessRepresentatives;
                model.DirectorOrPartner = dbModel.DirectorOrPartner;
                model.NamedIndividuals = dbModel.NamedIndividuals;
                model.Organisation = dbModel.Organisation;
                return View("SubmitApplication", model);
            }

            model.NewLicenceStatus = LicenceStatusEnum.SubmittedOnline;
            licenceApplicationPostDataHandler.Update(licenceId, model);

            return RedirectToAction("DecisionWait");
        }

        [Route("Licence/DecisionWait")]
        [HttpGet]
        public ActionResult DecisionWait()
        {
            var licenceId = session.GetCurrentLicenceId();

            var model = licenceStatusViewModelBuilder.BuildLatestStatus(licenceId);
            return View(model);
        }

        [Route("Licence/PayFee")]
        [HttpGet]
        public ActionResult PayFee()
        {
            var licenceApplicationModel = new LicenceApplicationViewModel();
            return View(licenceApplicationModel);
        }

        [HttpGet]
        [Route("Licence/Resume")]
        public ActionResult Resume()
        {
            var model = new ResumeApplicationViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Licence/Resume")]
        public ActionResult Resume(ResumeApplicationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var licence = licenceApplicationViewModelBuilder.Build(model.ApplicationId);

                if (licence != null)
                {
                    session.SetCurrentLicenceId(licence.Id);
                    session.Set("ApplicationId", model.ApplicationId);

                    return RedirectToAction("TaskList");
                }

                ModelState.AddModelError("ApplicationNotFound",
                    $"We were unable to find your application with the ID: {model.ApplicationId}.");
                ViewData.Add("doOverride", true);
            }

            return View(model);
        }
    }
}