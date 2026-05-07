using JobPortal_API.DTOs;
using JobPortal_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal_API.Utilities.Interfaces
{
    public interface IUserHelper
    {
        Task<ApplicationUser> CreateUser([FromBody] RegisterationRequestDTO model);


        Task<IdentityResult> AddUserAsync(ApplicationUser user, string password);


        Task AddUserToRoleAsync(ApplicationUser user, string roleName);

        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string oldPassword, string newPassword);

        Task CheckRoleAsync(string roleName);

        Task CreateRolesAsync();

        Task<ApplicationUser> GetUserByEmailAsync(string email);

        Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName);

        Task<Microsoft.AspNetCore.Identity.SignInResult> LoginAsync(RegisterationRequestDTO model);

        Task LogoutAsync();

        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);

        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);

        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);

        Task<ApplicationUser> GetUserByIdAsync(string id);

        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string password);

        Task<Microsoft.AspNetCore.Identity.SignInResult> ValidatePasswordAsync(ApplicationUser user, string password);

        Task<IList<string>> GetRolesAsync(ApplicationUser user);


        Task<bool> ExistsAsync(string email);
    }
}
