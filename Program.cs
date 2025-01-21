using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimilarityDemo.Data.Contexts;
using SimilarityDemo.Data.Entities;
using SimilarityDemo.DTOs;
using SimilarityDemo.Identity;
using SimilarityDemo.Infrastructure;
using SimilarityDemo.Services.SeedingService;
using SimilarityDemo.Services.UsersService;

namespace SimilarityDemo;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddSwaggerGen();

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.Configure<RouteOptions>(options =>
        {
            options.LowercaseQueryStrings = true;
            options.LowercaseUrls = true;
        });

        builder.Services
            .AddDbContext<IdentityDatabase>(options => options
            .UseSqlServer(builder.Configuration.GetConnectionString(nameof(IdentityDatabase))));

        builder.Services.AddIdentity<User, Role>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;
            options.Lockout.AllowedForNewUsers = true;
        })
            .AddEntityFrameworkStores<IdentityDatabase>()
            .AddUserManager<InternalUserManager<User>>()
            .AddUserStore<InternalUserStore<User>>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<ISeedingService, SeedingService>();
        builder.Services.AddScoped<IUsersService, UsersService>();
        builder.Services.AddValidatorsFromAssemblyContaining<UserNameRequestValidator>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            var identityDatabase = scope.ServiceProvider.GetRequiredService<IdentityDatabase>();
            await identityDatabase.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception has occurred while migrating the identity database.");
            throw;
        }

        if (app.Environment.IsDevelopment())
        {
            try
            {
                var seedingService = scope.ServiceProvider.GetRequiredService<ISeedingService>();
                await seedingService.SeedAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception has occurred while seeding the users table.");
                throw;
            }
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler();
        }

        app.Run();
    }
}
