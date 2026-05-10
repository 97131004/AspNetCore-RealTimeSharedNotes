using Microsoft.Playwright;

namespace AspNetCore_RealTimeSharedNotes.PlaywrightTests.PageObjects;

public class LoginPage(IPage page)
{
    private readonly IPage _page = page;

    public async Task GoToAsync()
    {
        await _page.GotoAsync(Config.TestConfig.BaseUrl + "/");
    }

    public async Task LoginAsync(string email, string password)
    {
        await _page.GetByTestId("email-input").FillAsync(email);
        await _page.GetByTestId("password-input").FillAsync(password);
        await _page.GetByTestId("login-btn").ClickAsync();
        await _page.WaitForURLAsync(u => !u.Contains("/Login") && !u.EndsWith("/"));
    }
}
