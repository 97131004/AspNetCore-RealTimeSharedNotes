using System.ComponentModel.DataAnnotations;
using AspNetCore_RealTimeSharedNotes.Models.Constants;

namespace AspNetCore_RealTimeSharedNotes.Models.ViewModels;

public class CreateUserViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(ModelConstants.PasswordMinLength)]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = UserRoles.User;
}
