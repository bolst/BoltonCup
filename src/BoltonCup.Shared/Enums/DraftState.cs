using System.ComponentModel;

namespace BoltonCup.Shared.Data;

public enum DraftState
{
    [Description("inactive")]
    Inactive,
    
    [Description("live")]
    Live,

    [Description("complete")]
    Complete,
}