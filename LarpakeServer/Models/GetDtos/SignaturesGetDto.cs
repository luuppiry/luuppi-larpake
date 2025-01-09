using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos;

public class SignaturesGetDto : GetDtoBase
{
    public required SignatureGetDto[] Signatures { get; set; }

    [JsonIgnore]
    public override int ItemCount => Signatures.Length;

    internal static SignaturesGetDto MapFrom(IEnumerable<Signature> records)
    {
        return new SignaturesGetDto
        {
            Signatures = records.Select(SignatureGetDto.From).ToArray(),
        };
    }
}
