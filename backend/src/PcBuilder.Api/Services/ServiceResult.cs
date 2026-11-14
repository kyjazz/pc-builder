namespace PcBuilder.Api.Services;

/// <summary>
/// Either a value or user-facing validation errors (field name -> Turkish messages).
/// Services return this instead of throwing for expected problems such as "part not found",
/// and controllers turn the errors into a 400 ValidationProblemDetails.
/// </summary>
public sealed record ServiceResult<T>(T? Value, IReadOnlyDictionary<string, string[]> Errors)
    where T : class
{
    public bool Succeeded => Errors.Count == 0;

    public static ServiceResult<T> Success(T value) => new(value, new Dictionary<string, string[]>());

    public static ServiceResult<T> Failure(IReadOnlyDictionary<string, string[]> errors) => new(null, errors);

    public static ServiceResult<T> Failure(string field, params string[] messages) =>
        new(null, new Dictionary<string, string[]> { [field] = messages });
}
