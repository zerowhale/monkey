using Monkey.Game;
using Monkey.Games.Agricola;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Monkey
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            LoadGamePlatformSetup();
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Web.config")));


        }

        private void LoadGamePlatformSetup()
        {
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // This only checks the current assembly. 
            var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(IGamePlatformSetup)));
            foreach (Type type in types)
            {
                var instance = Activator.CreateInstance(type) as IGamePlatformSetup;
                instance.RegisterBundles(BundleTable.Bundles);
                instance.LoadGameData();
            }
        }

    }
}
