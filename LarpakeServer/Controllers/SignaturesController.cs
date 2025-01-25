using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Helpers.Generic;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.MultipleItems;
using LarpakeServer.Models.GetDtos.SingleItem;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SignaturesController : ExtendedControllerBase
{
    readonly ISignatureDatabase _db;
    readonly int _signaturePointLimit;

    public SignaturesController(
        ISignatureDatabase db,
        ILogger<SignaturesController> logger,
        IConfiguration config,
        IClaimsReader claimsReader) : base(claimsReader, logger)
    {
        _db = db;
        _signaturePointLimit = config.GetValue<int>("Signature:PointLimit");
    }


    [HttpGet]
    [RequiresPermissions(Permissions.CommonRead)]
    public async Task<IActionResult> GetSignatures([FromQuery] SignatureQueryOptions options)
    {
        var records = await _db.Get(options);
        var result = SignaturesGetDto.MapFrom(records);
        result.SetNextPaginationPage(options);
        return Ok(result);
    }

    [HttpGet("{signatureId}")]
    [RequiresPermissions(Permissions.CommonRead)]
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
    [RequiresPermissions(Permissions.CreateSignature)]
    public async Task<IActionResult> PostSignature([FromBody] SignaturePostDto dto)
    {
        if (dto.Signature.CalculatePointsCount() > _signaturePointLimit)
        {
            Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
            _logger.LogInformation("User {userId} tried to load too large signature.", userId);
            return BadRequest($"Signature point limit ({_signaturePointLimit}) exceeded.");
        }

        Signature record = Signature.From(dto);
        Result<Guid> id = await _db.Insert(record);
        if (id)
        {
            return CreatedId((Guid)id);
        }
        return FromError(id);
    }

    [HttpDelete("{signatureId}")]
    [RequiresPermissions(Permissions.CreateSignature)]
    public async Task<IActionResult> DeleteSignature(Guid signatureId)
    {
        // Validate only admins or signature owner can delete
        var isValid = await RequireOwnerOrAdmin(signatureId);
        if (isValid.IsError)
        {
            return FromError(isValid);
        }

        Result<int> result = await _db.Delete(signatureId);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }


    private async Task<Result<bool>> RequireOwnerOrAdmin(Guid signatureId)
    {

        Permissions userPermissions = _claimsReader.ReadAuthorizedUserPermissions(Request);
        if (userPermissions.Has(Permissions.Admin))
        {
            // Is admin
            return true;
        }

        // Is not admin
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        Signature? signature = await _db.Get(signatureId);
        if (signature is null)
        {
            // Signature does not even exist
            return Error.NotFound("Id not found");
        }
        if (userId != signature?.UserId)
        {
            // Not owner
            return Error.Unauthorized("Must be admin or signature owner.");
        }
        // Is owner
        return true;
    }


}
