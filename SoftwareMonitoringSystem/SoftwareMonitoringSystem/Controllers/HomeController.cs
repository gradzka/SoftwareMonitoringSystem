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
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
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
                                        byte[] oldKey = sha256.ComputeHash(Encoding.Default.GetBytes(aPAbbrev));
                                        byte[] newKey = sha256.ComputeHash(Encoding.Default.GetBytes(newPAbbrev));
                                        
                                        ReEncrypt(oldKey, newKey,dbContext.Settings.FirstOrDefault().Password);
                                        TempData["ChangePassword"] = false;
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
        public ActionResult FactoryReset()
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
            return View(blockedIndexes);
        }
        [HttpPost]
        public ActionResult FactoryReset(List<int> indexes)
        {
            if (indexes.Count == 8)
            {
                for (int i = 0; i < 8; i++)
                {
                    //TempData["NonBlockedIndexes"]
                }
            }
            return RedirectToAction("FactoryReset", "Home");
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
                    aes.Key = sha256.ComputeHash(key);
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
                    aes.Key = sha256.ComputeHash(key);
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