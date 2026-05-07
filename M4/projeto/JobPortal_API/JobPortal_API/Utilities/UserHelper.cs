using Azure;
using JobPortal_API.Data;
using JobPortal_API.DTOs;
using JobPortal_API.Models;
using JobPortal_API.Utilities.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal_API.Utilities
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext _context;


        public UserHelper(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }


        public async Task<ApplicationUser> CreateUser([FromBody] RegisterationRequestDTO model)
        {
            if (model == null) // se o modelo for nulo, retorna nulo
            {
                return null;
            }

            var user = await GetUserByEmailAsync(model.Email); //buscar user  
            if (user != null)
            {
                return null; //já existe o user --> resposta negativa (null)
            }

            var newUser = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, model.Role); // "Admin", "Candidato", "Empresa"


                if (model.Role == "Candidato")
                {
                    var candidato = new Candidato
                    {
                        UserId = newUser.Id,
                        Nome = model.UserName,
                        Email = model.Email
                    };
                    _context.Candidato.Add(candidato);
                }
                
                if (model.Role == "Empresa")
                {
                    var empresa = new Empresa
                    {
                        UserId = newUser.Id,
                        Nome = model.UserName,
                        Email = model.Email
                    };
                    _context.Empresa.Add(empresa);
                }
                
                return user;
            }
            return null;
        }

        /// <summary>
        /// Creates a new user with the specified password.
        /// </summary>
        /// <param name="user">The "User" entity to create.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains an "IdentityResult" indicating the success or failure of the operation.
        /// </returns>
        public async Task<IdentityResult> AddUserAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }


        /// <summary>
        /// Asynchronously adds a user to a specified role.
        /// </summary>
        /// <param name="user">The "User" entity to add to the role.</param>
        /// <param name="roleName">The name of the role to add the user to.</param>
        /// <returns>A "Task" that represents the asynchronous operation.</returns>
        public async Task AddUserToRoleAsync(ApplicationUser user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }


        /// <summary>
        /// Asynchronously changes the password for a user.
        /// </summary>
        /// <param name="user">The "User" entity whose password will be changed.</param>
        /// <param name="oldPassword">The current password of the user.</param>
        /// <param name="newPassword">The new password for the user.</param>
        /// <returns>
        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }


        /// <summary>
        /// Asynchronously checks if a role exists, if it doesn't it creates the role.
        /// </summary>
        /// <param name="roleName">The name of the role to check.</param>
        /// <returns>A "Task" that represents the asynchronous operation.</returns>
        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName); // se existe, buscar o role

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });

            }

        }


        /// <summary>
        /// Creates predefined roles ("Employee", "Student", "Admin") if they do not already exist in the system.
        /// </summary>
        /// <returns>A "Task" that represents the asynchronous operation.</returns>
        public async Task CreateRolesAsync()
        {
            string[] roleNames = { "Empresa", "Candidato", "Admin", "SysAdmin" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);

                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }



        /// <summary>
        /// Asynchronously retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>
        /// A "Task{TResult} that represents the asynchronous operation.
        /// The task result contains the "User" entity if found, otherwise "null".
        /// </returns>
        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }


        /// <summary>
        /// Asynchronously checks if a user is a member of a specified role.
        /// </summary>
        /// <param name="user">The "User" entity to check.</param>
        /// <param name="roleName">The name of the role to check against.</param>
        /// <returns>
        /// A "Task{TResult} that represents the asynchronous operation.
        /// The task result contains "true" if the user is in the specified role, otherwise "false".
        /// </returns>
        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName); //devolve uma booleana dizendo se user está no role ou não
        }


        /// <summary>
        /// Asynchronously attempts to sign in a user with the provided credentials.
        /// </summary>
        /// <param name="model">The "LoginViewModel" containing the username, password, and remember me preference.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains a "SignInResult" indicating the outcome of the sign-in attempt.
        /// </returns>
        public async Task<Microsoft.AspNetCore.Identity.SignInResult> LoginAsync(RegisterationRequestDTO model)
        {
            return await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
        }


        /// <summary>
        /// Asynchronously signs out the currently authenticated user.
        /// </summary>
        /// <returns>A "Task" that represents the asynchronous operation.</returns>
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }


        /// <summary>
        /// Asynchronously updates the user's information in the data store.
        /// </summary>
        /// <param name="user">The "User" entity with updated properties.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains an "IdentityResult" indicating the success or failure of the update.
        /// </returns>
        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }


        /// <summary>
        /// Asynchronously generates an email confirmation token for the specified user.
        /// </summary>
        /// <param name="user">The "User" entity for whom to generate the token.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains the generated email confirmation token.
        /// </returns>
        public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }


        /// <summary>
        /// Asynchronously generates a password reset token for the specified user.
        /// </summary>
        /// <param name="user">The "User" entity for whom to generate the token.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains the generated password reset token.
        /// </returns>
        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }


        /// <summary>
        /// Asynchronously confirms the user's email address using a token.
        /// </summary>
        /// <param name="user">The "User" entity whose email is to be confirmed.</param>
        /// <param name="token">The email confirmation token.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains an "IdentityResult" indicating the success or failure of the confirmation.
        /// </returns>
        public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);

        }


        /// <summary>
        /// Asynchronously retrieves a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains the "User" entity if found, otherwise "null".
        /// </returns>
        public Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return _userManager.FindByIdAsync(id);
        }



        /// <summary>
        /// Asynchronously resets a user's password using a password reset token.
        /// </summary>
        /// <param name="user">The "User" entity whose password is to be reset.</param>
        /// <param name="token">The password reset token.</param>
        /// <param name="password">The new password for the user.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains an "IdentityResult" indicating the success or failure of the password reset.
        /// </returns>
        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }


        /// <summary>
        /// Asynchronously validates a user's password without signing them in.
        /// </summary>
        /// <param name="user">The "User" entity whose password is to be validated.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains a "SignInResult" indicating whether the password is valid.
        /// </returns>
        public async Task<Microsoft.AspNetCore.Identity.SignInResult> ValidatePasswordAsync(ApplicationUser user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, false);
        }


        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

      
        public async Task<bool> ExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }


            var user = await _userManager.FindByEmailAsync(email);
            {
                if (user != null)
                {
                    return true;
                }

                return false;
            }

        }

    }
}
