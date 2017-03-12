using System.Web.Mvc;

namespace Monkey.Areas.Catan
{
    public class CatanAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Catan";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            /*
            context.MapRoute(
                "Catan_default",
                "Catan/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );*/
        }
    }
}