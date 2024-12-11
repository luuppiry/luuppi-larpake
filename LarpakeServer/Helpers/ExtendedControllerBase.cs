namespace LarpakeServer.Helpers;

public class ExtendedControllerBase : ControllerBase
{
    protected ObjectResult FromError<T>(Result<T> error)
    {
#if DEBUG
        var (statusCode, message) = (Error)error;
        return StatusCode(statusCode, new { Message = message, Exception = error });
#else
        var (statusCode, message) = (Error)error;
        return StatusCode(statusCode, message);
#endif
    }
    
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

}
