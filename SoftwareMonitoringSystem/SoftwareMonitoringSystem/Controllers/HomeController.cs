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
using System.Text.RegularExpressions;
using System.Data.Entity;

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
                //Check password
                using (var dbContext = new SMSDBContext())
                {
                    SHA512 sha512 = SHA512.Create();
                    Admin admin = dbContext.Admins.SingleOrDefault();
                    if (admin != null)
                    {
                        //static password
                        string staticPassword = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes("9CE1EB62332A144B0A752460F9E789B2E4A6D7403D2E18041C4E80352DB736C51FD247301E079CEF9EDE13DFDCF3D040A3F0843E4D92073FDEA29F5838C421F3" + admin.LastEditDate))).Replace("-", string.Empty);//512 bit hash password
                        if (staticPassword.Equals(admin.Password))
                        {
                            TempData["ChangePassword"] = true;
                        }
                        else
                        {
                            TempData["ChangePassword"] = false;
                        }
                    }                   
                }
                
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
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost, Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet, Authorize]
        public ActionResult ChangePass()
        {
            return View();
        }
        [HttpPost, Authorize]
        public ActionResult ChangePass(String actualP, String newP, String confirmNewP)
        {
            using (var dbContext = new SMSDBContext())
            {
                Admin admin = dbContext.Admins.FirstOrDefault();
                if (admin != null)
                {
                    using (var sha512 = SHA512.Create())
                    {
                        DateTime editDate = dbContext.Admins.Select(x => x.LastEditDate).FirstOrDefault();
                        string aPAbbrev = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes(actualP))).Replace("-", string.Empty);//512 bit hash password
                        string aPAbbrevDate = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes(aPAbbrev + editDate))).Replace("-", string.Empty);
                        if (admin.Password.Equals(aPAbbrevDate)) //actualP is proper
                        {
                            if (newP.Equals(confirmNewP)) // if newP == confirmP
                            {
                                Regex regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$");
                                /* 8 signs
                                 * 1 upperCase
                                 * 1 lowerCase
                                 * 1 digit
                                 */
                                Match match = regex.Match(newP);
                                if (match.Success)
                                {
                                        DateTime now = DateTime.Now;
                                        admin.LastEditDate = now;
                                        string newPAbbrev = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes(newP))).Replace("-", string.Empty);//512 bit hash password
                                        string newPAbbrevDate = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes(newPAbbrev + now))).Replace("-", string.Empty);//512 bit hash password
                                        admin.Password = newPAbbrevDate;
                                        dbContext.Entry(admin).State = EntityState.Modified;
                                        dbContext.SaveChanges();
                                        return Json("Success");
                                }
                                else
                                {
                                    return Json("Passwords don't meet the requirements");
                                }
                            }
                            else
                            {
                                return Json("Please type proper passwords");
                            }
                        }
                        else
                        {
                            return Json("Please type proper passwords");
                        }
                    }
                }
                else
                {
                    return Json("Please try once more because of internal error");
                }
            }
        }

    }
}