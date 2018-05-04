using SoftwareMonitoringSystem.Infrastructure.Abstract;
using SoftwareMonitoringSystem.Infrastructure.Concrete;
using SoftwareMonitoringSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;

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
            if (DeviceID > 0 && MACAddress != "" && Manufacturer != "" && IPAddress != "")
            {
                Regex MACAddr = new Regex(@"^[a-fA-F0-9-]{17}|[a-fA-F0-9:]{17}$");
                Regex IPv4Addr = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(:[0-9]{1,5})?$");
                if (MACAddr.IsMatch(MACAddress))
                {
                    using (var dbContext = new SMSDBContext())
                    {
                        if (dbContext.Devices.Count(x => x.MACAddress.Equals(MACAddress) && x.DeviceID!=DeviceID) > 0)
                        {
                            return Json("Wpisz inny adres MAC (podany jest zajęty)");
                        }
                        else
                        {
                            if (IPv4Addr.IsMatch(IPAddress))
                            {
                                if (dbContext.Devices.Count(x => x.IPAddress.Equals(IPAddress) && x.DeviceID!=DeviceID) > 0)
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
                                    return Json("Success");
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
                        else
                        {
                            return Json("Błąd wewnętrzny systemu");
                        }
                    }
                    dbContext.SaveChanges();
                    return Json("Success");
                }
            }
            catch
            {
                return Json("Błąd wewnętrzny systemu");
            }
        }
        [HttpGet]
        public ActionResult ScanHistory()
        {
            authProvider.CheckDefaultPassword(this);
            //dictionary: KEY: DateTime, Value: D_S_IDDescStatus
            Dictionary<DateTime, List<D_S_IDDescStatus>> dict = new Dictionary<DateTime, List<D_S_IDDescStatus>>();
            using (var context = new SMSDBContext())
            {
                List<Scan> scans = context.Scans.OrderByDescending(x=>x.ScanDateTime).ToList();
                if (scans.Count>0)
                {
                    List<ScanAndDevice> scansAndDevices = context.ScansAndDevices.ToList();
                    if (scansAndDevices.Count>0)
                    {
                        var devices = context.Devices.Select(x=>new {x.DeviceID, x.Description, x.IPAddress, x.MACAddress }).ToList();
                        if (devices.Count>0)
                        {
                            foreach (var scan in scans)
                            {                              
                                var D_IDStatuses = scansAndDevices.Where(x => x.ScanID == scan.ScanID).Select(x=> new { x.DeviceID, x.IsSuccessful });
                                if (D_IDStatuses != null)
                                {
                                    foreach (var D_IDStatus in D_IDStatuses)
                                    {
                                        var device = devices.Where(x => x.DeviceID == D_IDStatus.DeviceID).FirstOrDefault();
                                        if (device != null)
                                        {
                                            D_S_IDDescStatus D_S_IDDescStatus = new D_S_IDDescStatus();
                                            D_S_IDDescStatus.DeviceID = device.DeviceID;
                                            D_S_IDDescStatus.ScanID = scan.ScanID;
                                            string deviceDescription = device.Description;
                                            string toDevDesc = "(" + device.MACAddress + ", " + device.IPAddress + ")";
                                            if (deviceDescription == "")
                                            {
                                                D_S_IDDescStatus.Description = toDevDesc;
                                            }
                                            else
                                            {
                                                D_S_IDDescStatus.Description = deviceDescription + " " + toDevDesc;
                                            }
                                            if (D_IDStatus.IsSuccessful == 0)
                                            {
                                                D_S_IDDescStatus.Status = "Niepowodzenie";
                                            }
                                            else
                                            {
                                                D_S_IDDescStatus.Status = "Sukces";
                                            }

                                            if (!dict.ContainsKey(scan.ScanDateTime))//key exists
                                            {
                                                dict.Add(scan.ScanDateTime, new List<D_S_IDDescStatus>());
                                            }
                                            dict[scan.ScanDateTime].Add(D_S_IDDescStatus);
                                        }
                                    }
                                }
                                else
                                {
                                    //brak urzadzenia o podanym id
                                }
                            }                         
                        }
                        else
                        {
                            //lista urzadzen jest pusta
                        }
                    }
                    else
                    {
                        //lista scansAndDevices jest pusta
                    }
                }
                else
                {
                    //lista skanowan jest pusta
                }
            }
            return View(dict);
        }

        [HttpPost]
        public ActionResult Scan(List<int> IDs)
        {
            //TODO
            return Json("Success");
        }
        [HttpGet]
        public ActionResult DevHistory(int DeviceID)
        {
            authProvider.CheckDefaultPassword(this);
            List<D_S_IDDateStatus> d_S_IDDateStatuses = new List<D_S_IDDateStatus>();
            using (var context = new SMSDBContext())
            {
                Device device = context.Devices.Where(x => x.DeviceID == DeviceID).FirstOrDefault();
                if (device != null)
                {
                    string description = device.Description;
                    string toDevDesc = "(" + device.MACAddress + ", " + device.IPAddress + ")";
                    if (description == "")
                    {
                        TempData["DevHistoryDesc"] = toDevDesc;
                    }
                    else
                    {
                        TempData["DevHistoryDesc"] = description + " " + toDevDesc;
                    }
                    var scansAndDevices = context.ScansAndDevices.Where(x => x.DeviceID == DeviceID).Select(x => new { x.ScanID, x.IsSuccessful });
                    if (scansAndDevices != null)
                    {
                        List<Scan> scans = context.Scans.ToList();
                        if (scans.Count > 0)
                        {
                            foreach (var scanAndDevice in scansAndDevices)
                            {
                                D_S_IDDateStatus d_S_IDDateStatus = new D_S_IDDateStatus();
                                d_S_IDDateStatus.DeviceID = DeviceID;
                                d_S_IDDateStatus.ScanID = scanAndDevice.ScanID;
                                if (scanAndDevice.IsSuccessful == 0)
                                {
                                    d_S_IDDateStatus.Status = "Niepowodzenie";
                                }
                                else
                                {
                                    d_S_IDDateStatus.Status = "Sukces";
                                }
                                DateTime dateTime = scans.Where(x => x.ScanID == scanAndDevice.ScanID).Select(x => x.ScanDateTime).FirstOrDefault();
                                if (dateTime != null)
                                {
                                    d_S_IDDateStatus.DateTime = dateTime;
                                }
                                else
                                {
                                    d_S_IDDateStatus.DateTime = DateTime.MinValue;
                                }
                                d_S_IDDateStatuses.Add(d_S_IDDateStatus);
                            }
                        }
                        else
                        {
                            //lista scans jest pusta
                        }
                    }
                    else
                    {
                        //lista scansAndDevices jest pusta
                    }
                }
            }
            d_S_IDDateStatuses = d_S_IDDateStatuses.OrderByDescending(x => x.DateTime).ToList();
            return View(d_S_IDDateStatuses);
        }
        [HttpGet]
        public ActionResult DevScanDetails()
        {
            authProvider.CheckDefaultPassword(this);
            return View();
        }
        [HttpPost]
        public ActionResult DeleteScan(int ScanID, int DeviceID)
        {
            try
            {
                using (var context = new SMSDBContext())
                {
                    int number = context.ScansAndDevices.Count(x => x.ScanID == ScanID);
                    ScanAndDevice scanAndDevice = context.ScansAndDevices.Where(x => x.ScanID == ScanID && x.DeviceID == DeviceID).FirstOrDefault();
                    if (scanAndDevice!=null)
                    {
                        context.ScansAndDevices.Remove(scanAndDevice);
                        context.SaveChanges();
                    }
                    else
                    {
                        return Json("Błąd wewnętrzny systemu");
                    }
                    if (number == 1)
                    {
                        Scan scan = context.Scans.Where(x => x.ScanID == ScanID).FirstOrDefault();
                        if (scan != null)
                        {
                            context.Scans.Remove(scan);
                            context.SaveChanges();
                        }
                        else
                        {
                            return Json("Błąd wewnętrzny systemu");
                        }
                    }

                }
                return Json("Success");
            }
            catch
            {
                return Json("Błąd wewnętrzny systemu");
            }
        }
    }
}
