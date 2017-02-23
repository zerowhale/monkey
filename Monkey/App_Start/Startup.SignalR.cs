using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;

[assembly: OwinStartup(typeof(Monkey.Startup))]
namespace Monkey
{
    public partial class Startup
    {
        public void ConfigureSignalR(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}