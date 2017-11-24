using System.Diagnostics.CodeAnalysis;
using Autofac;
using Autofac.Integration.Mvc;
using AutoMapper;
using GLAA.Web.App_Start.AutofacModules;
using System.Web.Mvc;
using GLAA.Services.Automapper;
using GLAA.Web.AutofacModules;
using Owin;

namespace GLAA.Web.App_Start
{
    [ExcludeFromCodeCoverage]
    public class AutofacConfig
    {
        public static void ConfigureContainer()
        {
            var builder = new ContainerBuilder();
                        
            builder.Register(x => new AutoMapperConfig().Configure().CreateMapper()).As<IMapper>().InstancePerLifetimeScope();

            builder.RegisterModule(new AutofacWebTypesModule());

            // Register dependencies in controllers
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register dependencies in filter attributes
            builder.RegisterFilterProvider();

            // Register dependencies in custom views
            builder.RegisterSource(new ViewRegistrationSource());

            // Register our dependencies
            builder.RegisterModule(new AuthModule());
            builder.RegisterModule(new LicenceModule("GLAAContext"));
            builder.RegisterModule(new AdminModule());
            
            var container = builder.Build();

            // Set MVC DI resolver to use our Autofac container
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}