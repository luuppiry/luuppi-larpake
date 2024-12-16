using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SignaturesController : ExtendedControllerBase
{
    private readonly ISignatureDatabase _db;

    public SignaturesController(ISignatureDatabase db)
    {
        _db = db;
    }


    [HttpGet]
    public async Task<IActionResult> GetSignatures([FromQuery] SignatureQueryOptions options)
    {
        var records = await _db.Get(options);
        var result = SignaturesGetDto.MapFrom(records);
        result.CalculateNextPageFrom(options);
        return Ok(result);
    }

    [HttpGet("{signatureId}")]
    public async Task<IActionResult> GetSignature(Guid signatureId)
    {
        Signature? record = await _db.Get(signatureId);
        if (record is null)
        {
            return NotFound();
        }
        return Ok(SignatureGetDto.From(record));
    }


    [HttpPost] 
    public async Task<IActionResult> PostSignature([FromBody] SignaturePostDto dto)
    {
        Signature record = Signature.From(dto);
        Result<Guid> id = await _db.Insert(record);
        if (id)
        {
            return CreatedId((Guid)id);
        }
        return FromError(id);
    }

    [HttpDelete("{signatureId}")]
    public async Task<IActionResult> DeleteSignature(Guid signatureId)
    {
        Result<int> result = await _db.Delete(signatureId);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }



}
