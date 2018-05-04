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
using System.IO;

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
                int result = authProvider.Authenticate(loginData);
                switch(result)
                {
                    case -2:
                        {
                            Session["LogInAlert"] = "Błąd wewnętrzny systemu";
                            break;
                        }
                    case -1:
                        {
                            Session["LogInAlert"] = "Błędne dane logowania";
                            break;
                        }
                    case 0:
                        {
                            Session["LogInAlert"] = "Konto zablokowane czasowo";
                            break;
                        }
                    case 1:
                        {
                            Session["LogInAlert"] = "";
                            break;
                        }
                    default:
                        {
                            Session["LogInAlert"] = "Błąd wewnętrzny systemu";
                            break;
                        }
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost, Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
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
                        byte[] aPAbbrevB = sha512.ComputeHash(Encoding.Default.GetBytes(actualP));
                        string aPAbbrev = BitConverter.ToString(aPAbbrevB).Replace("-", string.Empty);//512 bit hash password
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
                                    byte[] newaPAbbrevB = sha512.ComputeHash(Encoding.Default.GetBytes(newP));
                                    string newPAbbrev = BitConverter.ToString(newaPAbbrevB).Replace("-", string.Empty);//512 bit hash password
                                    string newPAbbrevDate = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes(newPAbbrev + now))).Replace("-", string.Empty);//512 bit hash password
                                    admin.Password = newPAbbrevDate;
                                    dbContext.Entry(admin).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                    //ReEncrypt factory reset password
                                    using (var sha256 = SHA256.Create())
                                    {
                                        byte[] oldKey = sha256.ComputeHash(Encoding.Default.GetBytes(aPAbbrevDate));
                                        byte[] newKey = sha256.ComputeHash(Encoding.Default.GetBytes(newPAbbrevDate));

                                        ReEncrypt(oldKey, newKey, dbContext.Settings.FirstOrDefault().Password);
                                        Session["ChangePassword"] = false;
                                        return Json("Success");
                                    }
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
        public List<int> RandIndexes()
        {
            List<int> blockedIndexes = new List<int>();
            List<int> nonBlockedIndexes = new List<int>();
            for (int i = 1; i <= 16; i++)
            {
                nonBlockedIndexes.Add(i);
            }
            Random rand = new Random();
            int index = -1;
            for (int i = 0; i < 8; i++)
            {
                do
                {
                    index = rand.Next(1, 17);
                } while (blockedIndexes.Contains(index));
                blockedIndexes.Add(index);
                nonBlockedIndexes.Remove(index);
            }
            TempData["NonBlockedIndexes"] = nonBlockedIndexes;
            return blockedIndexes;
        }
        public ActionResult FactoryReset()
        {
            return View(RandIndexes());
        }
        [HttpPost]
        public ActionResult FactoryReset(List<string> indexes)
        {
            using (var context = new SMSDBContext())
            {
                byte[] key;
                using (var sha256 = SHA256.Create())
                {
                    key = sha256.ComputeHash(Encoding.Default.GetBytes(context.Admins.SingleOrDefault().Password));
                }
                string password = decrypt(key, System.Convert.FromBase64String(context.Settings.SingleOrDefault().Password), "admin");

                if (indexes.Count == 8)
                {
                    int iter = 0;
                    List<int> nonBlockedIndexes = (List<int>)TempData["NonBlockedIndexes"];
                    if (nonBlockedIndexes != null)
                    {
                        foreach (var non in nonBlockedIndexes)
                        {
                            if (password[non - 1] == indexes[iter][0])
                            {
                                iter++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (iter == 8)
                        {
                            ViewData["Result"] = "Success";
                            if (Request.IsAuthenticated)
                            {
                                FormsAuthentication.SignOut();
                            }
                            //***** Clear tables
                            //Admins
                            var admins = context.Admins;
                            context.Admins.RemoveRange(admins);
                            //ScansAndDevices
                            var scansAndDevices = context.ScansAndDevices;
                            context.ScansAndDevices.RemoveRange(scansAndDevices);
                            //Devices
                            var devices = context.Devices;
                            context.Devices.RemoveRange(devices);
                            //Scans
                            var scans = context.Scans;
                            context.Scans.RemoveRange(scans);
                            //Settings
                            var settings = context.Settings;
                            context.Settings.RemoveRange(settings);

                            context.SaveChanges();

                            //Reseed
                            context.Database.ExecuteSqlCommand("update sqlite_sequence set seq = 0 where name = 'Admins'");
                            context.Database.ExecuteSqlCommand("update sqlite_sequence set seq = 0 where name = 'Devices'");
                            context.Database.ExecuteSqlCommand("update sqlite_sequence set seq = 0 where name = 'Scans'");
                            context.Database.ExecuteSqlCommand("update sqlite_sequence set seq = 0 where name = 'Settings'");

                            context.SaveChanges();


                            using (var sha512 = SHA512.Create())
                            {
                                DateTime time = DateTime.MinValue;
                                string pAbbrev = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes("9CE1EB62332A144B0A752460F9E789B2E4A6D7403D2E18041C4E80352DB736C51FD247301E079CEF9EDE13DFDCF3D040A3F0843E4D92073FDEA29F5838C421F3" + time))).Replace("-", string.Empty);
                                Admin admin = new Admin();
                                admin.Username = "admin";
                                admin.Password = pAbbrev;
                                context.Admins.Add(admin);
                            }
                            context.Settings.Add(new Setting());
                            context.SaveChanges();
                            //******

                        }
                        else
                        {
                            ViewData["Result"] = "Error";
                        }
                    }
                }

            }
            return View(RandIndexes());
        }
        public void ReEncrypt(byte[] oldKey, byte[] newKey, string message)
        {
            string password = decrypt(oldKey, System.Convert.FromBase64String(message), "admin");
            string encrypted = System.Convert.ToBase64String(encrypt(newKey, password, "admin"));
            using (var context = new SMSDBContext())
            {
                Setting setting = context.Settings.FirstOrDefault();
                setting.Password = encrypted;
                context.Entry(setting).State = EntityState.Modified;
                context.SaveChanges();
            }
        }
        public byte[] encrypt(byte[] key, string secretMessage, string login)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] IV = (sha256.ComputeHash(Encoding.Default.GetBytes(login))).Take(16).ToArray();
                using (Aes aes = new AesCryptoServiceProvider())
                {
                    aes.Key = key;
                    aes.IV = IV;
                    // Encrypt the message
                    using (MemoryStream ciphertext = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plaintextMessage = Encoding.UTF8.GetBytes(secretMessage);
                        cs.Write(plaintextMessage, 0, plaintextMessage.Length);
                        cs.Close();
                        byte[] encryptedMessage = ciphertext.ToArray();
                        return encryptedMessage;
                    }
                }
            }
        }
        public string decrypt(byte[] key, byte[] encryptedMessage, string login)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] IV = (sha256.ComputeHash(Encoding.Default.GetBytes(login))).Take(16).ToArray();
                using (Aes aes = new AesCryptoServiceProvider())
                {
                    aes.Key = key;
                    aes.IV = IV;
                    // Decrypt the message
                    using (MemoryStream plaintext = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(encryptedMessage, 0, encryptedMessage.Length);
                            cs.Close();
                            string message = Encoding.UTF8.GetString(plaintext.ToArray());
                            return message;
                        }
                    }
                }
            }
        }
    }
}