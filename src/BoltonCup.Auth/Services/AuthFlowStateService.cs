namespace BoltonCup.Auth.Services;

public class AuthFlowStateService
{
    public string? Email { get; private set; }
    
    public void Initialize(string email)
    {
        Email = email;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Email);
    }

    public void Reset()
    {
        Email = null;
    }
}