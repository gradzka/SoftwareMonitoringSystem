using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoftwareMonitoringSystem.Models
{
    public class D_S_IDDateStatus
    {
        public D_S_IDDateStatus() { }
        public int DeviceID { get; set; }
        public int ScanID { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
    }
}