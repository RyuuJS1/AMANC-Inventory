using System;

namespace AMANC_Inventory.Patients.Models
{
    public class PatientModel
    {
        public string Id { get; set; } = string.Empty;
        public string ChildName { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string Curp { get; set; } = string.Empty;
        public string CancerType { get; set; } = string.Empty;
        public string Guardians { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Phones { get; set; } = string.Empty;
        public string Emails { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}