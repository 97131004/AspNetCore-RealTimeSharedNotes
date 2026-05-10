using AspNetCore_RealTimeSharedNotes.Models.Constants;
using AspNetCore_RealTimeSharedNotes.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspNetCore_RealTimeSharedNotes.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IUsersService _userService;
    private readonly IConfiguration _config;

    public JwtTokenService(IApiKeyService apiKeyService, IUsersService userService, IConfiguration config)
    {
        _apiKeyService = apiKeyService;
        _userService = userService;
        _config = config;
    }

    public async Task<string?> CreateTokenAsync(string clientId, string clientSecret)
    {
        if (!await _apiKeyService.ValidateApiKeyAsync(clientId, clientSecret))
            return null;

        var userId = await _apiKeyService.GetUserIdByClientIdAsync(clientId);
        if (userId == null)
            return null;

        var user = await _userService.GetUserAsync(userId);
        if (user == null)
            return null;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var expiry = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, user?.UserRoles.FirstOrDefault()?.Role.Name ?? UserRoles.User)
            ],
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
