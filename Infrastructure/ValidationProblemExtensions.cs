using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SimilarityDemo.Infrastructure;

public static class ValidationProblemExtensions
{
    public static ValidationProblem AsValidationProblem(this ValidationResult result)
    {
        var groupedErrors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.ToList());

        var errorDictionary = new Dictionary<string, string[]>();

        foreach (var group in groupedErrors)
        {
            var errors = group.Value.Select(e => e.ErrorMessage).ToArray();
            errorDictionary.Add(group.Key, errors);
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }
}
