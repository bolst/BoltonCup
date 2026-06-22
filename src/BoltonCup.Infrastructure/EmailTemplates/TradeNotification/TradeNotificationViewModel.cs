namespace BoltonCup.Infrastructure.EmailTemplates;

public class TradeNotificationViewModel : EmailAuthBaseViewModel
{
    public required string Heading { get; set; }
    public required string Intro { get; set; }
    public required string ProposingTeamName { get; set; }
    public required string ReceivingTeamName { get; set; }
    public required IReadOnlyList<string> PlayersFromProposing { get; set; }
    public required IReadOnlyList<string> PlayersFromReceiving { get; set; }
}
