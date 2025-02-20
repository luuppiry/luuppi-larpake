using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace LarpakeServer.Helpers;

public class ExtendedControllerBase : ControllerBase
{
    protected record MessageResponse(string Message);
    protected record MessageDetailsResponse(string Message, string? Details);
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
        return NotFound(new MessageResponse("Id not found."));
    }

    protected ObjectResult IdNotFound(string message)
    {
        return NotFound(new MessageDetailsResponse("Id not found.", message));
    }

    protected ObjectResult InvalidJWT(string message)
    {
        return BadRequest(new MessageDetailsResponse("Invalid JWT token.", message));
    }

    protected ObjectResult InternalServerError(string message)
    {
        return StatusCode(500, new MessageResponse(message));
    }

    protected ObjectResult BadRequest(string message, string? details = null)
    {
        return StatusCode(400, new MessageDetailsResponse(message, details));
    }

    protected ObjectResult Unauthorized(string message)
    {
        return Unauthorized(new MessageResponse(message));
    }
    protected ObjectResult BadRequest(string message)
    {
        return BadRequest(new MessageResponse(message));
    }


    private ObjectResult FromError(Error error)
    {
        _logger.LogInformation("Handled request error: {error}", error);

        if (error is DataError dataError)
        {
            return StatusCode(dataError.HttpStatusCode, new
            {
                dataError.Message,
                dataError.Data,
                dataError.DataKind
            });
        }

#if DEBUG
        var (statusCode, message) = error;
        return StatusCode(statusCode, new { Message = message, Exception = error.Ex });
#else
        var (statusCode, message) = error;
        return StatusCode(statusCode, new MessageResponse(message));
#endif
    }
}
