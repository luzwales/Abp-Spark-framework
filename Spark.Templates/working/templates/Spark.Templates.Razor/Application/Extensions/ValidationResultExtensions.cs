using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Spark.Templates.Razor.Application.Extensions;

/// <summary>
/// Replaces the <c>AddToModelState</c> extension that was removed together with the
/// deprecated FluentValidation.AspNetCore package.
/// </summary>
public static class ValidationResultExtensions
{
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState, string prefix)
    {
        if (result.IsValid)
        {
            return;
        }

        foreach (var error in result.Errors)
        {
            var key = string.IsNullOrEmpty(prefix) ? error.PropertyName : $"{prefix}.{error.PropertyName}";
            modelState.AddModelError(key, error.ErrorMessage);
        }
    }
}
