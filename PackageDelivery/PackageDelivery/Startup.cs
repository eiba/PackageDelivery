using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PackageDelivery.Startup))]
namespace PackageDelivery
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
