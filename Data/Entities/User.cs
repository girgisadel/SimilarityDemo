using Microsoft.AspNetCore.Identity;

namespace SimilarityDemo.Data.Entities;

public class User : IdentityUser<long>
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}
