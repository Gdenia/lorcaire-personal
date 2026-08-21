namespace Lorcaire.Core.Domain;

internal static class DomainValidation
{
    public static void EnsureIdentifier(
        Guid value,
        string entityName,
        string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                $"The {entityName} identifier cannot be empty.",
                parameterName);
        }
    }

    public static string RequiredText(
        string value,
        int maximumLength,
        string fieldName,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"The {fieldName} is required.",
                parameterName);
        }

        var normalized = value.Trim();
        EnsureMaximumLength(
            normalized,
            maximumLength,
            fieldName,
            parameterName);
        return normalized;
    }

    public static string? OptionalText(
        string? value,
        int maximumLength,
        string fieldName,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        EnsureMaximumLength(
            normalized,
            maximumLength,
            fieldName,
            parameterName);
        return normalized;
    }

    private static void EnsureMaximumLength(
        string value,
        int maximumLength,
        string fieldName,
        string parameterName)
    {
        if (value.Length > maximumLength)
        {
            throw new ArgumentException(
                $"The {fieldName} cannot exceed {maximumLength} characters.",
                parameterName);
        }
    }
}
