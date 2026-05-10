namespace AspNetCore_RealTimeSharedNotes.Routing;

public static class RouteConfig
{
    public static void MapAppRoutes(this WebApplication app)
    {
        app.MapControllerRoute(
            name: "users",
            pattern: "users",
            defaults: new { controller = "Users", action = "Index" });

        app.MapControllerRoute(
            name: "notes",
            pattern: "notes",
            defaults: new { controller = "Notes", action = "Index" });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}");
    }
}
