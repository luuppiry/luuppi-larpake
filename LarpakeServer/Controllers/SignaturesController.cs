using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.ComplexDataTypes;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.QueryOptions;
using SignaturesGetDto = LarpakeServer.Models.GetDtos.Templates.QueryDataGetDto<LarpakeServer.Models.GetDtos.SignatureGetDto>;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/signatures")]
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
    [ProducesResponseType<SignaturesGetDto>(200)]
    public async Task<IActionResult> GetSignatures([FromQuery] SignatureQueryOptions options)
    {
        var records = await _db.Get(options);

        // Map to result
        SignaturesGetDto result = SignaturesGetDto
            .MapFrom(records)
            .AppendPaging(options);

        return Ok(result);
    }

    [HttpGet("own")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType<SignaturesGetDto>(200)]
    public async Task<IActionResult> GetOwnSignatures([FromQuery] QueryOptions options)
    {
        return await GetSignatures(new SignatureQueryOptions
        {
            UserId = GetRequestUserId(),
            PageOffset = options.PageOffset,
            PageSize = options.PageSize
        });
    }

    [HttpGet("{signatureId}")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(SignatureGetDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSignature(Guid signatureId)
    {
        Signature? record = await _db.Get(signatureId);
        if (record is null)
        {
            return IdNotFound();
        }
        SignatureGetDto result = SignatureGetDto.From(record);
        return Ok(result);
    }


    [HttpPost]
    [RequiresPermissions(Permissions.Admin)]
    [ProducesResponseType<GuidIdResponse>(201)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
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
        return id.IsOk ? CreatedId((Guid)id) : FromError(id);
    }

    [HttpPost("own")]
    [RequiresPermissions(Permissions.CreateSignature)]
    [ProducesResponseType<GuidIdResponse>(201)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> PostOwnSignature([FromBody] SvgMetadata signature)
    {
        return await PostSignature(new SignaturePostDto
        {
            OwnerId = GetRequestUserId(),
            Signature = signature
        });
    }

    [HttpDelete("{signatureId}")]
    [RequiresPermissions(Permissions.CreateSignature)]
    [ProducesResponseType<RowsAffectedResponse>(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> DeleteSignature(Guid signatureId)
    {
        // Validate only admins or signature owner can delete
        var isValid = await RequireOwnerOrAdmin(signatureId);
        if (isValid.IsError)
        {
            return FromError(isValid);
        }

        Result<int> result = await _db.Delete(signatureId);
        return result.IsOk ? OkRowsAffected(result) : FromError(result);
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
            return Error.NotFound("Id not found", ErrorCode.IdNotFound);
        }
        if (userId != signature?.UserId)
        {
            // Not owner
            return Error.Unauthorized("Must be admin or signature owner.", ErrorCode.RequiresHigherRole);
        }
        // Is owner
        return true;
    }


}
