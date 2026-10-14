using AMANC_Inventory.Trips.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMANC_Inventory.Trips.Interfaces
{
    public interface ITripService
    {
        Task<List<TripModel>> GetAllTripsAsync();
        Task<bool> SaveTripAsync(TripModel trip);
    }
}
