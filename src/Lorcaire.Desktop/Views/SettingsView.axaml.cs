using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application.Settings;

namespace Lorcaire.Desktop.Views;

public partial class SettingsView : UserControl
{
    private readonly GetUserPreferencesHandler _getPreferencesHandler;
    private readonly SaveUserPreferencesHandler _savePreferencesHandler;
    private readonly Action<UserPreferences>? _preferencesSaved;

    public SettingsView(
        GetUserPreferencesHandler getPreferencesHandler,
        SaveUserPreferencesHandler savePreferencesHandler,
        Action<UserPreferences>? preferencesSaved = null)
    {
        ArgumentNullException.ThrowIfNull(getPreferencesHandler);
        ArgumentNullException.ThrowIfNull(savePreferencesHandler);

        _getPreferencesHandler = getPreferencesHandler;
        _savePreferencesHandler = savePreferencesHandler;
        _preferencesSaved = preferencesSaved;

        InitializeComponent();
        Loaded += LoadSettings;
    }

    private async void LoadSettings(object? sender, RoutedEventArgs e)
    {
        try
        {
            ApplyToForm(await _getPreferencesHandler.HandleAsync());
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to load settings: {exception.Message}";
        }
    }

    private async void SaveSettings(object? sender, RoutedEventArgs e)
    {
        SaveSettingsButton.IsEnabled = false;
        OperationMessage.Text = string.Empty;

        try
        {
            var preferences = await _savePreferencesHandler.HandleAsync(
                new SaveUserPreferencesCommand(
                    DisplayName.Text ?? string.Empty,
                    AppTheme.Dark,
                    ShowCompletedTasks.IsChecked == true));

            ApplyToForm(preferences);
            _preferencesSaved?.Invoke(preferences);
            OperationMessage.Text = "Settings saved.";
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to save settings: {exception.Message}";
        }
        finally
        {
            SaveSettingsButton.IsEnabled = true;
        }
    }

    private void ApplyToForm(UserPreferences preferences)
    {
        DisplayName.Text = preferences.DisplayName;
        ShowCompletedTasks.IsChecked = preferences.ShowCompletedTasks;
    }
}
