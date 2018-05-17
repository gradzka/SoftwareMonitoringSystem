using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoftwareMonitoringSystem.Models
{
    public class Software
    {
        public Software() { }
        public string Name { get; set; }
        public string Version { get; set; }
        public string LocationPath { get; set; }
        public string Publisher { get; set; }
        public string InstallDate { get; set; }
        public string Description { get; set; }
    }

    public class DevScanDetails
    {
        public DevScanDetails() { }
        public string IPAddress { get; set; }
        public string Mac { get; set; }
        public int FoundSoftware { get; set; }
        public int RecognizedSoftware { get; set; }
        public int UnknownSoftware { get; set; }
        public string Manufacturer { get; set; }
        public string System { get; set; }
        public List<Software> Software { get; set; }
    }
}