using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SoftwareMonitoringSystem.Controllers
{
    public class DevMGMTController : Controller
    {
        // GET: DevMGMT
        public ActionResult GetDevices()
        {
            List<Device> devices = null;
            using (var dbContext = new SMSDBContext())
            {
                Device device = new Device();
                device.MACAddress = "12-a2-12-a3-12-a2";
                device.Manufacturer = "Lenovo";
                device.IPAddress = "192.168.9.54";
                device.Description = "Work device";
                dbContext.Devices.Add(device);
                dbContext.SaveChanges();
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

        // GET: DevMGMT/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DevMGMT/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
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
