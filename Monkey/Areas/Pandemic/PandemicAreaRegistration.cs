using System.Web.Mvc;

namespace Monkey.Areas.Pandemic
{
    public class PandemicAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Pandemic";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            /*
            context.MapRoute(
                "Pandemic_default",
                "Pandemic/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
             * */
        }
    }
}