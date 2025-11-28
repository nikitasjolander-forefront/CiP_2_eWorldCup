namespace CiPeWorldCup.Core.RailwayErrorHandling;

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new Result<T>(true, value, null);
    public static Result<T> Fail(string error) => new Result<T>(false, default, error);
}


public readonly struct Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    private Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    public static Result Ok() => new Result(true, null);
    public static Result Fail(string error) => new Result(false, error);
}
