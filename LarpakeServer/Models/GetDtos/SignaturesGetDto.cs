using LarpakeServer.Models.DatabaseModels;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos;

public class SignaturesGetDto : IPageable
{
    public required SignatureGetDto[] Signatures { get; set; }
    public int NextPage { get; set; } = -1;

    [JsonIgnore]
    public int ItemCount => Signatures.Length;

    internal static SignaturesGetDto MapFrom(IEnumerable<Signature> records)
    {
        return new SignaturesGetDto
        {
            Signatures = records.Select(SignatureGetDto.From).ToArray(),
        };
    }
}
