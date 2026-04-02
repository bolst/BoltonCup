using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components.Forms;

namespace BoltonCup.Common;

public static class ValidationMessageStoreExtensions
{

    /// <summary>
    /// Don't forget to notify that validation state has changed!
    /// </summary>
    public static void AddApiErrorResponse<T>(this ValidationMessageStore messageStore, T model, BoltonCupValidationProblemDetails validationProblemDetails)
    {
        if (model is null)
            return;

        if (validationProblemDetails.Errors is not null)
        {
            foreach (var (fieldName, fieldErrors) in validationProblemDetails.Errors)
            {
                var identifier = new FieldIdentifier(model, fieldName);
                messageStore.Add(identifier, fieldErrors);
            }
        }
    }
    
}