namespace MusicDatabase.Contracts;

public enum ErrorType 
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}

public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static Error Failure(string code, string message) => new(ErrorType.Failure, code, message);
    public static Error Validation(string code, string message) => new(ErrorType.Validation, code, message);
    public static Error NotFound(string code, string message) => new(ErrorType.NotFound, code, message);
    public static Error Conflict(string code, string message) => new(ErrorType.Conflict, code, message);
    public static Error Unauthorized(string code, string message) => new(ErrorType.Unauthorized, code, message);
    public static Error Forbidden(string code, string message) => new(ErrorType.Forbidden, code, message);
}

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Ok() => new(true, null);
    static public Result Failure(string code, string message)
    {
        return new Result(false, Error.Failure(code, message));
    }
    
    static public Result Validation(string code, string message)
    {
        return new Result(false, Error.Validation(code, message));
    }
    
    static public Result NotFound(string code, string message)
    {
        return new Result(false, Error.NotFound(code, message));
    }
    
    static public Result Conflict(string code, string message)
    {
        return new Result(false, Error.Conflict(code, message));
    }
    
    static public Result Unauthorized(string code, string message)
    {
        return new Result(false, Error.Unauthorized(code, message));
    }
    
    static public Result Forbidden(string code, string message)
    {
        return new Result(false, Error.Forbidden(code, message));
    }
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, null) => _value = value;
    private Result(Error error) : base(false, error) { }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Attempt to read a value from a failed Result<>");

    public static Result<T> Ok(T value) => new(value);
    new static public Result<T> Failure(string code, string message)
    {
        return new Result<T>(Error.Failure(code, message));
    }
    
    new static public Result<T> Validation(string code, string message)
    {
        return new Result<T>(Error.Validation(code, message));
    }
    
    new static public Result<T> NotFound(string code, string message)
    {
        return new Result<T>(Error.NotFound(code, message));
    }
    
    new static public Result<T> Conflict(string code, string message)
    {
        return new Result<T>(Error.Conflict(code, message));
    }
    
    new static public Result<T> Unauthorized(string code, string message)
    {
        return new Result<T>(Error.Unauthorized(code, message));
    }
    
    new static public Result<T> Forbidden(string code, string message)
    {
        return new Result<T>(Error.Forbidden(code, message));
    }
}