using System.Diagnostics.CodeAnalysis;

namespace BoltonCup.SessionStorage;

[ExcludeFromCodeCoverage]
public class ChangedEventArgs
{
    public string Key { get; set; } = null!;
    public object OldValue { get; set; } = null!;
    public object NewValue { get; set; } = null!;
}