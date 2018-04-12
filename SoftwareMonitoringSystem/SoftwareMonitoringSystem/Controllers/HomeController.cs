using SoftwareMonitoringSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SoftwareMonitoringSystem.Infrastructure.Abstract;
using SoftwareMonitoringSystem.Infrastructure.Concrete;
using System.Web.Security;

namespace SoftwareMonitoringSystem.Controllers
{
    public class HomeController : Controller
    {
        IAuthProvider authProvider;
        public HomeController()
        {
            authProvider = new FormsAuthProvider();
        }
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                //FormsAuthentication.SignOut();
                return RedirectToAction("GetDevices", "DevMGMT");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult LogIn(Login loginData)
        {
            if (ModelState.IsValid)
            {
                if (authProvider.Authenticate(loginData))
                {
                    return RedirectToAction("GetDevices", "DevMGMT");
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}