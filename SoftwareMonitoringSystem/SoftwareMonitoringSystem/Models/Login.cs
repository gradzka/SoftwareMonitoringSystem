using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SoftwareMonitoringSystem.Models
{
    public class Login
    {
        public Login() { }
        [Required(ErrorMessage ="To pole jest wymagane!")]
        public string login { get; set; }
        [Required(ErrorMessage ="To pole jest wymagane!")]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }
}