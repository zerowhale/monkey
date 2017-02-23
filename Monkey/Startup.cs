using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Monkey.Startup))]
namespace Monkey
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureSignalR(app);
        }
    }
}
