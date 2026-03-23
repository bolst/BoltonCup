using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BoltonCup.Core.Values;
using static BoltonCup.Core.Values.Position;

namespace BoltonCup.WebClient.Services;

public class TournamentRegistrationModel
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
    
    [Display(Name = "Phone")]
    [ReadOnly(true)]
    [DataType(DataType.PhoneNumber)]
    [Required]
    public string Phone { get; set; } = string.Empty;
    
    [Display(Name = "Birthday")]
    [ReadOnly(true)]
    [Required]
    public DateTime Birthday { get; set; } = DateTime.Now;
    
    [Display(Name = "Highest level played")]
    [AllowedValues(SkillLevel.HouseLeague, SkillLevel.AA, SkillLevel.AAA, SkillLevel.JrC, SkillLevel.JrB, SkillLevel.JrA)]
    [Required]
    public string? HighestLevel { get; set; }
    
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
}