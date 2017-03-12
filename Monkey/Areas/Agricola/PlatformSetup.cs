using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Monkey.Game;
using System.Web.Optimization;
using System.Web;

namespace Monkey.Games.Agricola
{
    public class PlatformSetup: IGamePlatformSetup
    {

        public void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new StyleBundle("~/bundles/agricola/css").Include(
                        "~/Areas/Agricola/Content/game.css"));

            bundles.Add(new ScriptBundle("~/bundles/agricola/js").Include(
                        "~/Scripts/Lib/astar.js",
                        "~/Areas/Agricola/Scripts/Popup.js",
                        "~/Areas/Agricola/Scripts/PlayerBoard.js",
                        "~/Areas/Agricola/Scripts/Game.js",
                        "~/Areas/Agricola/Scripts/FenceValidator.js",
                        "~/Areas/Agricola/Scripts/FenceUtils.js",
                        "~/Areas/Agricola/Scripts/AnimalManager.js",
                        "~/Areas/Agricola/Scripts/Curator.js",
                        "~/Areas/Agricola/Scripts/UI/*.js",
                        "~/Areas/Agricola/Scripts/UI/Popups/Popup.js",
                        "~/Areas/Agricola/Scripts/UI/Popups/FarmyardPopup.js",
                        "~/Areas/Agricola/Scripts/UI/Popups/FarmyardWithAnimalsPopup.js",
                        "~/Areas/Agricola/Scripts/UI/Popups/AnimalChoicePopup.js"));


        }

        public void LoadGameData()
        {
            HttpContext context = HttpContext.Current;

            context.Application["JsonMajorImprovements"] = Curator.LoadMajorImprovements(System.Web.Hosting.HostingEnvironment.MapPath("/Areas/Agricola/App_Data/MajorImprovements.xml"));
            context.Application["JsonOccupations"] = Curator.LoadOccupations(System.Web.Hosting.HostingEnvironment.MapPath("/Areas/Agricola/App_Data/Occupations.xml"));
            context.Application["JsonMinorImprovements"] = Curator.LoadMinorImprovements(System.Web.Hosting.HostingEnvironment.MapPath("/Areas/Agricola/App_Data/MinorImprovements.xml"));
        }
    }
}