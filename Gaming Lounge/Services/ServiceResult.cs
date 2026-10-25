namespace Gaming_Lounge.Services;

public sealed record ServiceResult(bool Succeeded, string Message)
{
    public static ServiceResult Success(string message) => new(true, message);
    public static ServiceResult Failure(string message) => new(false, message);
}

public sealed record ServiceResult<T>(bool Succeeded, string Message, T? Value = default)
{
    public static ServiceResult<T> Success(T value, string message = "") => new(true, message, value);
    public static ServiceResult<T> Failure(string message) => new(false, message);
}
