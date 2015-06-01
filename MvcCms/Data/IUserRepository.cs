using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MvcCms.Models;

namespace MvcCms.Data
{
    public interface IUserRepository
    {
        Task<CmsUser> GetUserByNameAsync(string username);
        IEnumerable<CmsUser> GetAllUsers();
        Task CreateAsync(CmsUser user, string password);
        Task DeleteAsync(CmsUser user);
        Task UpdateAsync(CmsUser user);
        string HashPassword(string newPassword);
        bool VerifyPassword(string passwordHash, string currentPassword);
        Task AddUserToRoleAsync(CmsUser newUser, string role);
        Task<IEnumerable<string>> GetRolesForUserAsync(CmsUser user);
        Task RemoveUserFromRoleAsync(CmsUser user, params string[] roleNames);
    }
}