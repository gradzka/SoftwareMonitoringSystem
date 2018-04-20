using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                    string deviceJson = JsonConvert.SerializeObject(device);
                    return Json("Success");
                }
            }
            catch
            {
                return Json("Error");
            }
        }

        // GET: DevMGMT/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DevMGMT/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DevMGMT/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DevMGMT/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
