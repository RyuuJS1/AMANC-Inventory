using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Patients.Models;
using Firebase.Database;

namespace AMANC_Inventory.Patients.Services
{
    public interface IPatientService
    {
        Task<List<PatientModel>> GetAllPatientsAsync();
    }

    public class PatientService : IPatientService
    {
        private readonly FirebaseClient _firebaseClient = new(FirebaseConfig.RealtimeDbUrl);

        public async Task<List<PatientModel>> GetAllPatientsAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    var items = await _firebaseClient
                        .Child("Patients")
                        .OnceAsync<PatientModel>();

                    return items.Select(item =>
                    {
                        var patient = item.Object;
                        patient.Id = string.IsNullOrEmpty(patient.Id) ? item.Key : patient.Id;
                        return patient;
                    }).ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Error al consultar pacientes]: {ex.Message}");
                    return new List<PatientModel>();
                }
            }, "Consultar Pacientes desde Firebase");
        }
    }
}