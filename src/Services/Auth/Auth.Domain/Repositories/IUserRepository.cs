using Microsoft.AspNetCore.Identity;
using PaymentProcessor.Services.Auth.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentProcessor.Services.Auth.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetByIdAsync(string id);
        Task<ApplicationUser> GetByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
        Task<IdentityResult> UpdateAsync(ApplicationUser user);
    }
}