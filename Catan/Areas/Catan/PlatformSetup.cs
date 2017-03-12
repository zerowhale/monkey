using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Monkey.Game;
using System.Web.Optimization;

namespace Monkey.Games.Catan
{
    public class PlatformSetup: IGamePlatformSetup
    {

        public void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/bundles/catan/css").Include(
                        "~/Areas/Catan/Site/Content/game.css"));

            bundles.Add(new ScriptBundle("~/bundles/catan/js").Include(
                        "~/Scripts/Lib/three.min.js",
                        "~/Areas/Catan/Site/Scripts/Rendering/*.js",
                        "~/Areas/Catan/Site/Scripts/*.js"));
        }
    }
}