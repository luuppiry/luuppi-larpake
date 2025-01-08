using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class UserPutDto
{
    public int? StartYear { get; set; } = null;
}
