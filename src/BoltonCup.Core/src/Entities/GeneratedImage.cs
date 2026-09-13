namespace BoltonCup.Core;

public class GeneratedImage : EntityBase
{
    public int Id { get; set; }
    public required string StorageKey { get; set; }
    public required string TemplateKey { get; set; }
    public required string Label { get; set; }
    public required string ContentType { get; set; }
}