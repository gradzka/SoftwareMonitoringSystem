using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SoftwareMonitoringSystem.Infrastructure.Abstract;
using SoftwareMonitoringSystem.Models;

namespace SoftwareMonitoringSystem.Infrastructure.Concrete
{
    public class FormsAuthProvider : IAuthProvider
    {
        public bool Authenticate(Login loginData)
        {
            bool result = ValidateUser(loginData);
            if (result)
            {
                FormsAuthentication.SetAuthCookie(loginData.login, false);
            }
            return result;
        }
        public bool ValidateUser(Login loginData)
        {
            bool result = false;
            using (var dbContext = new SMSDBContext())
            {
                DateTime now = DateTime.Now;
                Admin admin = dbContext.Admins.SingleOrDefault();
                if (loginData.login != "" && loginData.password != "")
                {
                    using (var sha512 = SHA512.Create())
                    {
                        DateTime editdate = admin.LastEditDate;
                        if (editdate != null)
                        {
                            int loginCounter = admin.LogInAttemptCounter;
                            if (loginCounter >= 3)
                            {
                                DateTime? lastLoginAttemptDate = admin.LastLogInAttemptDate;
                                if (lastLoginAttemptDate != null)
                                {
                                    double substraction = DateTime.Now.Subtract(lastLoginAttemptDate.Value).TotalMinutes;
                                    if (substraction > 15)
                                    {
                                        loginCounter = 0;
                                    }
                                }
                            }
                            else if (loginCounter<3)
                            {
                                    string pAbbrev = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes(loginData.password))).Replace("-", string.Empty);
                                    string pAbbrevDate = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes(pAbbrev + editdate))).Replace("-", string.Empty);
                                    if (admin != null)
                                    {
                                        if (admin.Password.Equals(pAbbrevDate))
                                        {
                                            result = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                admin.LastLogInAttemptDate = now;
                if (result == true)
                {
                    admin.LogInAttemptCounter = 0;
                }
                else
                {
                    admin.LogInAttemptCounter++;
                }

                dbContext.Entry(admin).State = EntityState.Modified;
                dbContext.SaveChanges();
                return result;
            }
        }
        public void CheckDefaultPassword(Controller controller)
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
                        controller.TempData["ChangePassword"] = true;
                    }
                    else
                    {
                        controller.TempData["ChangePassword"] = false;
                    }
                }
                else
                {
                    controller.TempData["ChangePassword"] = true;
                }
            }
        }
    }
}