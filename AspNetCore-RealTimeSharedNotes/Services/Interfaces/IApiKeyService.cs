using AspNetCore_RealTimeSharedNotes.Models.ViewModels;

namespace AspNetCore_RealTimeSharedNotes.Services.Interfaces;

public interface IApiKeyService
{
    Task<ApiKeyViewModel> CreateApiKeyAsync(string userId);
    Task<bool> ValidateApiKeyAsync(string clientId, string clientSecret);
    Task<string?> GetUserIdByClientIdAsync(string clientId);
    Task<bool> DeleteApiKeyAsync(string userId);
}
