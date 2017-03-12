using System.Web.Mvc;

namespace Monkey.Areas.Agricola
{
    public class AgricolaAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Agricola";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            /*
            context.MapRoute(
                "Agricola_default",
                "Agricola/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
             * */
        }
    }
}