using System.Diagnostics.CodeAnalysis;

namespace BoltonCup.SessionStorage;

[ExcludeFromCodeCoverage]
public class ChangedEventArgs
{
    public string Key { get; set; }
    public object OldValue { get; set; }
    public object NewValue { get; set; }
}