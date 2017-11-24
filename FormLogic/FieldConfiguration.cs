using System.Collections.Generic;
using System.Linq;
using GLAA.ViewModels.LicenceApplication;
using GLAA.Web.Controllers;

namespace GLAA.Web.FormLogic
{
    public class FieldConfiguration : IFieldConfiguration
    {
        public FieldConfiguration()
        {
            Fields =
                new Dictionary<FormSection, FormPageDefinition[]>
                {
                    {
                        FormSection.Declaration,
                        new []
                        {
                            new FormPageDefinition(nameof(DeclarationViewModel)), 
                        }
                    },
                    {
                        FormSection.Eligibility,
                        new []
                        {
                            new FormPageDefinition(nameof(EligibilityViewModel.SuppliesWorkers)), 
                            new FormPageDefinition(nameof(EligibilityViewModel.OperatingIndustries)), 
                            new FormPageDefinition(nameof(EligibilityViewModel.Turnover)),
                            new FormPageDefinition(nameof(EligibilityViewModel.EligibilitySummary)),
                            new FormPageDefinition()
                        }
                    },
                    {
                        FormSection.OrganisationDetails,
                        new[]
                        {
                            new FormPageDefinition(),
                            new FormPageDefinition(nameof(OrganisationNameViewModel.OrganisationName)),
                            new FormPageDefinition(nameof(TradingNameViewModel.TradingName)),
                            new FormPageDefinition("Address"),
                            new FormPageDefinition(nameof(BusinessPhoneNumberViewModel.BusinessPhoneNumber)),
                            new FormPageDefinition(nameof(BusinessMobileNumberViewModel.BusinessMobileNumber)),
                            new FormPageDefinition(nameof(BusinessEmailAddressViewModel.BusinessEmailAddress)),
                            new FormPageDefinition(nameof(BusinessWebsiteViewModel.BusinessWebsite)),
                            new FormPageDefinition(nameof(CommunicationPreferenceViewModel.CommunicationPreference)),
                            new FormPageDefinition(nameof(LegalStatusViewModel.LegalStatus)),
                            new FormPageDefinition("PAYEERNStatus"),
                            new FormPageDefinition("VATStatus"),
                            new FormPageDefinition("TaxReference"),
                            new FormPageDefinition(nameof(OperatingIndustriesViewModel.OperatingIndustries)),
                            new FormPageDefinition("Turnover"),
                            new FormPageDefinition(nameof(OperatingCountriesViewModel.OperatingCountries)),
                            new FormPageDefinition()
                        }
                    },
                    {
                        FormSection.PrincipalAuthority,
                        
                        new[]
                        {
                            new FormPageDefinition(),
                            new FormPageDefinition(nameof(IsDirectorViewModel.IsDirector)),
                            new FormPageDefinition("PrincipalAuthorityConfirmation"),
                        }
                        .Concat(BasicPersonFields)
                        .Concat(new [] {
                            new FormPageDefinition(nameof(NationalityViewModel.Nationality)),
                            new FormPageDefinition(nameof(PassportViewModel)),
                            new FormPageDefinition(nameof(PrincipalAuthorityRightToWorkViewModel)),
                            new FormPageDefinition(nameof(UndischargedBankruptViewModel)),
                            new FormPageDefinition(nameof(DisqualifiedDirectorViewModel)),
                            new FormPageDefinition(nameof(RestraintOrdersViewModel), true),
                            new FormPageDefinition(nameof(RestraintOrdersViewModel)),
                            new FormPageDefinition(nameof(UnspentConvictionsViewModel), true),
                            new FormPageDefinition(nameof(UnspentConvictionsViewModel)),
                            new FormPageDefinition(nameof(OffencesAwaitingTrialViewModel), true),
                            new FormPageDefinition(nameof(OffencesAwaitingTrialViewModel)),
                            new FormPageDefinition(nameof(PreviousLicenceViewModel)),
                            new FormPageDefinition("PreviousTradingNames", true),
                            new FormPageDefinition("PreviousTradingNames"),
                            new FormPageDefinition(nameof(PreviousExperienceViewModel.PreviousExperience)),
                            new FormPageDefinition()
                        }).ToArray()
                    },
                    {
                        FormSection.AlternativeBusinessRepresentatives,
                        new []
                        {
                            new FormPageDefinition(),
                            new FormPageDefinition(),
                            new FormPageDefinition()
                        }
                    },
                    {
                        FormSection.AlternativeBusinessRepresentative,
                        BasicPersonFields
                        .Concat(PersonSecurityFields)
                        .Concat(new[] {
                            new FormPageDefinition()
                        }).ToArray()
                    },
                    {
                        FormSection.DirectorOrPartner,
                        new[]
                        {
                            new FormPageDefinition(nameof(IsPreviousPrincipalAuthorityViewModel.IsPreviousPrincipalAuthority)),
                        }
                        .Concat(BasicPersonFields)
                        .Concat(PersonSecurityFields)
                        .Concat(new [] {
                            new FormPageDefinition()
                        }).ToArray()                        
                    },
                    {
                        FormSection.DirectorsOrPartners,
                        new []
                        {
                            new FormPageDefinition(),
                            new FormPageDefinition(),
                            new FormPageDefinition()
                        }
                    },
                    {
                        FormSection.NamedIndividual,
                        new[]
                        {
                            new FormPageDefinition(nameof(FullNameViewModel.FullName)),
                            new FormPageDefinition(nameof(DateOfBirthViewModel.DateOfBirth)),
                            new FormPageDefinition(nameof(BusinessPhoneNumberViewModel.BusinessPhoneNumber)),
                            new FormPageDefinition(nameof(BusinessExtensionViewModel.BusinessExtension)),
                            new FormPageDefinition(nameof(RightToWorkViewModel)),
                            new FormPageDefinition(nameof(UndischargedBankruptViewModel)),
                            new FormPageDefinition(nameof(DisqualifiedDirectorViewModel)),
                            new FormPageDefinition(nameof(RestraintOrdersViewModel), true),
                            new FormPageDefinition(nameof(RestraintOrdersViewModel)),
                            new FormPageDefinition(nameof(UnspentConvictionsViewModel), true),
                            new FormPageDefinition(nameof(UnspentConvictionsViewModel)),
                            new FormPageDefinition(nameof(OffencesAwaitingTrialViewModel), true),
                            new FormPageDefinition(nameof(OffencesAwaitingTrialViewModel)),
                            new FormPageDefinition(nameof(PreviousLicenceViewModel)),
                            new FormPageDefinition()
                        }
                    },
                    {
                        FormSection.JobTitle,
                        new[]
                        {
                            new FormPageDefinition(),
                            new FormPageDefinition()
                        }
                    },
                    {
                        FormSection.NamedIndividuals,
                        new []
                        {
                            new FormPageDefinition(),
                            new FormPageDefinition(),
                            new FormPageDefinition()
                        }
                    },
                    {
                        FormSection.Organisation,
                        new[]
                        {
                            new FormPageDefinition(),
                            new FormPageDefinition(nameof(OutsideSectorsViewModel)),
                            new FormPageDefinition(nameof(WrittenAgreementViewModel)),
                            new FormPageDefinition(nameof(PSCControlledViewModel)),
                            new FormPageDefinition(nameof(MultipleBranchViewModel)),
                            new FormPageDefinition(nameof(TransportingWorkersViewModel)),
                            new FormPageDefinition(nameof(AccommodatingWorkersViewModel)),
                            new FormPageDefinition(nameof(SourcingWorkersViewModel)),
                            new FormPageDefinition(nameof(WorkerSupplyMethodViewModel)),
                            new FormPageDefinition(nameof(WorkerContractViewModel)),
                            new FormPageDefinition(nameof(BannedFromTradingViewModel)),
                            new FormPageDefinition(nameof(SubcontractorViewModel)),
                            new FormPageDefinition(nameof(ShellfishWorkerNumberViewModel)),
                            new FormPageDefinition(nameof(ShellfishWorkerNationalityViewModel)),
                            new FormPageDefinition(nameof(PreviouslyWorkedInShellfishViewModel)),
                            new FormPageDefinition()
                        }
                    }
                };
        }

        private static readonly FormPageDefinition[] BasicPersonFields =
        {
            new FormPageDefinition(nameof(FullNameViewModel.FullName)),
            new FormPageDefinition(nameof(AlternativeFullNameViewModel.AlternativeName)),
            new FormPageDefinition(nameof(DateOfBirthViewModel.DateOfBirth)),
            new FormPageDefinition(nameof(TownOfBirthViewModel.TownOfBirth)),
            new FormPageDefinition(nameof(CountryOfBirthViewModel.CountryOfBirth)),
            new FormPageDefinition(nameof(JobTitleViewModel.JobTitle)),
            new FormPageDefinition("Address"),
            new FormPageDefinition(nameof(BusinessPhoneNumberViewModel.BusinessPhoneNumber)),
            new FormPageDefinition(nameof(BusinessExtensionViewModel.BusinessExtension)),
            new FormPageDefinition(nameof(PersonalMobileNumberViewModel.PersonalMobileNumber)),
            new FormPageDefinition(nameof(PersonalEmailAddressViewModel.PersonalEmailAddress)),
            new FormPageDefinition(nameof(NationalInsuranceNumberViewModel.NationalInsuranceNumber)),
        };

        private static readonly FormPageDefinition[] PersonSecurityFields =
        {
            new FormPageDefinition(nameof(NationalityViewModel.Nationality)),
            new FormPageDefinition(nameof(PassportViewModel)),
            new FormPageDefinition(nameof(RightToWorkViewModel)),
            new FormPageDefinition(nameof(UndischargedBankruptViewModel)),
            new FormPageDefinition(nameof(DisqualifiedDirectorViewModel)),
            new FormPageDefinition(nameof(RestraintOrdersViewModel), true),
            new FormPageDefinition(nameof(RestraintOrdersViewModel)),
            new FormPageDefinition(nameof(UnspentConvictionsViewModel), true),
            new FormPageDefinition(nameof(UnspentConvictionsViewModel)),
            new FormPageDefinition(nameof(OffencesAwaitingTrialViewModel), true),
            new FormPageDefinition(nameof(OffencesAwaitingTrialViewModel)),
            new FormPageDefinition(nameof(PreviousLicenceViewModel)),
        };

        public IDictionary<FormSection, FormPageDefinition[]> Fields { get; set; }
    }
}