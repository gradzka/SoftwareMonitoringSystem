using SoftwareMonitoringSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SoftwareMonitoringSystem.Infrastructure.Abstract
{
    public interface IAuthProvider
    {
        bool Authenticate(Login loginData);
        void CheckDefaultPassword(Controller controller);
    }
}
