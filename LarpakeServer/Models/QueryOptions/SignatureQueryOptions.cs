﻿namespace LarpakeServer.Models.QueryOptions;

public class SignatureQueryOptions : QueryOptions
{
    public Guid? UserId { get; set; }

    public Guid[]? SignatureIds { get; set; }
}
