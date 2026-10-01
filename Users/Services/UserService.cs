using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Modules.Auth.Interfaces;
using AMANC_Inventory.Modules.Auth.Services;
using AMANC_Inventory.Users.Interfaces;
using AMANC_Inventory.Users.Models;
using Firebase.Database;
using Firebase.Database.Query;

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

        /// <summary>
        /// Genera el siguiente ID numérico consecutivo para el nodo especificado.
        /// </summary>
        private async Task<string> GetNextCustomIdAsync(string nodeName, string prefix = "USR-")
        {
            try
            {
                var items = await _dbClient
                    .Child(nodeName)
                    .OnceAsync<UserModel>();

                if (!items.Any()) return $"{prefix}0001";

                int maxId = items
                    .Select(i =>
                    {
                        string key = i.Key.Replace(prefix, "");
                        return int.TryParse(key, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return $"{prefix}{(maxId + 1):D4}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al calcular siguiente ID: {ex.Message}");
                return $"{prefix}0001";
            }
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            try
            {
                var items = await _dbClient
                    .Child("Usuarios")
                    .OnceAsync<UserModel>();

                return items
                    .Where(item => item.Object != null)
                    .Select(item =>
                    {
                        var user = item.Object;
                        user.Id = string.IsNullOrEmpty(user.Id) ? item.Key : user.Id;
                        return user;
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error crítico en GetAllUsersAsync: {ex.Message}");
                return new List<UserModel>();
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

        // 1. Crear en Firebase Auth
        string uid = await _authService.CreateUserAsync(userProfile.Email, password);

        // 2. Generar ID con prefijo (ej: "USR-0001")
        string customId = await GetNextCustomIdAsync("Usuarios", "USR-");

        userProfile.Id = customId;
        userProfile.AuthUid = uid;
        userProfile.CreatedAt = DateTime.UtcNow;

        // 3. Guardar en Realtime Database
        await _dbClient
            .Child("Usuarios")
            .Child(customId)
            .PutAsync(userProfile);

        return true;
    }
    catch (Exception ex)
    {
        throw new Exception($"Error durante el registro: {ex.Message}", ex);
    }
}

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email)) return false;

                string uid = await _authService.SignInAsync(email, password);

                if (string.IsNullOrEmpty(uid)) return false;

                // Consulta al usuario por correo para ubicarlo sin importar el ID de clave del nodo
                var userProfile = await GetUserByEmailAsync(email);

                return userProfile != null && userProfile.Status == "Active";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al validar credenciales: {ex.Message}");
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

                var todosLosUsuarios = await _dbClient
                    .Child("Usuarios")
                    .OnceAsync<UserModel>();

                return todosLosUsuarios.Any(u =>
                    u.Object != null &&
                    !string.IsNullOrEmpty(u.Object.Email) &&
                    u.Object.Email.Trim().ToLower() == emailLimpio
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error crítico en EmailExistsAsync: {ex.Message}");
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

                var todosLosUsuarios = await _dbClient
                    .Child("Usuarios")
                    .OnceAsync<UserModel>();

                var usuarioEncontrado = todosLosUsuarios.FirstOrDefault(u =>
                    u.Object != null &&
                    !string.IsNullOrEmpty(u.Object.Email) &&
                    u.Object.Email.Trim().ToLower() == emailLimpio
                );

                return usuarioEncontrado?.Object;
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