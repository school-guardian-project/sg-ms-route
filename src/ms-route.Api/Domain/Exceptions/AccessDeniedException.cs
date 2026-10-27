namespace ms_route.Api.Domain.Exceptions;

/// <summary>
/// El usuario autenticado no puede actuar sobre el recurso pedido (403).
/// </summary>
public sealed class AccessDeniedException : Exception
{
    public AccessDeniedException(string message = "Access denied.")
        : base(message)
    {
    }
}
