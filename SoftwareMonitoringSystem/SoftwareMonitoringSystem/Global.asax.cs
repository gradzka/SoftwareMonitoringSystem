using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SoftwareMonitoringSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //| PathToDB |\SMSDB.sqlite : https://stackoverflow.com/a/6941582
            string startupPath = System.AppDomain.CurrentDomain.BaseDirectory + "bin\\";
            AppDomain.CurrentDomain.SetData("DataDirectory", startupPath);
            //Add basic account -> must be change after first use
            using (var dbContext = new SMSDBContext())
            {
                if (dbContext.Admins.Count()==0)
                {
                    Admin admin = new Admin();
                    admin.Username = "admin";
                    admin.Password = "9CE1EB62332A144B0A752460F9E789B2E4A6D7403D2E18041C4E80352DB736C51FD247301E079CEF9EDE13DFDCF3D040A3F0843E4D92073FDEA29F5838C421F3";
                    dbContext.Admins.Add(admin);
                    dbContext.SaveChanges();
                }  
            }

        }
    }
}
