using AspNetCore_RealTimeSharedNotes.Data.Interfaces;
using AspNetCore_RealTimeSharedNotes.Models;
using AspNetCore_RealTimeSharedNotes.Models.ViewModels;
using AspNetCore_RealTimeSharedNotes.Services.Interfaces;

namespace AspNetCore_RealTimeSharedNotes.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IApiKeyRepository _repo;
    private readonly IEncryptionService _encryption;

    public ApiKeyService(IApiKeyRepository repo, IEncryptionService encryption)
    {
        _repo = repo;
        _encryption = encryption;
    }

    //generates a new client id + secret, stores the encrypted secret
    public async Task<ApiKeyViewModel> CreateApiKeyAsync(string userId)
    {
        var clientId = Guid.NewGuid().ToString("N");
        var clientSecret = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        await _repo.AddApiKeyAsync(new ApiKey
        {
            UserId = userId,
            ClientId = clientId,
            EncryptedClientSecret = _encryption.Encrypt(clientSecret)
        });

        return new ApiKeyViewModel { ClientId = clientId, ClientSecret = clientSecret };
    }

    public async Task<bool> ValidateApiKeyAsync(string clientId, string clientSecret)
    {
        var key = await _repo.GetApiKeyAsync(clientId);
        if (key == null)
            return false;
        return _encryption.Decrypt(key.EncryptedClientSecret) == clientSecret;
    }

    public async Task<string?> GetUserIdByClientIdAsync(string clientId)
    {
        var key = await _repo.GetApiKeyAsync(clientId);
        return key?.UserId;
    }

    public async Task<bool> DeleteApiKeyAsync(string userId)
    {
        return await _repo.DeleteApiKeyAsync(userId);
    }
}
