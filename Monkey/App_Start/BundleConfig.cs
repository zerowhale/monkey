using System.Web;
using System.Web.Optimization;

namespace Monkey
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/Lib/jquery-{version}.js",
                        "~/Scripts/Lib/jquery-ui.min.js",
                        "~/Scripts/Lib/jquery.easing-{version}.js",
                        "~/Scripts/Lib/jquery.cookie.js",
                        "~/Scripts/Lib/jquery.signalR-{version}.js",
                        "~/Scripts/Lib/store.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/Lib/jquery.validate*"));


            bundles.Add(new ScriptBundle("~/bundles/site/js").Include(
                        "~/Scripts/Monkey/Utils.js"));

            bundles.Add(new StyleBundle("~/bundles/site/css").Include(
                        "~/Content/site.css"));



            bundles.Add(new ScriptBundle("~/bundles/lobby/js").Include(
                        "~/Scripts/Monkey/Lobby/LobbyPanel.js",
                        "~/Scripts/Monkey/Lobby/GameLobbyScreen.js",
                        "~/Scripts/Monkey/Lobby/Lobby.js"));

            bundles.Add(new StyleBundle("~/bundles/lobby/css").Include(
                        "~/Content/Lobby/lobby.css"));


            bundles.Add(new ScriptBundle("~/bundles/game").Include(
                        "~/Scripts/Monkey/GameInterface.js",
                        "~/Scripts/Monkey/ExternalLog.js"));



            bundles.Add(new ScriptBundle("~/bundles/debug").Include(
                        "~/Scripts/Monkey/Debug/Debug.js"));


            bundles.Add(new StyleBundle("~/Login/css").Include(
                        "~/Content/login.css"));

           
        }
    }
}
