using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Patients.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace AMANC_Inventory.Patients.Services
{
    public interface IPatientService
    {
        Task<List<PatientModel>> GetAllPatientsAsync();
        Task<bool> SavePatientAsync(PatientModel patient);
    }

    public class PatientService : IPatientService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "Patients";
        private const string IdPrefix = "PAC-";

        public PatientService()
        {
            _firebaseClient = new FirebaseClient(FirebaseConfig.RealtimeDbUrl);
        }

        public async Task<List<PatientModel>> GetAllPatientsAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    var items = await _firebaseClient
                        .Child(NodeName)
                        .OnceAsync<PatientModel>();

                    return items
                        .Where(item => item.Object != null)
                        .Select(item =>
                        {
                            var patient = item.Object;
                            patient.Id = string.IsNullOrEmpty(patient.Id) ? item.Key : patient.Id;
                            return patient;
                        })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al consultar pacientes]: {ex.Message}");
                    return new List<PatientModel>();
                }
            }, "Consultar Pacientes desde Firebase");
        }

        public async Task<bool> SavePatientAsync(PatientModel patient)
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    if (patient == null) return false;

                    // Si es un nuevo paciente o no tiene un ID asignado, se genera el consecutivo
                    if (string.IsNullOrWhiteSpace(patient.Id) || patient.Id == "0")
                    {
                        patient.Id = await GetNextCustomIdAsync();
                    }

                    // Guardado mediante PutAsync para usar la clave personalizada (PAC-000001)
                    await _firebaseClient
                        .Child(NodeName)
                        .Child(patient.Id)
                        .PutAsync(patient);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al guardar paciente]: {ex.Message}");
                    return false;
                }
            }, "Guardar Paciente en Firebase");
        }

        /// <summary>
        /// Genera el siguiente ID consecutivo dinámico (ej. PAC-000001).
        /// Admite más de 999,999 registros ya que se expande automáticamente si el número crece.
        /// </summary>
        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var items = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<PatientModel>();

                if (!items.Any()) return $"{IdPrefix}000001";

                int maxId = items
                    .Select(i =>
                    {
                        string key = i.Key;
                        // Extrae únicamente los dígitos de la clave
                        string numericPart = Regex.Match(key, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                int nextNumericId = maxId + 1;

                // :D6 garantiza al menos 6 dígitos con ceros a la izquierda.
                return $"{IdPrefix}{nextNumericId:D6}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al calcular el consecutivo del paciente]: {ex.Message}");
                return $"{IdPrefix}000001";
            }
        }
    }
}