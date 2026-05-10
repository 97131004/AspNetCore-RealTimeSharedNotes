namespace AspNetCore_RealTimeSharedNotes.Models.Constants;

//important: changes require a database rebuild from scratch
public static class ModelConstants
{
    public const int NoteContentMaxLength = 1000;
    public const int ApiKeyClientIdMaxLength = 100;
    public const int PasswordMinLength = 6;
}
