namespace HRMS.Helpers;

/// <summary>
/// All domain-level custom exceptions. The ExceptionHandlingMiddleware maps
/// these to the correct HTTP status codes via RFC 7807 ProblemDetails.
/// </summary>

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message) { }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message = "The resource was modified by another user. Please reload and try again.")
        : base(message) { }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Authentication is required.") : base(message) { }
}
