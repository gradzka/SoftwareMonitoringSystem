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
        public int Authenticate(Login loginData)
        {
            int result = ValidateUser(loginData);
            if (result == 1)
            {
                FormsAuthentication.SetAuthCookie(loginData.login, false);
            }
            return result;
        }
        public int ValidateUser(Login loginData)
        {
            int result = -1;
            using (var dbContext = new SMSDBContext())
            {
                DateTime now = DateTime.Now;
                Admin admin = dbContext.Admins.SingleOrDefault();
                int loginCounter = -1;
                if (loginData.login != "" && loginData.password != "")
                {
                    using (var sha512 = SHA512.Create())
                    {
                        DateTime editdate = admin.LastEditDate;
                        if (editdate != null)
                        {
                            loginCounter = admin.LogInAttemptCounter;
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
                                    else
                                    {
                                        result = 0;
                                    }
                                }
                                else
                                {
                                    result = -2;
                                }
                            }
                            if (loginCounter < 3)
                            {
                                string pAbbrev = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes(loginData.password))).Replace("-", string.Empty);
                                string pAbbrevDate = BitConverter.ToString(sha512.ComputeHash(Encoding.Default.GetBytes(pAbbrev + editdate))).Replace("-", string.Empty);
                                if (admin != null)
                                {
                                    if (admin.Password.Equals(pAbbrevDate))
                                    {
                                        result = 1;
                                    }
                                    else
                                    {
                                        result = -1;
                                    }
                                }
                                else
                                {
                                    result = -2;
                                }
                            }
                        }
                        else
                        {
                            result = -2;
                        }
                    }
                }
                else
                {
                    result = -1;
                }
                admin.LastLogInAttemptDate = now;
                if (result == 1)
                {
                    admin.LogInAttemptCounter = 0;
                }
                else
                {
                    loginCounter++;
                    admin.LogInAttemptCounter = loginCounter;
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
                DateTime dateDB = dbContext.Admins.SingleOrDefault().LastEditDate;
                if (dateDB != null)
                {
                    if (dateDB == DateTime.MinValue)
                    {
                        controller.Session["ChangePassword"] = true;
                    }
                    else
                    {
                        controller.Session["ChangePassword"] = false;
                    }
                }
                else
                {
                    controller.Session["ChangePassword"] = true;
                }
            }
        }
    }
}