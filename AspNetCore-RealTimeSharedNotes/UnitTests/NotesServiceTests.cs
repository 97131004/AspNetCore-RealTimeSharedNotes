using AspNetCore_RealTimeSharedNotes.Data.Interfaces;
using AspNetCore_RealTimeSharedNotes.Models;
using AspNetCore_RealTimeSharedNotes.Models.Constants;
using AspNetCore_RealTimeSharedNotes.Services;
using Moq;

namespace AspNetCore_RealTimeSharedNotes.UnitTests;

[TestFixture]
public class NotesServiceTests : TestBase
{
    private Mock<INotesRepository> _repoMock;
    private NotesService _service;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<INotesRepository>();
        _service = new NotesService(_repoMock.Object);
    }

    //helpers

    private static Note MakeNote(int id, ApplicationUser owner, string content = "hello")
    {
        return new Note() {
            NoteId = id,
            UserId = owner.Id,
            Content = content,
            User = owner
        };
    }

    //AddNoteAsync

    [Test]
    public async Task AddNoteAsync_ContentExceedsMaxLength_SavesTruncatedContent()
    {
        var longContent = new string('x', ModelConstants.NoteContentMaxLength + 500);
        var userId = "user-1";
        var role = UserRoles.User;

        _repoMock
            .Setup(r => r.AddNoteAsync(It.IsAny<Note>()))
            .ReturnsAsync((Note n) =>
            {
                n.NoteId = 1;
                n.User = MakeUser(userId, "user@test.com", role);
                return n;
            });

        var result = await _service.AddNoteAsync(userId, role, longContent);

        //checks resulting output
        Assert.That(result.Content.Length, Is.EqualTo(ModelConstants.NoteContentMaxLength));

        //checks if database call (AddNoteAsync) was actually called with correct parameters, exactly once
        _repoMock.Verify(r => r.AddNoteAsync(It.Is<Note>(n => n.Content.Length == ModelConstants.NoteContentMaxLength)), Times.Once);
    }

    //DeleteNoteAsync

    public record DeleteNoteCase(
        string OwnerId,
        string OwnerRole,
        string RequesterId,
        string RequesterRole,
        bool ExpectedResult,
        Times DeleteCallTimes);

    private static TestCaseData[] DeleteNoteAsync_Cases() =>
    [
        new TestCaseData(new DeleteNoteCase("owner-id", UserRoles.User, "owner-id", UserRoles.User, true, Times.Once()))
            .SetName("RequestingUserIsOwner_Succeeds"),
        new TestCaseData(new DeleteNoteCase("user-id", UserRoles.Admin, "superadmin-id", UserRoles.SuperAdmin, true, Times.Once()))
            .SetName("SuperAdmin_CanDeleteAnyNote"),
        new TestCaseData(new DeleteNoteCase("admin-id", UserRoles.Admin, "other-admin-id", UserRoles.Admin, false, Times.Never()))
            .SetName("Admin_CannotDeleteAnotherAdminsNote"),
        new TestCaseData(new DeleteNoteCase("user-id", UserRoles.User, "admin-id", UserRoles.Admin, true, Times.Once()))
            .SetName("Admin_CanDeletePlainUsersNote"),
    ];

    //will be run with different test cases (parameters) defined in DeleteNoteAsync_Cases method
    [TestCaseSource(nameof(DeleteNoteAsync_Cases))]
    public async Task DeleteNoteAsync(DeleteNoteCase dnc)
    {
        var owner = MakeUser(dnc.OwnerId, $"{dnc.OwnerId}@test.com", dnc.OwnerRole);
        var note = MakeNote(1, owner);

        _repoMock.Setup(r => r.GetNoteAsync(1)).ReturnsAsync(note);
        _repoMock.Setup(r => r.DeleteNoteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteNoteAsync(1, dnc.RequesterId, dnc.RequesterRole);

        Assert.That(result, Is.EqualTo(dnc.ExpectedResult));
        _repoMock.Verify(r => r.DeleteNoteAsync(It.IsAny<int>()), dnc.DeleteCallTimes);
    }
}
