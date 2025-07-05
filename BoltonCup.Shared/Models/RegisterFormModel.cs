using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Shared.Data;

public class RegisterFormModel
{
    [Required(ErrorMessage="First name is required")] 
    public string FirstName { get; set; }
        
    [Required(ErrorMessage="Last name is required")] 
    public string LastName { get; set; }
        
    [Required(ErrorMessage="Email is required")] 
    public string Email { get; set; }
        
    [Required(ErrorMessage="Birthday is required")] 
    public DateTime? Birthday { get; set; }
        
    [Required(ErrorMessage="Position is required")] 
    public string Position { get; set; }
        
    [Required(ErrorMessage="This is required")] 
    public string HighestLevel { get; set; }    
    
    [Required(ErrorMessage="This is required")] 
    public string JerseySize { get; set; }
    
    public bool IsForward => Position == "forward";
    public bool IsDefense => Position == "defense";
    public bool IsGoalie => Position == "goalie";
}