using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.WebAPI.Mapping;
using FluentAssertions;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Mapping;

public class DraftMapperTests
{
    private readonly Mock<IAssetUrlResolver> _urlResolver = new();
    private readonly Mapper _mapper;

    public DraftMapperTests()
    {
        _mapper = new Mapper(_urlResolver.Object);
    }

    // ---------- ToQuery ----------

    [Fact]
    public void ToQuery_MapsAllFields()
    {
        var request = new GetDraftsRequest
        {
            TournamentId = 3,
            Status = DraftStatus.InProgress
        };

        var query = _mapper.ToQuery(request);

        query.TournamentId.Should().Be(3);
        query.Status.Should().Be(DraftStatus.InProgress);
    }

    // ---------- ToDto (Draft, isAuthorized) ----------

    [Fact]
    public void ToDto_NullDraft_ReturnsNull()
    {
        _mapper.ToDto((Draft?)null, isAuthorized: true).Should().BeNull();
    }

    // ---------- ToCreateDraftCommand ----------

    [Fact]
    public void ToCreateDraftCommand_MapsAllFields()
    {
        var request = new CreateDraftRequest
        {
            TournamentId = 4,
            Title = "Championship Draft"
        };

        var command = _mapper.ToCommand(request);

        command.TournamentId.Should().Be(4);
        command.Title.Should().Be("Championship Draft");
    }

    // ---------- ToUpdateDraftCommand ----------

    [Fact]
    public void ToUpdateDraftCommand_MapsAllFields()
    {
        var request = new UpdateDraftRequest
        {
            Title = "Updated Title",
            DraftType = DraftType.Snake,
            Ordering =
            [
                new DraftOrderingRequestEntry(1, 1),
                new DraftOrderingRequestEntry(2, 2)
            ]
        };

        var command = _mapper.ToCommand(request);

        command.Title.Should().Be("Updated Title");
        command.DraftType.Should().Be(DraftType.Snake);
        command.Ordering.Should().HaveCount(2);
        command.Ordering![0].TeamId.Should().Be(1);
        command.Ordering![1].TeamId.Should().Be(2);
    }

    [Fact]
    public void ToUpdateDraftCommand_NullOrdering_ProducesNullOrdering()
    {
        var request = new UpdateDraftRequest { Title = "Draft", Ordering = null };

        var command = _mapper.ToCommand(request);

        command.Ordering.Should().BeNull();
    }

    // ---------- ToDraftPlayerCommand ----------

    [Fact]
    public void ToDraftPlayerCommand_MapsAllFields()
    {
        var request = new DraftPlayerRequest
        {
            PlayerId = 10,
            TeamId = 3,
            OverallPick = 7
        };

        var command = _mapper.ToCommand(5, request);

        command.DraftId.Should().Be(5);
        command.PlayerId.Should().Be(10);
        command.TeamId.Should().Be(3);
        command.OverallPick.Should().Be(7);
    }

    // ---------- ToDto (DraftPick) ----------

    [Fact]
    public void ToDraftPickSingleDto_NullPick_ReturnsNull()
    {
        _mapper.ToDto((DraftPick?)null).Should().BeNull();
    }
}
