using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMANC_Inventory.Interfaces
{
    public interface IAuthService
    {
        Task<string> CreateUserAsync(string email, string password);
        Task<string> SignInAsync(string email, string password);
        Task<bool> SendPasswordResetEmailAsync(string email);
    }
}