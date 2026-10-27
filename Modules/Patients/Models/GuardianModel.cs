using System;
using System.Collections.Generic;
using System.Text;

namespace AMANC_Inventory.Patients.Models
{
    public class GuardianModel
    {
        public string Name { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
