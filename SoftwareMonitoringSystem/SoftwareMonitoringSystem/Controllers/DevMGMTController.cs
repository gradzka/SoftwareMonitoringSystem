using SoftwareMonitoringSystem.Infrastructure.Abstract;
using SoftwareMonitoringSystem.Infrastructure.Concrete;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace SoftwareMonitoringSystem.Controllers
{
    //equire authorization for all actions on the controller
    [Authorize]
    public class DevMGMTController : Controller
    {
        IAuthProvider authProvider;
        public DevMGMTController()
        {
            authProvider = new FormsAuthProvider();
        }
        // GET: DevMGMT
        public ActionResult GetDevices()
        {
            authProvider.CheckDefaultPassword(this);
            List<Device> devices = null;
            using (var dbContext = new SMSDBContext())
            {
                devices = dbContext.Devices.ToList();
                devices.Reverse();
            }
            //lista urzadzen
            return View("DevManagement", devices);
        }

        // GET: DevMGMT/Details/5
        public ActionResult Details(int id)
        {
            authProvider.CheckDefaultPassword(this);
            return View();
        }


        // POST: DevMGMT/Create
        [HttpPost]
        public JsonResult AddDevice(string MACAddress, string Manufacturer, string IPAddress, string Description)
        {
            try
            {
                if (Manufacturer == "")
                {
                    return Json("Uzupełnij pole Producent");
                }
                using (var dbContext = new SMSDBContext())
                {
                    Regex MACAddr = new Regex(@"^[a-fA-F0-9-]{17}|[a-fA-F0-9:]{17}$");
                    if (MACAddr.IsMatch(MACAddress))
                    {
                        if (dbContext.Devices.Count(x => x.MACAddress.Equals(MACAddress)) > 0)
                        {
                            return Json("Wpisz inny adres MAC (podany jest zajęty)");
                        }
                    }
                    else
                    {
                        return Json("Wpisz poprawny adres MAC");
                    }
                    Regex IPv4Addr = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(:[0-9]{1,5})?$");
                    if (IPv4Addr.IsMatch(IPAddress))
                    {
                        if (dbContext.Devices.Count(x => x.IPAddress.Equals(IPAddress)) > 0)
                        {
                            return Json("Wpisz inny adres IP (podany jest zajęty)");
                        }
                    }
                    else
                    {
                        return Json("Wpisz poprawny adres IP");
                    }

                    Device device = new Device();
                    device.MACAddress = MACAddress;
                    device.Manufacturer = Manufacturer;
                    device.IPAddress = IPAddress;
                    device.Description = Description;
                    dbContext.Devices.Add(device);
                    dbContext.SaveChanges();
                    return Json(device);
                }
            }
            catch
            {
                return Json("Spróbuj ponownie (błąd wewnętrzny aplikacji)");
            }
        }

        [HttpPost]
        public ActionResult Edit(int DeviceID, string MACAddress, string Manufacturer, string IPAddress, string Description)
        {
            if (DeviceID < 0 && MACAddress != "" && Manufacturer != "" && IPAddress != "" && Description != "")
            {
                Regex MACAddr = new Regex(@"^[a-fA-F0-9-]{17}|[a-fA-F0-9:]{17}$");
                Regex IPv4Addr = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(:[0-9]{1,5})?$");
                if (MACAddr.IsMatch(MACAddress))
                {
                    using (var dbContext = new SMSDBContext())
                    {
                        if (dbContext.Devices.Count(x => x.IPAddress.Equals(IPAddress)) > 0)
                        {
                            return Json("Wpisz inny adres MAC (podany jest zajęty)");
                        }
                        else
                        {
                            if (IPv4Addr.IsMatch(IPAddress))
                            {
                                if (dbContext.Devices.Count(x => x.IPAddress.Equals(IPAddress)) > 0)
                                {
                                    return Json("Wpisz inny adres IP (podany jest zajęty)");
                                }
                                else
                                {
                                    Device dev = dbContext.Devices.SingleOrDefault(x => x.DeviceID.Equals(DeviceID));
                                    dev.MACAddress = MACAddress;
                                    dev.Manufacturer = Manufacturer;
                                    dev.IPAddress = IPAddress;
                                    dev.Description = Description;
                                    dbContext.Entry(dev).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                    return Json("Urządzenie o ID: " + DeviceID + " zostało zmodyfikowane");
                                }
                            }
                            else
                            {
                                return Json("Wpisz prawidłowy adres IP");
                            }
                        }
                    }
                }
                else
                {
                    return Json("Wpisz prawidłowy adres MAC");
                }

            }
            else
            {
                return Json("Spróbuj ponownie (błąd wewnętrzny aplikacji)");
            }
        }

        // POST: DevMGMT/Delete/5
        [HttpPost]
        public ActionResult Delete(List<int> IDs)
        {
            try
            {
                using (var dbContext = new SMSDBContext())
                {
                    Device device;
                    foreach (var item in IDs)
                    {
                        device = dbContext.Devices.SingleOrDefault(dev => dev.DeviceID == item);
                        if (device != null)
                        {
                            dbContext.Devices.Remove(device);
                        }
                    }
                    dbContext.SaveChanges();
                    return Json("Success");
                }
            }
            catch
            {
                return Json("Error");
            }
        }
        [HttpGet]
        public ActionResult ScanHistory()
        {
            authProvider.CheckDefaultPassword(this);
            return View();
        }

        [HttpPost]
        public ActionResult Scan(List<int> IDs)
        {
            //TODO
            return Json("Success");
        }

    }
}
