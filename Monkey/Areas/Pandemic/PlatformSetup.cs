using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Monkey.Game;
using System.Web.Optimization;

namespace Monkey.Games.Pandemic
{
    public class PlatformSetup: IGamePlatformSetup
    {

        public void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/bundles/pandemic/css").Include(
                        "~/Areas/Pandemic/Content/game.css"));

            bundles.Add(new ScriptBundle("~/bundles/pandemic/js").Include(
                        "~/Areas/Pandemic/Scripts/*.js",
                        "~/Areas/Pandemic/Scripts/Overlays/*.js"
                        ));
        }

        public void LoadGameData()
        {
        }
    }
}