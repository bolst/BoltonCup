using System.Linq.Expressions;
using BoltonCup.Core;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Data;

public static class TagModelBuilderExtensions
{
    /// <summary>
    /// Configures a concrete tag table. Every per-target detail comes from
    /// <see cref="TagTargets.All"/>, so a new target needs no change here.
    /// </summary>
    public static ModelBuilder ConfigureTagTable<TTag, TSubject>(
        this ModelBuilder modelBuilder,
        string tableName,
        string subjectColumnName,
        Expression<Func<TSubject, IEnumerable<TTag>?>> subjectTags)
        where TTag : EntityTag<TSubject>
        where TSubject : class
    {
        modelBuilder.Entity<TTag>(entity =>
        {
            entity.ToTable(tableName, t => t.HasCheckConstraint(
                $"CK_{tableName}_exactly_one_target",
                TagTargets.CheckConstraintSql));
            entity.HasKey(e => e.Id);

            entity
                .HasOne(e => e.Subject)
                .WithMany(subjectTags)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SubjectId);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.SubjectId).HasColumnName(subjectColumnName);

            foreach (var target in TagTargets.All)
            {
                // No inverse navigation: a target must not accumulate one collection per
                // taggable subject type.
                entity
                    .HasOne(target.ClrType, target.Navigation)
                    .WithMany()
                    .HasForeignKey(target.ForeignKey)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(target.ForeignKey);

                // One tag per (subject, target). Partial because Postgres treats NULLs as
                // distinct, so an unfiltered unique index would not constrain anything.
                entity
                    .HasIndex(nameof(EntityTag.SubjectId), target.ForeignKey)
                    .IsUnique()
                    .HasFilter($"{target.Column} IS NOT NULL");

                entity.Property(target.ForeignKey).HasColumnName(target.Column);
            }
        });

        return modelBuilder;
    }
}
