using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AMANC_Inventory.Models;

namespace AMANC_Inventory.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(UserModel userProfile, string password);
        Task<bool> ValidateCredentialsAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task<UserModel?> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserProfileAsync(UserModel userProfile);
    }
}