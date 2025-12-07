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
                              primary_color_hex as PrimaryColorHex,
                              secondary_color_hex as SecondaryColorHex,
                              tertiary_color_hex as TertiaryColorHex,
                              logo_url as LogoUrl,
                              tournament_id as TournamentId,
                              gm_account_id as GmAccountId,
                              banner_image as BannerImage,
                              goal_horn_url as GoalHornUrl,
                              penalty_song_url as PenaltySongUrl
                            from
                              team T
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
                              primary_color_hex as PrimaryColorHex,
                              secondary_color_hex as SecondaryColorHex,
                              tertiary_color_hex as TertiaryColorHex,
                              logo_url as LogoUrl,
                              tournament_id as TournamentId,
                              gm_account_id as GmAccountId,
                              banner_image as BannerImage,
                              goal_horn_url as GoalHornUrl,
                              penalty_song_url as PenaltySongUrl
                            from
                              team T
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
                              primary_color_hex as PrimaryColorHex,
                              secondary_color_hex as SecondaryColorHex,
                              tertiary_color_hex as TertiaryColorHex,
                              logo_url as LogoUrl,
                              tournament_id as TournamentId,
                              gm_account_id as GmAccountId,
                              banner_image as BannerImage,
                              goal_horn_url as GoalHornUrl,
                              penalty_song_url as PenaltySongUrl
                            from
                              team T
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