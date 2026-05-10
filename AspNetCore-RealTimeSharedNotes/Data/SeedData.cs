using AspNetCore_RealTimeSharedNotes.Data.Interfaces;
using AspNetCore_RealTimeSharedNotes.Models;
using AspNetCore_RealTimeSharedNotes.Models.Constants;
using AspNetCore_RealTimeSharedNotes.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore_RealTimeSharedNotes.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();

        string[] roles = [UserRoles.SuperAdmin, UserRoles.Admin, UserRoles.User];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var superAdminEmail = config["SuperAdmin:Email"]!;
        var superAdminPassword = config["SuperAdmin:Password"]!;
        var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
        if (superAdmin == null)
        {
            superAdmin = new ApplicationUser { UserName = superAdminEmail, Email = superAdminEmail };
            var result = await userManager.CreateAsync(superAdmin, superAdminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, UserRoles.SuperAdmin);
        }

        //create superadmin api key if not yet present
        var apiKeyRepo = services.GetRequiredService<IApiKeyRepository>();
        var encryption = services.GetRequiredService<IEncryptionService>();
        var existingKey = await apiKeyRepo.GetApiKeyAsync(config["SuperAdmin:ApiClientId"]!);
        if (existingKey == null)
        {
            var clientId = config["SuperAdmin:ApiClientId"]!;
            var clientSecret = config["SuperAdmin:ApiClientSecret"]!;
            await apiKeyRepo.AddApiKeyAsync(new ApiKey
            {
                UserId = superAdmin.Id,
                ClientId = clientId,
                EncryptedClientSecret = encryption.Encrypt(clientSecret)
            });
        }
    }
}

