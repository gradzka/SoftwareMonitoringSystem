using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoftwareMonitoringSystem.Models
{
    public class D_S_IDDescStatus
    {
        public D_S_IDDescStatus() { }
        public int DeviceID { get; set; }
        public int ScanID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}