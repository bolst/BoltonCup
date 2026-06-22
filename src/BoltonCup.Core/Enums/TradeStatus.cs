using System.Runtime.Serialization;

namespace BoltonCup.Core;

public enum TradeStatus
{
    [EnumMember(Value = "pending")]
    Pending,

    [EnumMember(Value = "accepted")]
    Accepted,

    [EnumMember(Value = "approved")]
    Approved,

    [EnumMember(Value = "declined")]
    Declined,

    [EnumMember(Value = "cancelled")]
    Cancelled,
}
