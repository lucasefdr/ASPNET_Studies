using Studies.Domain.Errors;

namespace Studies.Domain.Shared;

public class Result 
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    public bool IsFailure => !IsSuccess;

    private readonly List<Error> _errors = [];
    public IReadOnlyCollection<Error> Errors => _errors;

    protected Result(bool isSuccess, IEnumerable<Error>? errors = null)
    {
        if (isSuccess && errors?.Any() == true)
            throw new InvalidOperationException("A successful result cannot contain errors.");

        if (!isSuccess && (errors == null || !errors.Any()))
            throw new InvalidOperationException("A failure result must contain at least one error.");

        IsSuccess = isSuccess;

        if (errors is not null)
            _errors.AddRange(errors);
    }

    // Métodos para resultados que não precisam de um retorno
    public static Result Success() => new(true);
    public static Result Failure(Error error) => new(false, [error]);
    public static Result Failure(IEnumerable<Error>? errors) => new(false, errors);

    // Métodos para resultados que precisam de um retorno
    public static Result<T> Success<T>(T value) => new(value);
    public static Result<T> Failure<T>(Error error) => new(default, false, [error]);
    public static Result<T> Failure<T>(IEnumerable<Error> errors) => new(default, false, errors);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T value) : base(true)
    {
        _value = value;
    }

    internal Result(T? value, bool isSuccess, IEnumerable<Error> errors) : base(isSuccess, errors)
    {
        _value = value;
    }

    public T Value
        => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of a failure result.");
}