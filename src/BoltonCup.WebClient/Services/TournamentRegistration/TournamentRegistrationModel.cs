using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BoltonCup.Core.Values;
using static BoltonCup.Core.Values.Position;

namespace BoltonCup.WebClient.Services;


public class TournamentRegistrationModel
{
    public required UserInfoModel UserInfo { get; set; }
    public required DocumentModel Documents { get; set; }
}


public class UserInfoModel
{
    [Display(Name = "First name")]
    [ReadOnly(true)]
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Display(Name = "Last name")]
    [ReadOnly(true)]
    [Required]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [ReadOnly(true)]
    [DataType(DataType.EmailAddress)]
    [Required]
    public string Email { get; set; } = string.Empty;
    
    [Display(Name = "Highest level played")]
    [ReadOnly(true)]
    [Required]
    public string? HighestLevel { get; set; }
    
    [Display(Name = "Phone")]
    [DataType(DataType.PhoneNumber)]
    [Required]
    [StringLength(10, MinimumLength = 10,  ErrorMessage = "Please enter a valid phone number.")]
    public string Phone { get; set; } = string.Empty;
    
    [Display(Name = "Jersey size")]
    [AllowedValues("S", "M", "L", "XL", "XXL")]
    [Required]
    public string? JerseySize { get; set; }
    
    [Display(Name = "Position")]
    [AllowedValues(Forward, Defense)]
    [Required]
    public string? Position { get; set; }

    [Display(Name = "I can play either position")]
    public bool CanPlayEitherPosition { get; set; }
    
    [Display(Name = "List any players you want to be on a team with")]
    [Description("We can't guarantee you will be drafted with them, but we will try to make it work.")]
    public string? Friends { get; set; }
}

public class DocumentModel
{
    [Display(Name = "I have read, understand and agree to the terms and conditions of the code of conduct.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree in order to register and play.")]
    public bool HasAgreedToCodeOfConductWaiver { get; set; }
    
    // [Display(Name = "I have read, understand and agree to the terms and conditions of the liability waiver.")]
    // [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree in order to register and play.")]
    // public bool HasAgreedToLiabilityWaiver { get; set; }
    
    [Display(Name = "I have read, understand and agree to the terms and conditions of the concussion waiver.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree in order to register and play.")]
    public bool HasAgreedToConcussionWaiver { get; set; }

    [Display(Name = "I have read, understand and agree to the terms and conditions of the communication consent form.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree in order to register and play.")]
    public bool HasAgreedToCommunicationConsent { get; set; }
}