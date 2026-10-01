using System;

namespace AMANC_Inventory.Trips.Models
{
    public class TripModel
    {
        public string Id { get; set; } = string.Empty;
        public string ChildName { get; set; } = string.Empty;
        public string GuardianName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string TransportType { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; } = DateTime.Now;
        public DateTime ArrivalDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}