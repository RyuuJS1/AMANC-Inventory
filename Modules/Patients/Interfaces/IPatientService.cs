using AMANC_Inventory.Patients.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMANC_Inventory.Patients.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientModel>> GetAllPatientsAsync();
        Task<bool> SavePatientAsync(PatientModel patient);
    }
}
