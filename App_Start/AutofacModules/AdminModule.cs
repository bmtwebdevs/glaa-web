using System.Diagnostics.CodeAnalysis;
using Autofac;
using GLAA.Repository;
using GLAA.Services.Admin;

namespace GLAA.Web.App_Start.AutofacModules
{
    [ExcludeFromCodeCoverage]
    public class AdminModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LicenceRepository>().As<ILicenceRepository>().InstancePerRequest();

            builder.RegisterType<AdminHomeViewModelBuilder>().As<IAdminHomeViewModelBuilder>().InstancePerRequest();

            builder.RegisterType<AdminLicenceListViewModelBuilder>().As<IAdminLicenceListViewModelBuilder>().InstancePerRequest();

            builder.RegisterType<AdminLicenceViewModelBuilder>().As<IAdminLicenceViewModelBuilder>()
                .InstancePerRequest();

            builder.RegisterType<AdminLicencePostDataHandler>().As<IAdminLicencePostDataHandler>()
                .InstancePerRequest();

            base.Load(builder);
        }
    }
}