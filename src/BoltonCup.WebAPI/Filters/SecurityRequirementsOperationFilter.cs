using BoltonCup.WebAPI.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BoltonCup.WebAPI.Filters;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = 
            context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ||
            (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false);

        // if it is anonymous, don't add security
        if (hasAllowAnonymous)
            return; 
        
        // assume everything else requires the API Key or "Bearer" scheme.
        operation.Security = 
        [
            new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecuritySchemeReference(ApiKeyConstants.Scheme, context.Document), []
                }
            },
            new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", context.Document), []
                }
            },            
        ];
    }
}

// CfDJ8JPLY9WyNqtBpsTfTL89Z7_twArZx-nyVW4wvraXfjCH8u8KObCfUhwb598F9B9tUMbnuqA9aGLdQGMZc3H09_MN-VVF28GhXf4pX2apE-IF5uGMWKWFlHj1fcnw5eWp7xfDLB5KZgSehLoNAiAMNnrT8julYqUxcQub0iZ3H2oAv_ebXBXmyEhOBeQjbISKGnijDPJ95PV3u6oW7jlCs7AKZsWb4SqTNxZd3cGJGm_KEtuh6OezWEBjS6OHXnc--NB1_XeHrwZnnqEiKwe_IZqC00VkBKyUtGV4bM-SP7BihhEVfrrRRZ7pFaZ7e3wZZIdAPyUcDLrXePzwjjMw1XQRjEEnW0aODFEe4wMLcuNzGhkd5A1XwUbTmhDZTPlga68Nj2FSraATnq83oIHxuBut_clZwBMaNLhH383DmIX_yrzCka85Qeem2zmsdzN5JmKSuOK6hEB4CdkfCohuo-bpOCgvd7Wq-2OqhhpGEw-Gj0wxKV-uorMd8BqUkvdgeDLX8D7UgyP209wRc9WGB2J6J-KIDahfddxjP_0WP_SEt2DgS7I85KYoNiEJDjgYyt2IK35a0XCGGYmL0ALsnfu2qCdQc9_-qDVfs5aldU2KlRavkmt1bYoSJXkyJ9PUGBPcjC6fbTYGbZYA2F0xPtvvaYMw8YF2rqT5ZDA9PYZbB8_WULQINPlO2qHkiTgtKDp8gKVDVlqLwmErOKmeq5rJIwFIgLs4Zi4jh48GCvip