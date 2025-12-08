using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Models;
using Npgsql;
using Dapper;

namespace BoltonCup.WebAPI.Repositories;

public class TeamRepository : ITeamRepository
{
    // private readonly ILogger<TeamRepository> _logger;
    private readonly string _connectionString;

    public TeamRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetValue<string>(ConfigurationPaths.ConnectionString) 
                            ?? throw new Exception("Connection string is null");
    }

    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        const string sql = @"select
                              id as Id,
                              name as Name,
                              name_short as NameShort,
                              abbreviation as Abbreviation,
                              tournament_id as TournamentId,
                              logo_url as LogoUrl,
                              banner_url as BannerUrl,
                              primary_hex as PrimaryHex,
                              secondary_hex as SecondaryHex,
                              tertiary_hex as TertiaryHex,
                              goal_song_url as GoalSongUrl,
                              penalty_song_url as PenaltySongUrl
                            from
                              core.teams
                            order by
                              tournament_id,
                              id";
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Team>(sql);
    }
    
    public async Task<IEnumerable<Team>> GetAllAsync(int tournamentId)
    {
        const string sql = @"select
                              id as Id,
                              name as Name,
                              name_short as NameShort,
                              abbreviation as Abbreviation,
                              tournament_id as TournamentId,
                              logo_url as LogoUrl,
                              banner_url as BannerUrl,
                              primary_hex as PrimaryHex,
                              secondary_hex as SecondaryHex,
                              tertiary_hex as TertiaryHex,
                              goal_song_url as GoalSongUrl,
                              penalty_song_url as PenaltySongUrl
                            from
                              core.teams
                            where
                              tournament_id = @TournamentId
                            order by
                              tournament_id,
                              id";
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Team>(sql, new { TournamentId = tournamentId });
    }

    public async Task<Team?> GetByIdAsync(int id)
    {
        const string sql = @"select
                              id as Id,
                              name as Name,
                              name_short as NameShort,
                              abbreviation as Abbreviation,
                              tournament_id as TournamentId,
                              logo_url as LogoUrl,
                              banner_url as BannerUrl,
                              primary_hex as PrimaryHex,
                              secondary_hex as SecondaryHex,
                              tertiary_hex as TertiaryHex,
                              goal_song_url as GoalSongUrl,
                              penalty_song_url as PenaltySongUrl
                            from
                              core.teams
                            where
                                id = @Id";
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Team>(sql, new { Id = id });
    }

    public Task<bool> AddAsync(Team entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Team entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}