using System.Runtime.Serialization;

namespace BoltonCup.Core;

public enum GameType
{
    [EnumMember(Value = "Round robin")]
    RoundRobin,
    
    [EnumMember(Value = "Quarter finals")]
    QuarterFinals,
    
    [EnumMember(Value = "Semis")]
    SemiFinals,
    
    [EnumMember(Value = "Finals (5th)")]
    FifthPlaceFinals,
    
    [EnumMember(Value = "Finals (3rd)")]
    ThirdPlaceFinals,
    
    [EnumMember(Value = "Finals")]
    Finals,
}