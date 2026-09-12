using System.Reflection;
using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;



/// <summary>
/// Discovers every mapped tag table from the EF model, so a newly taggable subject shows up
/// here as soon as it is configured, with no change to this class or its callers.
/// </summary>
class TagUsageService(IDbContextFactory<BoltonCupDbContext> _dbContextFactory) : ITagUsageService
{
    static readonly MethodInfo LoadMethod = typeof(TagUsageService)
        .GetMethod(nameof(LoadAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public async Task<IReadOnlyList<TagUsageGroup>> GetLabelUsageAsync(int labelId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var groups = new List<TagUsageGroup>();

        foreach (var (tagType, subjectType) in TagTypes(db))
        {
            var task = (Task<List<string>>)LoadMethod
                .MakeGenericMethod(tagType, subjectType)
                .Invoke(null, [db, labelId, cancellationToken])!;

            var items = await task;
            if (items.Count > 0)
            {
                groups.Add(new TagUsageGroup(Pluralize(subjectType.Name), items));
            }
        }

        return groups.OrderBy(g => g.SubjectName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Mapped entity types deriving from EntityTag, paired with the subject each one tags.</summary>
    static IEnumerable<(Type TagType, Type SubjectType)> TagTypes(BoltonCupDbContext db) => db.Model
        .GetEntityTypes()
        .Select(e => e.ClrType)
        .Where(t => typeof(EntityTag).IsAssignableFrom(t) && !t.IsAbstract)
        .Distinct()
        .Select(t => (TagType: t, SubjectType: SubjectTypeOf(t)))
        .Where(x => x.SubjectType is not null)
        .Select(x => (x.TagType, x.SubjectType!));

    static Type? SubjectTypeOf(Type tagType)
    {
        for (var t = tagType; t is not null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(EntityTag<>))
            {
                return t.GetGenericArguments()[0];
            }
        }

        return null;
    }

    static async Task<List<string>> LoadAsync<TTag, TSubject>(BoltonCupDbContext db, int labelId, CancellationToken cancellationToken)
        where TTag : EntityTag<TSubject>
        where TSubject : class
    {
        var subjects = await db.Set<TTag>()
            .AsNoTracking()
            .Where(t => t.LabelId == labelId)
            .Select(t => t.Subject)
            .ToListAsync(cancellationToken);

        return subjects
            .Select(s => s.ToString() ?? string.Empty)
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    static string Pluralize(string name) => name.EndsWith('s') ? name : $"{name}s";
}
