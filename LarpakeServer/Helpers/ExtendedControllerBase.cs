using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace LarpakeServer.Helpers;

public class ExtendedControllerBase : ControllerBase
{
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
        return Ok(new { RowsAffected = rowsAffected });
    }

    protected ObjectResult CreatedId(long id)
    {
        string? resourceUrl = $"{Request.Path.Value}/{id}";
        return Created(resourceUrl, new { Id = id });
    }

    protected ObjectResult CreatedId(Guid id)
    {
        string? resourceUrl = $"{Request.Path.Value}/{id}";
        return Created(resourceUrl, new { Id = id });
    }

    protected ObjectResult IdNotFound()
    {
        return NotFound(new { Message = "Id not found." });
    }
    
    protected ObjectResult IdNotFound(string message)
    {
        return NotFound(new
        {
            Message = "Id not found.",
            Details = message
        });
    }

    protected ObjectResult InvalidJWT(string message)
    {
        return BadRequest(new
        {
            Message = "Invalid JWT token.",
            Details = message
        });
    }

    protected ObjectResult InternalServerError(string message)
    {
        return StatusCode(500, new { Message = message });
    }

    protected ObjectResult BadRequest(string message, string? details = null)
    {
        return StatusCode(400, new
        {
            Message = message,
            Details = details
        });
    }

    protected ObjectResult Unauthorized(string message)
    {
        return Unauthorized(new
        {
            Message = message,
        });
    }
    protected ObjectResult BadRequest(string message)
    {
        return NotFound(new { Message = message });
    }


    private ObjectResult FromError(Error error)
    {
        _logger.LogInformation("Handled request error: {error}", error);

        if (error is DataError dataError)
        {
            return StatusCode(dataError.StatusCode, new
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
        var (statusCode, message) = (Error)error;
        return StatusCode(statusCode, message);
#endif
    }
}
