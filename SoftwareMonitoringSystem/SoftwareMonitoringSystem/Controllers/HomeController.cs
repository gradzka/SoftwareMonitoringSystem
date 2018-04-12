using SoftwareMonitoringSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SoftwareMonitoringSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Logged"] == "Admin")
            {
                return RedirectToAction("GetDevices", "DevMGMT");
            }
            else
            { 
                return View();
            }
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
            using (var dbContext = new SMSDBContext())
            {
                Login loginDataDB = dbContext.Admins.Select(x => new Login{ login = x.Username, password = x.Password }).FirstOrDefault();
                if (loginDataDB != null)
                {
                    byte[] bytePasswd = Encoding.Default.GetBytes(loginData.password);
                    using (var sha512 = SHA512.Create())
                    {
                        byte[] hashBytePasswd = sha512.ComputeHash(bytePasswd); //512-bits
                        string hashBytePasswdHex = BitConverter.ToString(hashBytePasswd).Replace("-", string.Empty);
                        if (loginData.login == loginDataDB.login && hashBytePasswdHex == loginDataDB.password)
                        {
                            Session["Logged"] = "Admin";
                            return RedirectToAction("Index", "Home");
                            
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}