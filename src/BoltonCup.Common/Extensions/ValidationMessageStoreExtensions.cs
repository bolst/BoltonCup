using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components.Forms;

namespace BoltonCup.Common;

public static class ValidationMessageStoreExtensions
{

    /// <summary>
    /// Don't forget to notify that validation state has changed!
    /// </summary>
    public static void AddApiErrorResponse<T>(this ValidationMessageStore messageStore, T model, ApiErrorResponse errorResponse)
    {
        if (model is null)
            return;
        
        foreach (var (fieldName, fieldErrors) in errorResponse.Errors)
        {
            var identifier = new FieldIdentifier(model, fieldName);
            messageStore.Add(identifier, fieldErrors);
        }
    }
    
}