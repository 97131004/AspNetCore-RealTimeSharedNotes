namespace AspNetCore_RealTimeSharedNotes.Models;

public class ApiKey
{
    public string UserId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string EncryptedClientSecret { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
