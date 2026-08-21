namespace Lorcaire.Application.Errors;

public sealed class ConflictException : InvalidOperationException, IUserFacingException
{
    public ConflictException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
