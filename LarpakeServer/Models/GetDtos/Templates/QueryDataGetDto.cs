using LarpakeServer.Extensions;
using LarpakeServer.Models.DatabaseModels;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos.Templates;

internal class QueryDataGetDto<T> : GetDtoBase
{
    public required T[] Data { get; set; }

    [JsonIgnore]
    public override int ItemCount => Data.Length;

    private static QueryDataGetDto<TResult> MapFrom<TArg, TResult>(TArg[] records)
        where TResult : IMappable<TArg, TResult>
    {
        // Map array of TArg to MultipleDataGetDto<TResult>
        return new QueryDataGetDto<TResult>
        {
            Data = records.Select(TResult.From).ToArray()
        };
    }

    public QueryDataGetDto<T> AppendPaging(QueryOptions.QueryOptions options)
    {
        this.SetNextPaginationPage(options);
        return this;
    }

    public static QueryDataGetDto<SignatureGetDto> MapFrom(Signature[] records)
    {
        return MapFrom<Signature, SignatureGetDto>(records);
    }
    
    public static QueryDataGetDto<OrganizationEventGetDto> MapFrom(OrganizationEvent[] records)
    {
        return MapFrom<OrganizationEvent, OrganizationEventGetDto>(records);
    }
    
    public static QueryDataGetDto<LarpakeEventGetDto> MapFrom(LarpakeTask[] records)
    {
        return MapFrom<LarpakeTask, LarpakeEventGetDto>(records);
    }
    
    public static QueryDataGetDto<LarpakeGetDto> MapFrom(Larpake[] records)
    {
        return MapFrom<Larpake, LarpakeGetDto>(records);
    }
    
    public static QueryDataGetDto<UserGetDto> MapFrom(User[] records)
    {
        return MapFrom<User, UserGetDto>(records);
    }
    
    public static QueryDataGetDto<AttendanceGetDto> MapFrom(Attendance[] records)
    {
        return MapFrom<Attendance, AttendanceGetDto>(records);
    }
    
    public static QueryDataGetDto<FreshmanGroupGetDto> MapFrom(FreshmanGroup[] records)
    {
        return MapFrom<FreshmanGroup, FreshmanGroupGetDto>(records);
    }
    
    public static QueryDataGetDto<TType> MapFrom<TType>(TType[] records)
    {
        return new QueryDataGetDto<TType>
        {
            Data = records
        };
    }

    
}
