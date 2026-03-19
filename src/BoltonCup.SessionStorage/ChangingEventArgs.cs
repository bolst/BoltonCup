using System.Diagnostics.CodeAnalysis;

namespace BoltonCup.SessionStorage;

[ExcludeFromCodeCoverage]
public class ChangingEventArgs : ChangedEventArgs
{
    public bool Cancel { get; set; }
}