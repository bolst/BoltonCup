using BoltonCup.Core;
using FluentAssertions;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Bolton Cup 2026 Preview", "bolton-cup-2026-preview")]
    [InlineData("  Leading & trailing!!  ", "leading-trailing")]
    [InlineData("Émile's Réunion Café", "emile-s-reunion-cafe")]
    [InlineData("Game   Highlights -- Finals", "game-highlights-finals")]
    [InlineData("ALREADY-A-SLUG", "already-a-slug")]
    public void Generate_ProducesLowercaseHyphenatedAscii(string input, string expected)
    {
        SlugGenerator.Generate(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public void Generate_FallsBackWhenNothingUsable(string? input)
    {
        SlugGenerator.Generate(input).Should().Be("post");
    }

    [Fact]
    public void Generate_CapsLengthWithoutTrailingHyphen()
    {
        var slug = SlugGenerator.Generate(string.Join(' ', Enumerable.Repeat("word", 40)));

        slug.Length.Should().BeLessThanOrEqualTo(80);
        slug.Should().NotEndWith("-");
    }

    [Fact]
    public void WithSuffix_AppendsNumber()
    {
        SlugGenerator.WithSuffix("finals-recap", 2).Should().Be("finals-recap-2");
    }
}
