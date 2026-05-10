namespace AspNetCore_RealTimeSharedNotes.PlaywrightTests.Config;

public static class TestConfig
{
    //populated at start from main project
    public static string BaseUrl { get; set; } = string.Empty;
    public static string SuperAdminEmail { get; set; } = string.Empty;
    public static string SuperAdminPassword { get; set; } = string.Empty;

    //test user credentials
    public static string TestUserEmail => "user@user";
    public static string TestUserPassword => "123456";
    public static string TestUserRole => "user";
}
