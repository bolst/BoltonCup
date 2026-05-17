using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Mapping;

public class TournamentRegistrationMapperTests
{
    private readonly TournamentRegistrationMapper _mapper = new(Mock.Of<IAssetUrlResolver>());

    [Fact]
    public void ToDto_NullRegistration_ReturnsNull()
    {
        _mapper.ToDto(null).Should().BeNull();
    }

    [Fact]
    public void ToDto_ValidRegistration_MapsAllFields()
    {
        var registration = new TournamentRegistration
        {
            CurrentStep = 3,
            Payload = "{\"position\":\"skater\"}",
            IsComplete = true
        };

        var dto = _mapper.ToDto(registration);

        dto.Should().NotBeNull();
        dto!.CurrentStep.Should().Be(3);
        dto.Payload.Should().Be("{\"position\":\"skater\"}");
        dto.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void ToDto_NullPayload_MapsNullPayload()
    {
        var registration = new TournamentRegistration
        {
            CurrentStep = 0,
            Payload = null,
            IsComplete = false
        };

        var dto = _mapper.ToDto(registration);

        dto.Should().NotBeNull();
        dto!.Payload.Should().BeNull();
    }
}
