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
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Web.config")));

            Agricola_Start();

        }

        /// <summary>
        /// Kick off application loading for agricola
        /// </summary>
        private void Agricola_Start()
        {
            // This is the wrong place for this stuff.
            Application["JsonMajorImprovements"] = Curator.LoadMajorImprovements(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Agricola/MajorImprovements.xml"));
            Application["JsonOccupations"] = Curator.LoadOccupations(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Agricola/Occupations.xml"));
            Application["JsonMinorImprovements"] = Curator.LoadMinorImprovements(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Agricola/MinorImprovements.xml"));
        }


    }
}
