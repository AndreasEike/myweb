namespace myweb.api.Models;

public class Result<T>
{
    public T? Value { get; private init; }
    public string? Error { get; private init; }
    public int Status { get; private init; }
    public bool Ok => Error == null;

    public static Result<T> Success(T value) => new() { Value = value, Status = 200 };
    public static Result<T> Fail(string error, int status = 400) => new() { Error = error, Status = status };
}
