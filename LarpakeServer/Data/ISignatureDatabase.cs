using LarpakeServer.Helpers.Generic;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;

public interface ISignatureDatabase
{

    Task<Signature[]> Get(SignatureQueryOptions options);
    Task<Signature?> Get(Guid id);
    Task<Result<Guid>> Insert(Signature record);
    Task<Result<int>> Delete(Guid id);
}
