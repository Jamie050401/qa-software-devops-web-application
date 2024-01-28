namespace Application.Common;

public enum ResponseStatus
{
    Success,
    Error
}

public class Response<TValue, TError>(ResponseStatus status)
{
    public ResponseStatus Status { get; } = status;
    public TValue? Value { get; set; }
    public bool HasValue => Value is not null;
    public List<TError> Errors { get; set; } = new();

    public void AddError(TError error)
    {
        Errors.Add(error);
    }

    public static Response<TValue, TError> OkResponse()
    {
        return new Response<TValue, TError>(ResponseStatus.Success);
    }

    public static Response<TValue, TError> OkValueResponse(TValue value)
    {
        return new Response<TValue, TError>(ResponseStatus.Success)
        {
            Value = value
        };
    }

    public static Response<TValue, Error> BadRequestResponse()
    {
        var response = new Response<TValue, Error>(ResponseStatus.Error);
        response.AddError(new Error(400)
        {
            ErrorMessage = "Bad request"
        });
        return response;
    }

    public static Response<TValue, Error> NotFoundResponse()
    {
        var response = new Response<TValue, Error>(ResponseStatus.Error);
        response.AddError(new Error(404)
        {
            ErrorMessage = "Not found"
        });
        return response;
    }
}