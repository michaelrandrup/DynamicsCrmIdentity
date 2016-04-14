using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DynamicsCrm.WebsiteIntegration.Core;

namespace DefaultMvcWebsite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles="Admin")]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [Authorize(Roles = "Customer,Admin")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Form()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Form([Bind(Prefix = "profile")] Dictionary<string,object> profiles, [Bind(Prefix = "property")] Dictionary<string,object> model, [Bind(Prefix ="setting")] Dictionary<string, string> settings)
        {
            settings.ResolveArrays();
            
            return View(model);
        }
    }
}