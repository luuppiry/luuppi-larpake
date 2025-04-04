using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace LarpakeServer.Helpers;

public class ExtendedControllerBase : ControllerBase
{
    protected record ErrorMessageResponse(string Message, string? Details, ErrorCode ApplicationError);
    protected record struct RowsAffectedResponse(int RowsAffected);
    protected record struct DataResponse<T>(T Data);
    protected record struct LongIdResponse(long Id);
    protected record struct GuidIdResponse(Guid Id);


    protected ILogger<ExtendedControllerBase> _logger;
    protected readonly IClaimsReader _claimsReader;

    public ExtendedControllerBase(IClaimsReader claimsReader, ILogger<ExtendedControllerBase>? logger = null)
    {
        _logger = logger ?? NullLogger<ExtendedControllerBase>.Instance;
        _claimsReader = claimsReader;
    }

    protected Guid GetRequestUserId() => _claimsReader.ReadAuthorizedUserId(Request);
    protected Permissions GetRequestPermissions() => _claimsReader.ReadAuthorizedUserPermissions(Request);
    protected int? GetRequestUserStartYear() => _claimsReader.ReadAuthorizedUserStartYear(Request);





    protected ObjectResult FromError(Result result) => FromError((Error)result);
    protected ObjectResult FromError<T>(Result<T> error) => FromError((Error)error);


    protected ObjectResult OkRowsAffected(int rowsAffected)
    {
        return Ok(new RowsAffectedResponse(rowsAffected));
    }

    protected ObjectResult OkRowsAffected(Result<int> rowsAffected)
    {
        return Ok(new RowsAffectedResponse((int)rowsAffected));
    }

    protected ObjectResult CreatedId(long id)
    {
        string? resourceUrl = $"{Request.Path.Value}/{id}";
        return Created(resourceUrl, new LongIdResponse(id));
    }


    protected ObjectResult OkData<T>(T Data)
    {
        return Ok(new DataResponse<T>(Data));
    }

    protected ObjectResult CreatedId(Guid id)
    {
        string? resourceUrl = $"{Request.Path.Value}/{id}";
        return Created(resourceUrl, new GuidIdResponse(id));
    }

    protected ObjectResult IdNotFound()
    {
        return NotFound(new ErrorMessageResponse("Id not found.", null, ErrorCode.IdNotFound));
    }

    protected ObjectResult IdNotFound(string message)
    {
        return NotFound(new ErrorMessageResponse("Id not found.", message, ErrorCode.IdNotFound));
    }

    protected ObjectResult InvalidJWT(string message)
    {
        return BadRequest(new ErrorMessageResponse("Invalid JWT token.", message, ErrorCode.InvalidJWT));
    }

    protected ObjectResult InternalServerError(string message, ErrorCode error = ErrorCode.UnknownServerError)
    {
        return StatusCode(500, new ErrorMessageResponse(message, null, error));
    }

    protected ObjectResult BadRequest(string message, string? details = null, ErrorCode error = ErrorCode.Undefined)
    {
        return StatusCode(400, new ErrorMessageResponse(message, details, error));
    }

    protected ObjectResult Unauthorized(string message, ErrorCode error = ErrorCode.Undefined)
    {
        return Unauthorized(new ErrorMessageResponse(message, null, error));
    }



    private ObjectResult FromError(Error error)
    {
        _logger.LogInformation("Handled request error: {error}", error);

        if (error is DataError dataError)
        {
            return StatusCode(dataError.HttpStatusCode, new
            {
                dataError.Message,
                Details = "",
                ApplicationError = error,
                dataError.Data,
                dataError.DataKind,
#if DEBUG
                Exception = error.Ex
#endif
            });
        }

#if DEBUG
        var (statusCode, message) = error;
        return StatusCode(statusCode,
            new
            {
                Message = message,
                Details = "",
                ApplicationError = error.ApplicationErrorCode,
                Exception = error.Ex
            });
#else
        var (statusCode, message) = error;
        return StatusCode(statusCode, new ErrorMessageResponse(message, null, error.ApplicationErrorCode));
#endif
    }
}
