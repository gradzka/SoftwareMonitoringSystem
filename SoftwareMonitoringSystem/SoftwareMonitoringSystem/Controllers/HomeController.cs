using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SoftwareMonitoringSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //only for checking
            var db = new SMSDBContext();
            Admin admin = new Admin();
            admin.Username = "Admin";
            admin.Password = "123456";
            db.Admins.Add(admin);
            db.SaveChanges();
            var admins = db.Admins.ToList();
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
    }
}