using Microsoft.Playwright;

namespace AspNetCore_RealTimeSharedNotes.PlaywrightTests.PageObjects;

public class NotesPage(IPage page)
{
    private readonly IPage _page = page;

    public async Task GoToAsync()
    {
        await _page.GotoAsync(Config.TestConfig.BaseUrl + "/notes");
        //wait for the async signalr notes load to finish
        await _page.GetByTestId("notes-loading").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden
        });
    }

    public ILocator NotesList => _page.GetByTestId("notes-list");

    public async Task<List<string>> GetNoteIdsAsync()
    {
        var cards = _page.GetByTestId("note-card");
        var count = await cards.CountAsync();

        var ids = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var id = await cards.Nth(i).GetAttributeAsync("data-note-id") ?? string.Empty;
            ids.Add(id);
        }
        return ids;
    }

    //posts a note, then waits until its displayed in notes list, returns note id
    public async Task<string> PostNoteAsync(string content)
    {
        await _page.GetByTestId("note-textarea").FillAsync(content);
        await _page.GetByTestId("note-post-btn").ClickAsync();

        var card = _page.GetByTestId("note-card").First;
        await Assertions.Expect(card.GetByTestId("note-content")).ToHaveTextAsync(content);

        return await card.GetAttributeAsync("data-note-id") ?? string.Empty;
    }


    //waits until a note id is visible in the notes list, return note's content
    public async Task<string> WaitForNoteAsync(string noteId)
    {
        var card = _page.Locator($"[data-note-id='{noteId}']");
        await card.WaitForAsync();

        return await card.GetByTestId("note-content").InnerTextAsync();
    }
}
