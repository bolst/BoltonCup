namespace BoltonCup.WebAPI.Handlers.Base;

public interface IAutoMappedExceptionHandler
{
    int StatusCode { get; }
    Type ResponseType { get; }
}