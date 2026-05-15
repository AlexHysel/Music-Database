record Result(bool Success, string? Message)
{
    public static Result Ok() => new(true, null);
    public static Result Fail(string message) => new(false, message);
}

record Result<T>(bool Success, string? Message, T? Data) : Result(Success, Message)
{
    public static Result<T> Ok(T data) => new(true, null, data);
    public static new Result<T> Fail(string message) => new(false, message, default);
}