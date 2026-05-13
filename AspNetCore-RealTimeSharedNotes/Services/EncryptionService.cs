using AspNetCore_RealTimeSharedNotes.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace AspNetCore_RealTimeSharedNotes.Services;

public class EncryptionService : IEncryptionService
{
    //https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/default-settings?view=aspnetcore-2.1
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("ApiKeyProtection");
    }

    public string Encrypt(string plainText) => _protector.Protect(plainText);
    public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
}

