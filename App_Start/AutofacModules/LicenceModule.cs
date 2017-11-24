using System.Diagnostics.CodeAnalysis;
using System.Web;
using Autofac;
using GLAA.Domain;
using GLAA.Repository;
using GLAA.Services;
using GLAA.Services.LicenceApplication;
using GLAA.Web.FormLogic;
using GLAA.Web.Helpers;

namespace GLAA.Web.App_Start.AutofacModules
{
    [ExcludeFromCodeCoverage]
    public class LicenceModule : Module
    {
        private readonly string connectionString;
        public LicenceModule(string connectionString)
        {
            this.connectionString = connectionString;
        }
        protected override void Load(ContainerBuilder builder)
        {            
            builder.Register(x => new GLAAContext(this.connectionString)).As<GLAAContext>().InstancePerRequest();

            builder.Register(x => x.Resolve<HttpContextBase>().Session).As<HttpSessionStateBase>().InstancePerRequest();

            builder.RegisterType<DateTimeProvider>().As<IDateTimeProvider>().InstancePerRequest();
            builder.RegisterType<SessionHelper>().As<ISessionHelper>().InstancePerRequest();

            builder.RegisterType<EntityFrameworkRepositoryBase>().As<IEntityFrameworkRepository>().InstancePerRequest();
            builder.RegisterType<LicenceRepository>().As<ILicenceRepository>().InstancePerRequest();
            builder.RegisterType<StatusRepository>().As<IStatusRepository>().InstancePerRequest();

            builder.RegisterType<LicenceApplicationPostDataHandler>().As<ILicenceApplicationPostDataHandler>().InstancePerRequest();
            builder.RegisterType<LicenceApplicationViewModelBuilder>().As<ILicenceApplicationViewModelBuilder>().InstancePerRequest();
            builder.RegisterType<LicenceStatusViewModelBuilder>().As<ILicenceStatusViewModelBuilder>().InstancePerRequest();

            builder.RegisterType<LicenceApplicationFormDefinition>().As<IFormDefinition>().InstancePerRequest();
            builder.RegisterType<FieldConfiguration>().As<IFieldConfiguration>().InstancePerRequest();

            base.Load(builder);
        }
    }
}