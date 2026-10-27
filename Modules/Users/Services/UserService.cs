using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Modules.Auth.Interfaces;
using AMANC_Inventory.Modules.Auth.Services;
using AMANC_Inventory.Users.Interfaces;
using AMANC_Inventory.Users.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace AMANC_Inventory.Users.Services
{
    public class UserService : IUserService
    {
        private readonly FirebaseClient _dbClient;
        private readonly IAuthService _authService;

        public UserService() : this(new AuthService()) { }

        public UserService(IAuthService authService)
        {
            _dbClient = new FirebaseClient(FirebaseConfig.RealtimeDbUrl);
            _authService = authService;
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            try
            {
                // OnceSingleAsync<JToken>() obtiene el JSON para procesar Objetos {} o Arreglos []
                var token = await _dbClient
                    .Child("Usuarios")
                    .OnceSingleAsync<JToken>();

                if (token == null || token.Type == JTokenType.Null)
                    return new List<UserModel>();

                var userList = new List<UserModel>();

                if (token is JArray jArray)
                {
                    // Procesa la respuesta cuando Firebase devuelve un Arreglo por usar claves 1, 2, 3...
                    foreach (var item in jArray)
                    {
                        if (item != null && item.Type != JTokenType.Null)
                        {
                            var user = item.ToObject<UserModel>();
                            if (user != null)
                            {
                                userList.Add(user);
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
                            var user = prop.Value.ToObject<UserModel>();
                            if (user != null)
                            {
                                if (string.IsNullOrEmpty(user.Id)) user.Id = prop.Name;
                                userList.Add(user);
                            }
                        }
                    }
                }

                return userList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error crítico en GetAllUsersAsync: {ex.Message}");
                return new List<UserModel>();
            }
        }

        private async Task<string> GetNextCustomIdAsync(string nodeName)
        {
            try
            {
                var users = await GetAllUsersAsync();

                if (!users.Any()) return "1";

                int maxId = users
                    .Select(u =>
                    {
                        string idToParse = !string.IsNullOrWhiteSpace(u.Id) ? u.Id : "0";
                        string numericPart = Regex.Match(idToParse, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return (maxId + 1).ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al calcular siguiente ID: {ex.Message}");
                return "1";
            }
        }

        public async Task<bool> RegisterUserAsync(UserModel userProfile, string password)
        {
            try
            {
                if (userProfile == null || string.IsNullOrWhiteSpace(userProfile.Email))
                {
                    return false;
                }

                userProfile.Email = userProfile.Email.Trim().ToLower();

                // 1. Crear usuario en Firebase Auth
                string uid = await _authService.CreateUserAsync(userProfile.Email, password);

                // 2. Generar ID numérico consecutivo (1, 2, 3...)
                string customId = await GetNextCustomIdAsync("Usuarios");

                userProfile.Id = customId;
                userProfile.AuthUid = uid;
                userProfile.CreatedAt = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(userProfile.Status))
                {
                    userProfile.Status = "Active";
                }

                // 3. Guardar en Realtime Database
                await _dbClient
                    .Child("Usuarios")
                    .Child(customId)
                    .PutAsync(userProfile);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error durante el registro: {ex.Message}");
                throw new Exception($"Error durante el registro: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return false;

                string emailLimpio = email.Trim().ToLower();

                // 1. Iniciar sesión en Firebase Auth
                string uid = await _authService.SignInAsync(emailLimpio, password);

                if (string.IsNullOrEmpty(uid))
                {
                    Debug.WriteLine("[Login Error]: Firebase Auth rechazó las credenciales.");
                    return false;
                }

                // 2. Consultar el perfil
                var userProfile = await GetUserByEmailAsync(emailLimpio);

                if (userProfile == null)
                {
                    Debug.WriteLine("[Login Alert]: Usuario autenticado en Auth pero no encontrado en base de datos.");
                    return true;
                }

                // 3. Validar estatus activo
                bool isActive = string.IsNullOrWhiteSpace(userProfile.Status) ||
                                userProfile.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                                userProfile.Status.Equals("Activo", StringComparison.OrdinalIgnoreCase);

                return isActive;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al validar credenciales: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                string emailLimpio = email.Trim().ToLower();
                var todosLosUsuarios = await GetAllUsersAsync();

                return todosLosUsuarios.Any(u =>
                    !string.IsNullOrEmpty(u.Email) &&
                    u.Email.Trim().ToLower() == emailLimpio
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en EmailExistsAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<UserModel?> GetUserByEmailAsync(string? email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;

                string emailLimpio = email.Trim().ToLower();
                var todosLosUsuarios = await GetAllUsersAsync();

                return todosLosUsuarios.FirstOrDefault(u =>
                    !string.IsNullOrEmpty(u.Email) &&
                    u.Email.Trim().ToLower() == emailLimpio
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetUserByEmailAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(UserModel userProfile)
        {
            try
            {
                if (userProfile == null || string.IsNullOrEmpty(userProfile.Id))
                    return false;

                await _dbClient
                    .Child("Usuarios")
                    .Child(userProfile.Id)
                    .PutAsync(userProfile);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UpdateUserProfileAsync: {ex.Message}");
                return false;
            }
        }
    }
}