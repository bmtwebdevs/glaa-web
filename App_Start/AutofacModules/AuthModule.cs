using System.Diagnostics.CodeAnalysis;
using System.Web;
using Autofac;
using Autofac.Core;
using GLAA.Domain;
using GLAA.Domain.Auth;
using GLAA.Domain.Models;
using GLAA.Repository;
using GLAA.Services.Admin;
using GLAA.Web.App_Start;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace GLAA.Web.AutofacModules
{
    [ExcludeFromCodeCoverage]
    public class AuthModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppDataProtectionProvider>().As<IAppDataProtectionProvider>().InstancePerRequest();

            builder.RegisterType<GLAAContext>().AsSelf().InstancePerRequest();

            builder.RegisterType<ApplicationUserManager>().AsSelf().InstancePerRequest();
            builder.RegisterType<ApplicationSignInManager>().AsSelf().InstancePerRequest();
            builder.Register(c => new UserStore<GLAAUser>(c.Resolve<GLAAContext>())).AsImplementedInterfaces().InstancePerRequest();
            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication).As<IAuthenticationManager>();
            builder.Register(c => new IdentityFactoryOptions<ApplicationUserManager>
            {
                DataProtectionProvider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("GLAA​")
            });

            base.Load(builder);
        }
    }
}