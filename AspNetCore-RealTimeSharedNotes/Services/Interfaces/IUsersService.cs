using AspNetCore_RealTimeSharedNotes.Models.Dtos;
using AspNetCore_RealTimeSharedNotes.Models;
using AspNetCore_RealTimeSharedNotes.Models.ViewModels;

namespace AspNetCore_RealTimeSharedNotes.Services.Interfaces;

public interface IUsersService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<(bool success, string? error, ApiKeyViewModel? apiKey)> CreateUserAsync(CreateUserViewModel model, string creatorRole);
    Task<bool> DeleteUserAsync(string requestingUserId, string requestingRole, string targetUserId);
    Task<ApplicationUser> GetUserAsync(string userId);
}
