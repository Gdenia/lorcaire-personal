using Lorcaire.Application.Errors;

namespace Lorcaire.Desktop.Presentation;

internal static class UserErrorMessages
{
    public static string Format(string operation, Exception exception)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentNullException.ThrowIfNull(exception);

        return exception is ArgumentException or IUserFacingException
            ? exception.Message
            : $"{operation}. Please try again.";
    }
}
