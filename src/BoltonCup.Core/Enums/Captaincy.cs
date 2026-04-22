using System.Runtime.Serialization;

namespace BoltonCup.Core;

public enum Captaincy
{
    [EnumMember(Value = "")]
    None,
    
    [EnumMember(Value = "captain")]
    Captain,
    
    [EnumMember(Value = "alternate")]
    Alternate,
}