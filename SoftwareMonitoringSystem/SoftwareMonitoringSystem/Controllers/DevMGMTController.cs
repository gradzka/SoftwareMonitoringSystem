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
        // GET: DevMGMT
        public ActionResult GetDevices()
        {
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
                    return Json("Manufacturer is empty");
                }
                using (var dbContext = new SMSDBContext())
                {                 
                    Regex MACAddr = new Regex(@"^[a-fA-F0-9-]{17}|[a-fA-F0-9:]{17}$");
                    if (MACAddr.IsMatch(MACAddress))
                    {
                        if (dbContext.Devices.Count(x=>x.MACAddress.Equals(MACAddress)) >0)
                        {
                            return Json("Occupied MAC address");
                        }
                    }
                    else
                    {
                        return Json("Inproper MAC address");
                    }
                    Regex IPv4Addr = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(:[0-9]{1,5})?$");
                    if (IPv4Addr.IsMatch(IPAddress))
                    {
                        if (dbContext.Devices.Count(x => x.IPAddress.Equals(IPAddress)) > 0)
                        {
                            return Json("Occupied IP address");
                        }
                    }
                    else
                    {
                        return Json("Inproper IP address");
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
                return Json("Error");
            }
        }

        [HttpGet]
        public ActionResult Edit(int DeviceID)
        {
            using (var dbContext = new SMSDBContext())
            {
                Device device = dbContext.Devices.SingleOrDefault(x => x.DeviceID.Equals(DeviceID));
                if (device != null)
                {
                    return View(device);
                }
                else
                {
                    return RedirectToAction("GetDevices", "DevMGMT");
                }
            }
        }

        [HttpPost]
        public ActionResult Edit(Device device)
        {
            if (device != null)
            {
                using (var dbContext = new SMSDBContext())
                {
                    Device dev = dbContext.Devices.SingleOrDefault(x => x.DeviceID.Equals(device.DeviceID));
                    dev.MACAddress = device.MACAddress;
                    dev.Manufacturer = device.Manufacturer;
                    dev.IPAddress = device.IPAddress;
                    dev.Description = device.Description;
                    dbContext.Entry(dev).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    return Json("Device with ID: " + device.DeviceID + " has been modified");
                }
            }
            else
            {
                return Json("Error");
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
