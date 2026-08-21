using Lorcaire.Application.Errors;
using Lorcaire.Desktop.Presentation;

namespace Lorcaire.Desktop.Tests.Presentation;

public sealed class UserErrorMessagesTests
{
    [Fact]
    public void Format_PreservesSafeValidationAndConflictMessages()
    {
        Assert.Equal(
            "The title is required.",
            UserErrorMessages.Format(
                "Unable to save",
                new ArgumentException("The title is required.")));
        Assert.Equal(
            "The item conflicts with existing data.",
            UserErrorMessages.Format(
                "Unable to save",
                new ConflictException(
                    "The item conflicts with existing data.",
                    new InvalidOperationException("technical detail"))));
    }

    [Fact]
    public void Format_HidesUnexpectedTechnicalDetails()
    {
        var result = UserErrorMessages.Format(
            "Unable to save the item",
            new InvalidOperationException("SQLite internal details"));

        Assert.Equal("Unable to save the item. Please try again.", result);
        Assert.DoesNotContain("SQLite", result);
    }
}
