using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AMANC_Inventory.Config;
using AMANC_Inventory.Interfaces;
using AMANC_Inventory.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace AMANC_Inventory.Services
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

        public async Task<bool> RegisterUserAsync(UserModel userProfile, string password)
        {
            try
            {
                if (userProfile == null || string.IsNullOrWhiteSpace(userProfile.Email))
                {
                    return false;
                }

                userProfile.Email = userProfile.Email.Trim().ToLower();

                string uid = await _authService.CreateUserAsync(userProfile.Email, password);

                userProfile.Id = uid;
                userProfile.CreatedAt = DateTime.UtcNow;

                await _dbClient
                    .Child("Usuarios")
                    .Child(uid)
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

                var userProfile = await _dbClient
                    .Child("Usuarios")
                    .Child(uid)
                    .OnceSingleAsync<UserModel>();

                // Devuelve true solo si el usuario existe y está activo
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