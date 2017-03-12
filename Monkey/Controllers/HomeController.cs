using Monkey.Games.Agricola;
using Monkey.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Monkey.Controllers
{
    public class HomeController : Controller
    {

        [Authorize]
        public ActionResult Index()
        {
            return View("Lobby"); 
        }

        /// <summary>
        /// Loads the specified games html
        /// </summary>
        /// <param name="id">The name of the game to load.</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult Game(string id)
        {
            return View("~/Areas/" + id + "/Views/View.cshtml");
        }

        [ActionName("external-log")]
        public ActionResult ExternalLog()
        {
            return View("~/Views/Shared/ExternalLog.cshtml");
        }
    }
}