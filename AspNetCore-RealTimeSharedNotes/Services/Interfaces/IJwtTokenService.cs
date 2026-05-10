namespace AspNetCore_RealTimeSharedNotes.Services.Interfaces;

public interface IJwtTokenService
{
    Task<string?> CreateTokenAsync(string clientId, string clientSecret);
}
