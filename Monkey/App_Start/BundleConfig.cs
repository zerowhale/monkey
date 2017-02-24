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





            bundles.Add(new StyleBundle("~/bundles/pandemic/css").Include(
                        "~/Content/Pandemic/game.css"));

            bundles.Add(new ScriptBundle("~/bundles/pandemic/js").Include(
                        "~/Scripts/Monkey/Pandemic/*.js",
                        "~/Scripts/Monkey/Pandemic/Overlays/*.js"
                        ));


            bundles.Add(new StyleBundle("~/bundles/agricola/css").Include(
                        "~/Content/Agricola/game.css"));

            bundles.Add(new ScriptBundle("~/bundles/agricola/js").Include(
                        "~/Scripts/Lib/astar.js",
                        "~/Scripts/Monkey/Agricola/Popup.js",
                        "~/Scripts/Monkey/Agricola/PlayerBoard.js",
                        "~/Scripts/Monkey/Agricola/Game.js",
                        "~/Scripts/Monkey/Agricola/FenceValidator.js",
                        "~/Scripts/Monkey/Agricola/FenceUtils.js",
                        "~/Scripts/Monkey/Agricola/AnimalManager.js",
                        "~/Scripts/Monkey/Agricola/Curator.js",
                        "~/Scripts/Monkey/Agricola/UI/*.js",
                        "~/Scripts/Monkey/Agricola/UI/Popups/Popup.js",
                        "~/Scripts/Monkey/Agricola/UI/Popups/FarmyardPopup.js",
                        "~/Scripts/Monkey/Agricola/UI/Popups/FarmyardWithAnimalsPopup.js",
                        "~/Scripts/Monkey/Agricola/UI/Popups/AnimalChoicePopup.js"));


            bundles.Add(new StyleBundle("~/bundles/catan/css").Include(
                        "~/Content/Catan/game.css"));

            bundles.Add(new ScriptBundle("~/bundles/catan/js").Include(
                        "~/Scripts/Lib/three.min.js",
                        "~/Scripts/Monkey/Catan/Rendering/*.js",
                        "~/Scripts/Monkey/Catan/*.js"));



            bundles.Add(new ScriptBundle("~/bundles/debug").Include(
                        "~/Scripts/Monkey/Debug/Debug.js"));


            bundles.Add(new StyleBundle("~/Login/css").Include(
                        "~/Content/login.css"));

            bundles.Add(new StyleBundle("~/bundles/pushpull/css").Include(
                        "~/Content/PushPull/game.css"));

            bundles.Add(new ScriptBundle("~/bundles/pushpull/js").Include(
                        "~/Scripts/Monkey/PushPull/Game.js"));


        }
    }
}
