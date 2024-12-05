namespace LarpakeServer.Helpers;

public class ExtendedControllerBase : ControllerBase
{
    protected ObjectResult FromError<T>(Result<T> error)
    {
        var (statusCode, message) = (Error)error;
        return StatusCode(statusCode, message);
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

}
