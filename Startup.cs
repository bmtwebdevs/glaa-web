using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GLAA.Web.Startup))]
namespace GLAA.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
