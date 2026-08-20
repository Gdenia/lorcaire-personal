using Avalonia.Controls;
using Lorcaire.Application.Settings;

namespace Lorcaire.Desktop.Composition;

public interface IPageFactory
{
    Control Create(
        DesktopPage page,
        Action<DesktopPage> navigate,
        Action<string> greetingChanged,
        Action<UserPreferences> preferencesSaved);
}
