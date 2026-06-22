using System.Runtime.Serialization;

namespace BoltonCup.Core;

public enum GameAvailability
{
    [EnumMember(Value = "in")]
    In,

    [EnumMember(Value = "maybe")]
    Maybe,

    [EnumMember(Value = "out")]
    Out,
}
