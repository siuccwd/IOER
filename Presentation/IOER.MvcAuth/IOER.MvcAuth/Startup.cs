using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IOER.MvcAuth.Startup))]
namespace IOER.MvcAuth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
