using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SimilarityDemo.DTOs;
using SimilarityDemo.Infrastructure;
using SimilarityDemo.Services.UsersService;

namespace SimilarityDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(IUsersService usersService) : ControllerBase
{
    [HttpGet("is-username-available")]
    public async Task<IActionResult> IsUserNameAvailableAsync([FromQuery] UserNameRequest request, 
        CancellationToken cancellationToken)
    {
        var userNameRequestValidator = HttpContext.RequestServices.GetRequiredService<IValidator<UserNameRequest>>();
        var validationResult = await userNameRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.AsValidationProblem());
        }

        var s = await usersService.GetNameScoreAsync(request.UserName);

        return Ok(new UserNameResponse(await usersService.IsNameAvailableAsync(request.UserName)));
    }

    [HttpPost("username-report")]
    public async Task<IActionResult> GetUserNameReportAsync([FromBody] UserNameReportRequest request,
        CancellationToken cancellationToken)
    {
        var userNameRequestValidator = HttpContext.RequestServices.GetRequiredService<IValidator<UserNameReportRequest>>();
        var validationResult = await userNameRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.AsValidationProblem());
        }

        var isNameAvailable = await usersService.IsNameAvailableAsync(request.UserName);
        var nameScore = await usersService.GetNameScoreAsync(request.UserName);
        List<string> nameSuggestions = [];

        if (!isNameAvailable)
        {
            nameSuggestions = await usersService.GetNameSuggestionsAsync(request.UserName, request.FirstName, request.LastName, 5);
        }

        return Ok(new UserNameReportResponse(isNameAvailable, nameScore, nameSuggestions));
    }
}
