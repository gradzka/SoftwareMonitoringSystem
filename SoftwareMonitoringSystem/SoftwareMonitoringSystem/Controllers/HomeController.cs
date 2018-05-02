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
                authProvider.CheckDefaultPassword(this);
                return RedirectToAction("GetDevices", "DevMGMT");
            }
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
                                    return Json("Wprowadź hasło spełniające kryteria: 8 znaków, 1 wielka litera, 1 mała litera, 1 cyfra");
                                }
                            }
                            else
                            {
                                return Json("Wprowadź poprawne hasła");
                            }
                        }
                        else
                        {
                            return Json("Wprowadź poprawne hasła");
                        }
                    }
                }
                else
                {
                    return Json("Spróbuj ponownie (błąd wewnętrzny aplikacji)");
                }
            }
        }
        public ActionResult FactoryReset()
        {
            List<int> blockedIndexes = new List<int>();
            Random rand = new Random();
            int index = -1;
            for (int i = 0; i < 8; i++)
            {
                do
                {
                    index = rand.Next(1, 17);
                } while (blockedIndexes.Contains(index));
                blockedIndexes.Add(index);
            }
            return View(blockedIndexes);
        }
        [HttpPost]
        public ActionResult FactoryReset(List<int> indexes)
        {
            return RedirectToAction("FactoryReset", "Home");
        }
    }
}