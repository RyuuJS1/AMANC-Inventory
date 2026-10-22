using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace AMANC_Inventory.Patients.Services
{
    public class PatientService : IPatientService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "Patients";

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
                    // Obtiene el token JSON crudo para procesar Objetos {} o Arreglos [] de Firebase
                    var token = await _firebaseClient
                        .Child(NodeName)
                        .OnceSingleAsync<JToken>();

                    if (token == null || token.Type == JTokenType.Null)
                        return new List<PatientModel>();

                    var patientList = new List<PatientModel>();

                    if (token is JArray jArray)
                    {
                        // Procesa la respuesta cuando Firebase devuelve un Arreglo por usar claves numéricas (1, 2, 3...)
                        foreach (var item in jArray)
                        {
                            if (item != null && item.Type != JTokenType.Null)
                            {
                                var patient = item.ToObject<PatientModel>();
                                if (patient != null)
                                {
                                    patientList.Add(patient);
                                }
                            }
                        }
                    }
                    else if (token is JObject jObject)
                    {
                        // Procesa la respuesta cuando Firebase devuelve un Objeto JSON estándar
                        foreach (var prop in jObject.Properties())
                        {
                            if (prop.Value != null && prop.Value.Type != JTokenType.Null)
                            {
                                var patient = prop.Value.ToObject<PatientModel>();
                                if (patient != null)
                                {
                                    if (string.IsNullOrEmpty(patient.Id)) patient.Id = prop.Name;
                                    patientList.Add(patient);
                                }
                            }
                        }
                    }

                    return patientList;
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

                    // Genera ID numérico consecutivo (1, 2, 3...) si es un nuevo registro
                    if (string.IsNullOrWhiteSpace(patient.Id) || patient.Id == "0" || Guid.TryParse(patient.Id, out _))
                    {
                        patient.Id = await GetNextCustomIdAsync();
                    }

                    // Guardado en Realtime Database con clave numérico simple
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
        /// Genera el siguiente ID consecutivo numérico simple (ej. 1, 2, 3...).
        /// </summary>
        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var patients = await GetAllPatientsAsync();

                if (!patients.Any()) return "1";

                int maxId = patients
                    .Select(p =>
                    {
                        string idToParse = !string.IsNullOrWhiteSpace(p.Id) ? p.Id : "0";
                        string numericPart = Regex.Match(idToParse, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return (maxId + 1).ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al calcular el consecutivo del paciente]: {ex.Message}");
                return "1";
            }
        }
    }
}