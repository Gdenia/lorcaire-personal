namespace Lorcaire.Application.Errors;

public abstract class NotFoundException : Exception, IUserFacingException
{
    protected NotFoundException(string message)
        : base(message)
    {
    }
}
