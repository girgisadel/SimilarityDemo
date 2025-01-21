using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimilarityDemo.Data.Entities;

namespace SimilarityDemo.Data.Contexts;

public class IdentityDatabase(DbContextOptions<IdentityDatabase> options)
    : IdentityDbContext<User, Role, long>(options) { }
