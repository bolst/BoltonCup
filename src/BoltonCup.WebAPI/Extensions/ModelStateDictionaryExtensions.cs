using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BoltonCup.WebAPI.Extensions;


public static class ModelStateDictionaryExtensions
{
    public static IDictionary<string, string[]> ToErrorDictionary(this ModelStateDictionary modelState)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        var errorDictionary = new Dictionary<string, string[]>(StringComparer.Ordinal);

        foreach (var keyModelStatePair in modelState)
        {
            var key = keyModelStatePair.Key;
            var errors = keyModelStatePair.Value.Errors;
            if (errors is { Count: > 0 })
            {
                if (errors.Count == 1)
                {
                    var errorMessage = GetErrorMessage(errors[0]);
                    errorDictionary.Add(key, [errorMessage]);
                }
                else
                {
                    var errorMessages = new string[errors.Count];
                    for (var i = 0; i < errors.Count; i++)
                    {
                        errorMessages[i] = GetErrorMessage(errors[i]);
                    }

                    errorDictionary.Add(key, errorMessages);
                }
            }
        }

        return errorDictionary;

        static string GetErrorMessage(ModelError error)
        {
            return string.IsNullOrEmpty(error.ErrorMessage) 
                ? "unknown error" 
                : error.ErrorMessage;
        }
    }

}