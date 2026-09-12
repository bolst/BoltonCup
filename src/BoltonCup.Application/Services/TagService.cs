using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BoltonCup.Application.Services;

// A context per call, not a scoped one: callers include Blazor Server components that fire
// overlapping operations from key events, which a circuit-scoped context cannot serve.
class TagService<TTag>(IDbContextFactory<BoltonCupDbContext> _dbContextFactory) : ITagService<TTag>
    where TTag : EntityTag, new()
{
    const string UniqueViolation = "23505";

    public async Task<TTag> AddTagAsync(int subjectId, TagTargetType type, int targetId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await FindAsync(db, subjectId, type, targetId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var tag = new TTag { SubjectId = subjectId };
        TagTargets.SetTargetId(tag, type, targetId);

        db.Set<TTag>().Add(tag);
        await db.SaveChangesAsync(cancellationToken);

        db.Entry(tag).State = EntityState.Detached;
        return tag;
    }

    public async Task<TTag> AddLabelTagAsync(int subjectId, string labelName, CancellationToken cancellationToken = default)
    {
        var name = labelName.Trim();
        if (name.Length == 0)
        {
            throw new ArgumentException("A label needs a name.", nameof(labelName));
        }

        for (var attempt = 0; ; attempt++)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var label = await FindLabelAsync(db, name, cancellationToken);
            if (label is not null)
            {
                var already = await FindAsync(db, subjectId, TagTargetType.Label, label.Id, cancellationToken);
                if (already is not null)
                {
                    already.Label = label;
                    return already;
                }
            }

            var tag = new TTag { SubjectId = subjectId };
            if (label is not null)
            {
                tag.LabelId = label.Id;
            }
            else
            {
                // Assigning the navigation lets one SaveChanges insert the label and the tag
                // together: if either fails, neither is written.
                tag.Label = new TagLabel { Name = name };
            }

            db.Set<TTag>().Add(tag);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException e)
                when (attempt == 0 && e.InnerException is PostgresException { SqlState: UniqueViolation })
            {
                // Another caller created the same label or the same tag first. Re-resolve once.
                continue;
            }

            tag.Label ??= label;
            db.Entry(tag).State = EntityState.Detached;
            return tag;
        }
    }

    // Matches the unique index on lower(name), so this seeks rather than scans and the
    // case-insensitivity is the same one the database enforces.
    static Task<TagLabel?> FindLabelAsync(BoltonCupDbContext db, string name, CancellationToken cancellationToken)
        => db.TagLabels
            .Where(l => l.Name.ToLower() == name.ToLower())
            .OrderBy(l => l.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task RemoveTagAsync(int tagId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var tag = await db.Set<TTag>().FirstOrDefaultAsync(t => t.Id == tagId, cancellationToken);
        if (tag is null)
        {
            return;
        }

        db.Set<TTag>().Remove(tag);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Guards against a duplicate racing in from another tab; the unique index is the real backstop.</summary>
    static async Task<TTag?> FindAsync(BoltonCupDbContext db, int subjectId, TagTargetType type, int targetId, CancellationToken cancellationToken)
    {
        var candidates = await db.Set<TTag>()
            .AsNoTracking()
            .Where(t => t.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault(t => TagTargets.GetTargetId(t, type) == targetId);
    }
}
