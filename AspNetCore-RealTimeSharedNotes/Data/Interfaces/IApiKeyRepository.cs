using AspNetCore_RealTimeSharedNotes.Models;

namespace AspNetCore_RealTimeSharedNotes.Data.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetApiKeyAsync(string clientId);
    Task<ApiKey> AddApiKeyAsync(ApiKey apiKey);
    Task<bool> DeleteApiKeyAsync(string userId);
}
