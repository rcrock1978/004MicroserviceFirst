namespace SaaSCommon.Domain;

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T value)
    {
        _value = value;
        _error = null;
    }

    private Result(Error error)
    {
        _value = default;
        _error = error;
    }

    public bool IsSuccess => _error is null;
    public bool IsFailure => _error is not null;

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value on a failed Result.");
    public Error Error => IsFailure ? _error! : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(_value!) : onFailure(_error!);
    }

    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        return IsSuccess ? func(_value!) : Result<TNext>.Failure(_error!);
    }

    public Result<TNext> Map<TNext>(Func<T, TNext> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        return IsSuccess ? Result<TNext>.Success(func(_value!)) : Result<TNext>.Failure(_error!);
    }
}

public static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    public static Result<object> Success() => Result<object>.Success(new object());
    public static Result<object> Failure(Error error) => Result<object>.Failure(error);
}
