using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
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
            using (var dbContext = new SMSDBContext())
            {
                Login loginDataDB = dbContext.Admins.Select(x => new Login { login = x.Username, password = x.Password }).FirstOrDefault();
                if (loginDataDB != null)
                {
                    byte[] bytePasswd = Encoding.Default.GetBytes(loginData.password);
                    using (var sha512 = SHA512.Create())
                    {
                        byte[] hashBytePasswd = sha512.ComputeHash(bytePasswd); //512-bits
                        string hashBytePasswdHex = BitConverter.ToString(hashBytePasswd).Replace("-", string.Empty);


                        if (loginData.login == loginDataDB.login && hashBytePasswdHex == loginDataDB.password)
                        {
                            return true;                     }
                    }
                }
            }
            return false;
        }
    }
}