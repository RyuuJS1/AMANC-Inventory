using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AMANC_Inventory.Config;
using AMANC_Inventory.Interfaces;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Newtonsoft.Json;

namespace AMANC_Inventory.Services
{
    public class AuthService : IAuthService
    {
        private readonly FirebaseAuthClient _authClient;
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string FirebaseRestApiUrl = "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=";

        public AuthService()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = FirebaseConfig.ApiKey,
                AuthDomain = FirebaseConfig.AuthDomain,
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                }
            };

            _authClient = new FirebaseAuthClient(config);
        }

        public async Task<string> CreateUserAsync(string email, string password)
        {
            var userCredential = await _authClient.CreateUserWithEmailAndPasswordAsync(email, password);
            return userCredential.User.Uid;
        }

        public async Task<string> SignInAsync(string email, string password)
        {
            var userCredential = await _authClient.SignInWithEmailAndPasswordAsync(email, password);
            return userCredential.User.Uid;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var payload = new
                {
                    requestType = "PASSWORD_RESET",
                    email = email
                };

                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string url = $"{FirebaseRestApiUrl}{FirebaseConfig.ApiKey}";
                var response = await _httpClient.PostAsync(url, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}