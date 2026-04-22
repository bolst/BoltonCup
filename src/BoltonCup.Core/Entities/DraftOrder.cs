namespace BoltonCup.Core;

public class DraftOrder : EntityBase
{
    public int Id { get; set; }
    public int DraftId { get; set; }
    public int TeamId { get; set; }
    public int Pick { get; set; }

    public Draft Draft { get; set; } = null!;
    public Team Team { get; set; } = null!;
}