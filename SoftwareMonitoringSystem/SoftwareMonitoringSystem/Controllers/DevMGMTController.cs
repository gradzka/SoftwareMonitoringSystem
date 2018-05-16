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
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace SoftwareMonitoringSystem.Controllers
{
    // Require authorization for all actions on the controller.
    [Authorize]
    public class DevMGMTController : Controller
    {
        IAuthProvider authProvider;
        public DevMGMTController()
        {
            authProvider = new FormsAuthProvider();
        }
        // GET: DevMGMT.
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

        // GET: DevMGMT/Details/5.
        public ActionResult Details(int id)
        {
            authProvider.CheckDefaultPassword(this);
            return View();
        }


        // POST: DevMGMT/Create.
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
                                    dev.IsActive = 1;
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

        // POST: DevMGMT/Delete/5.
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
            // Dictionary: KEY: DateTime, Value: D_S_IDDescStatus.
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

                                            if (!dict.ContainsKey(scan.ScanDateTime))// Key exists.
                                            {
                                                dict.Add(scan.ScanDateTime, new List<D_S_IDDescStatus>());
                                            }
                                            dict[scan.ScanDateTime].Add(D_S_IDDescStatus);
                                        }
                                    }
                                }
                                else
                                {
                                    // No devices with this ID.
                                }
                            }                         
                        }
                        else
                        {
                            // Empty devices ID.
                        }
                    }
                    else
                    {
                        // ScansAndDevices list is empty.
                    }
                }
                else
                {
                    // Scans list is empty.
                }
            }
            return View(dict);
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
                            // Scans list is empty.
                        }
                    }
                    else
                    {
                        // ScansAndDevices list is empty.
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
        // https://stackoverflow.com/a/24814027
        private static UnicastIPAddressInformation[] GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            List<UnicastIPAddressInformation> ipList = new List<UnicastIPAddressInformation>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipList.Add(ip);
                        }
                    }
                }
            }
            return ipList.ToArray();
        }

        private IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            if (address != null && subnetMask != null)
            {
                byte[] ipAdressBytes = address.GetAddressBytes();
                byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

                if (ipAdressBytes.Length != subnetMaskBytes.Length)
                    throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

                byte[] broadcastAddress = new byte[ipAdressBytes.Length];
                for (int i = 0; i < broadcastAddress.Length; i++)
                {
                    broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
                }
                return new IPAddress(broadcastAddress);
            }
            else
            {
                return null;
            }
        }

        private string GetNextIPAddress(string ipAddress)
        {
            // https://stackoverflow.com/a/36083042
            byte[] addressBytes = IPAddress.Parse(ipAddress).GetAddressBytes().Reverse().ToArray();
            uint ipAsUint = BitConverter.ToUInt32(addressBytes, 0);
            var nextAddress = BitConverter.GetBytes(++ipAsUint);
            return String.Join(".", nextAddress.Reverse());
        }
        private List<string> GetRangeOfIPAddress(string startIPAddress, int howManyAddresses)
        {
            List<string> ipAddresses = new List<string>();
            for (int i = 0; i < howManyAddresses; i++)
            {
                ipAddresses.Add(startIPAddress);
                startIPAddress = GetNextIPAddress(startIPAddress); 
            }
            return ipAddresses;
        }
        private async void CheckAvailability(List<string> rangeOfIPAddresses, int startIndex, int stopIndex, Dictionary<string, JObject> IPAddressAndDevice)
        {
            if (rangeOfIPAddresses != null && IPAddressAndDevice != null)
            {
                HttpClient client = new HttpClient();
                client.Timeout = new System.TimeSpan(0, 0, 1);
                for (int i = stopIndex - 1; i >= startIndex; i--)
                {
                    try
                    {
                        string URL = "http://" + rangeOfIPAddresses[i] + ":11050/available";
                        HttpResponseMessage response = client.GetAsync(URL).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            JObject parsedContent = JObject.Parse(content.ToString());
                            IPAddressAndDevice.Add(rangeOfIPAddresses[i], parsedContent);
                        }
                        else
                        {
                            rangeOfIPAddresses[i] = "";
                        }
                    }
                    catch (Exception e)
                    {
                        rangeOfIPAddresses[i] = "";
                    }
                }
                client.Dispose();
            }
        }
        private async void CheckAvailability(List<Device> devices, int startIndex, int stopIndex)
        {
            HttpClient client = new HttpClient();
            client.Timeout = new System.TimeSpan(0, 0, 1);
            using (var context = new SMSDBContext())
            {
                var devicesDB = context.Devices.Where(x => x.IsActive == 1).ToList();
                for (int i = startIndex; i < stopIndex; i++)
                {
                    try
                    {
                        string URL = "http://" + devices[i].IPAddress + ":11050/available";
                        HttpResponseMessage response = client.GetAsync(URL).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            // JObject parsedContent = JObject.Parse(content.ToString());
                        }
                        else
                        {
                            var dev = devicesDB.Single(x => x.IPAddress == devices[i].IPAddress);
                            if (dev!=null)
                            {
                                dev.IsActive = 0;
                                dev.LastEditDate = DateTime.Now;
                                context.Entry(dev).State = EntityState.Modified;
                            }
                            devices[i].IsActive = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        var dev = devicesDB.Single(x => x.IPAddress == devices[i].IPAddress);
                        if (dev != null)
                        {
                            dev.IsActive = 0;
                            dev.LastEditDate = DateTime.Now;
                            context.Entry(dev).State = EntityState.Modified;
                        }
                        devices[i].IsActive = 0;
                    }
                }
                context.SaveChanges();
            }
            client.Dispose();
        }
        [HttpPost]
        public ActionResult SearchDevices()
        {
            authProvider.CheckDefaultPassword(this);

            UnicastIPAddressInformation unicastIPAddressInformationEthernet = GetAllLocalIPv4(NetworkInterfaceType.Ethernet).FirstOrDefault();
            IPAddress networkAddressEthernet;
            networkAddressEthernet = unicastIPAddressInformationEthernet != null ? GetNetworkAddress(unicastIPAddressInformationEthernet.Address, unicastIPAddressInformationEthernet.IPv4Mask) : null;
            List<string> rangeOfIPAddresses = new List<string>();
            if (networkAddressEthernet != null)
            {
                int networkDevicesNoE = -1;
                networkDevicesNoE = Convert.ToInt32(Math.Pow(2, 32 - unicastIPAddressInformationEthernet.PrefixLength)) - 2;
                rangeOfIPAddresses = GetRangeOfIPAddress(GetNextIPAddress(networkAddressEthernet.ToString()), networkDevicesNoE);
            }
            else
            {
                UnicastIPAddressInformation unicastIPAddressInformationWireless = GetAllLocalIPv4(NetworkInterfaceType.Wireless80211).FirstOrDefault();
                IPAddress networkAddressWireless;
                networkAddressWireless = unicastIPAddressInformationWireless != null ? GetNetworkAddress(unicastIPAddressInformationWireless.Address, unicastIPAddressInformationWireless.IPv4Mask) : null;
                if (networkAddressWireless != null)
                {
                    int networkDevicesNoW = -1;
                    networkDevicesNoW = Convert.ToInt32(Math.Pow(2, 32 - unicastIPAddressInformationWireless.PrefixLength)) - 2;
                    rangeOfIPAddresses = GetRangeOfIPAddress(GetNextIPAddress(networkAddressWireless.ToString()), networkDevicesNoW);
                }
            }
            if (rangeOfIPAddresses.Count > 0)
            {
                List<Thread> threads = new List<Thread>();
                Dictionary<string, JObject> IPAddressAndDevice = new Dictionary<string, JObject>();
                int threadNo = 8;
                for (int i = 0; i < threadNo; i++)
                {
                    int myFirst = 0;
                    int myLast = 0;
                    myFirst = ((i * rangeOfIPAddresses.Count) / threadNo);
                    myLast = (((i + 1) * rangeOfIPAddresses.Count) / threadNo);
                    threads.Add(new Thread(() => CheckAvailability(rangeOfIPAddresses, myFirst, myLast, IPAddressAndDevice)));
                    threads[i].Name = "IPA_" + i;
                    threads[i].Start();
                }
                for (int i = 0; i < threadNo; i++)
                {
                    threads[i].Join();
                }
                rangeOfIPAddresses.RemoveAll(x => x == "");

                if (IPAddressAndDevice.Count() == rangeOfIPAddresses.Count())// Devices from local net.
                {
                    using (var context = new SMSDBContext())
                    {
                        var devices = context.Devices; // Devices from DB.
                        foreach (var pair in IPAddressAndDevice)// Iterate throw dictionary with <Key: addressIP, Value: json>.
                        {
                            string macTMP = pair.Value["mac"].ToString();
                            var device = devices.Where(x=> x.MACAddress == macTMP).ToList();
                            if (device.Count()==1) // Device is in DB.
                            {
                                if (device[0].IPAddress == pair.Value["ipAddress"].ToString())
                                {
                                    // Set IsActive to 1.
                                    device[0].IsActive = 1;
                                    device[0].LastEditDate = DateTime.Now;
                                    context.Entry(device[0]).State = EntityState.Modified;
                                    
                                }
                                else
                                {
                                    // Check other -> if they ip addr is duplicate set IsActive to 0.
                                    foreach (var dev in devices)
                                    {
                                        if (dev.IPAddress == pair.Value["ipAddress"].ToString())
                                        {
                                            dev.IsActive = 0;
                                            dev.LastEditDate = DateTime.Now;
                                            context.Entry(dev).State = EntityState.Modified;
                                        }
                                    }
                                    // Set IPAddr to pair.Value["ipAddress"].
                                    // Set IsActive to 1.
                                    device[0].IPAddress = pair.Value["ipAddress"].ToString();
                                    device[0].IsActive = 1;
                                    device[0].LastEditDate = DateTime.Now;
                                    context.Entry(device[0]).State = EntityState.Modified;

                                }
                            }
                            else if (device.Count() == 0)
                            {
                                // Check other -> if they ip addr is duplicate set IsActive to 0.
                                foreach (var dev in devices)
                                {
                                    if (dev.IPAddress == pair.Value["ipAddress"].ToString())
                                    {
                                        dev.IsActive = 0;
                                        dev.LastEditDate = DateTime.Now;
                                        context.Entry(dev).State = EntityState.Modified;
                                    }
                                }
                                // Set IPAddr to pair.Value["ipAddress"]
                                Device newDevice = new Device();
                                // Set IsActive to 1.
                                newDevice.IPAddress = pair.Value["ipAddress"].ToString();
                                newDevice.MACAddress = pair.Value["mac"].ToString();
                                newDevice.Manufacturer = "XYZ";
                                context.Devices.Add(newDevice);
                            }
                            else
                            {
                                return Json("Error");
                            }
                        }
                        context.SaveChanges();
                    }
                }
            }
            return Json("Success");
        }
        public async void ScanInstalledSoftware(List<Device> devices, int startIndex, int stopIndex, int scanID)
        {
            if (devices != null)
            {
                HttpClient client = new HttpClient();
                client.Timeout = new System.TimeSpan(0, 2, 0);
                using (var context = new SMSDBContext())
                {
                    for (int i = startIndex; i < stopIndex; i++)
                    {
                        // Save information about scan to DB - basic info.
                        ScanAndDevice scanAndDevice = new ScanAndDevice();
                        scanAndDevice.ScanID = scanID;
                        scanAndDevice.DeviceID = devices[i].DeviceID;
                        try
                        {
                            string URL = "http://" + devices[i].IPAddress + ":11050/installedSoftware";
                            HttpResponseMessage response = client.GetAsync(URL).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                var content = await response.Content.ReadAsStringAsync();
                                JObject parsedContent = JObject.Parse(content.ToString());
                                // Save information about scan to DB - is successful = true.
                                string pathToFile = System.AppDomain.CurrentDomain.BaseDirectory + "bin\\Scans\\" + devices[i].MACAddress;
                                System.IO.Directory.CreateDirectory(pathToFile);
                                string date = DateTime.Now.ToString().Replace(':','-');
                                pathToFile = pathToFile + "\\" + date + ".json";
                                scanAndDevice.Path = pathToFile;
                                scanAndDevice.IsSuccessful = 1;

                                // Save result into /folder/file.
                                System.IO.File.WriteAllText(pathToFile, content);
                            }
                            else
                            {
                                // Save information about scan to DB - is successful = false.
                                scanAndDevice.Path = "";
                                scanAndDevice.IsSuccessful = 0;
                            }
                        }
                        catch (Exception e)
                        {
                            // Save information about scan to DB - is successful = false.
                            scanAndDevice.Path = "//";
                            scanAndDevice.IsSuccessful = 0;
                        }
                        finally
                        {
                            context.ScansAndDevices.Add(scanAndDevice);
                        }
                    }
                    context.SaveChanges();
                }
                client.Dispose();
            }
        }
        [HttpPost]
        public ActionResult Scan(List <int> DeviceIDs)
        {
            if (DeviceIDs != null)
            {
                if (DeviceIDs.Count() > 0)
                {
                    // Check if device is active.
                    using (var context = new SMSDBContext())
                    {
                        System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "bin\\Scans");
                        // Only devices with IDs from DeviceIDs.
                        var devices = context.Devices.Where(x => DeviceIDs.Contains(x.DeviceID)).ToList();
                        // Remove devices which are inactive.
                        devices.RemoveAll(x => x.IsActive == 0);
                        if (devices.Count() > 0)
                        {
                            List<Thread> threadsA = new List<Thread>();
                            int threadNo = 8;
                            // Check Availability => remove devices which dont't answer.
                            for (int i = 0; i < threadNo; i++)
                            {
                                int myFirst = 0;
                                int myLast = 0;
                                myFirst = ((i * devices.Count) / threadNo);
                                myLast = (((i + 1) * devices.Count) / threadNo);
                                threadsA.Add(new Thread(() => CheckAvailability(devices, myFirst, myLast)));
                                threadsA[i].Name = "IPA_A" + i;
                                threadsA[i].Start();
                            }
                            for (int i = 0; i < threadNo; i++)
                            {
                                threadsA[i].Join();
                            }
                            // Remove devices which are inactive.
                            devices.RemoveAll(x => x.IsActive == 0);
                            if (devices.Count() > 0)
                            {
                                List<Thread> threadsS = new List<Thread>();
                                // Scan devices in threads.
                                Scan scan = new Scan();
                                context.Scans.Add(scan);
                                context.SaveChanges();
                                for (int i = 0; i < threadNo; i++)
                                {
                                    int myFirst = 0;
                                    int myLast = 0;
                                    myFirst = ((i * devices.Count) / threadNo);
                                    myLast = (((i + 1) * devices.Count) / threadNo);
                                    // Threads scan special range.
                                    threadsS.Add(new Thread(() => ScanInstalledSoftware(devices, myFirst, myLast, scan.ScanID)));
                                    threadsS[i].Name = "IPA_S" + i;
                                    threadsS[i].Start();
                                }
                                for (int i = 0; i < threadNo; i++)
                                {
                                    threadsS[i].Join();
                                }
                            }
                        }
                    }
                }
            }
            return Json("Success");
        }
    }
}
