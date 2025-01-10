using LarpakeServer.Extensions;
using LarpakeServer.Models.GetDtos.Templates;
using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Models.GetDtos;

public class LeadersGetDto<T> : GetDtoBase
{
    public LeadersGetDto() { }

    [SetsRequiredMembers]
    public LeadersGetDto(T[] groups, QueryOptions.QueryOptions options)
    {
        Data = groups;
        this.SetNextPaginationPage(options);
    }

    public required T[] Data { get; init; }
    public override int ItemCount => Data.Length;
}
