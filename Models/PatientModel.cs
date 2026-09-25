using System;
using System.Collections.Generic;
using System.Text;

namespace AMANC_Inventory.Models
{
    public class PatientModel
    {
        public string ChildName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string CancerType { get; set; } = string.Empty;
        public string Guardians { get; set; } = string.Empty;
        public string Emails { get; set; } = string.Empty;
        public string Phones { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
