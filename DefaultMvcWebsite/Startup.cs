using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DefaultMvcWebsite.Startup))]
namespace DefaultMvcWebsite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
