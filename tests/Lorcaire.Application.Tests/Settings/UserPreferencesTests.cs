using Lorcaire.Application.Settings;

namespace Lorcaire.Application.Tests.Settings;

public sealed class UserPreferencesTests
{
    [Fact]
    public void Default_UsesSafeFunctionalValues()
    {
        var preferences = UserPreferences.Default;

        Assert.Equal("User", preferences.DisplayName);
        Assert.Equal(AppTheme.Dark, preferences.Theme);
        Assert.True(preferences.ShowCompletedTasks);
    }

    [Fact]
    public void Constructor_NormalizesDisplayName()
    {
        var preferences = new UserPreferences(
            "  Denia  ",
            AppTheme.Dark,
            showCompletedTasks: false);

        Assert.Equal("Denia", preferences.DisplayName);
        Assert.False(preferences.ShowCompletedTasks);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptyDisplayName(string displayName)
    {
        Assert.Throws<ArgumentException>(() =>
            new UserPreferences(
                displayName,
                AppTheme.Dark,
                showCompletedTasks: true));
    }

    [Fact]
    public void Constructor_RejectsExcessivelyLongDisplayName()
    {
        var displayName = new string(
            'a',
            UserPreferences.MaximumDisplayNameLength + 1);

        Assert.Throws<ArgumentException>(() =>
            new UserPreferences(
                displayName,
                AppTheme.Dark,
                showCompletedTasks: true));
    }

    [Fact]
    public void Constructor_RejectsUnavailableTheme()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new UserPreferences(
                "User",
                (AppTheme)999,
                showCompletedTasks: true));
    }
}
