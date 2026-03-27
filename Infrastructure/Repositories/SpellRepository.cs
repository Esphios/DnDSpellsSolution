using ApplicationCore.Dtos;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SpellRepository(ApplicationDbContext dbContext) : ISpellRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<(IEnumerable<Spell> Spells, int TotalItems, int CurrentPage)> GetAllSpellsAsync(
        int page, int pageSize, string? filter, string? sortBy, string? sortDirection, CancellationToken cancellationToken)
    {
        IQueryable<Spell> query = _dbContext.Spells
            .Include(s => s.Classes)
            .Include(s => s.School)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(s => s.Name.Contains(filter));
        }

        query = ApplySorting(query, sortBy, sortDirection);

        int totalItems = await query.CountAsync(cancellationToken);
        List<Spell> spells = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (spells, totalItems, page);
    }

    private static IQueryable<Spell> ApplySorting(IQueryable<Spell> query, string? sortBy, string? sortDirection)
    {
        sortBy ??= "name";
        bool isAscending = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("asc", StringComparison.CurrentCultureIgnoreCase);

        query = sortBy.ToLower() switch
        {
            "name" => isAscending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
            "level" => isAscending ? query.OrderBy(s => s.Level) : query.OrderByDescending(s => s.Level),
            "class" => isAscending
                ? query.OrderBy(s => s.Classes.OrderBy(c => c.Name).Select(c => c.Name).FirstOrDefault() ?? string.Empty)
                : query.OrderByDescending(s => s.Classes.OrderBy(c => c.Name).Select(c => c.Name).FirstOrDefault() ?? string.Empty),
            _ => isAscending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
        };

        return query;
    }

    public async Task<Spell?> GetSpellByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _dbContext.Spells
            .Include(s => s.Classes)
            .Include(s => s.Subclasses)
            .Include(s => s.School)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddOrUpdateSpellAsync(SpellRequest request, CancellationToken cancellationToken)
    {
        Spell? existingSpell = await _dbContext.Spells
            .Include(s => s.Classes)
            .Include(s => s.Subclasses)
            .Include(s => s.School)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (existingSpell is null)
        {
            Spell newSpell = await CreateSpellAsync(request, cancellationToken);
            _ = _dbContext.Spells.Add(newSpell);
        }
        else
        {
            await UpdateSpellAsync(existingSpell, request, cancellationToken);
        }

        _ = await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Spell> CreateSpellAsync(SpellRequest request, CancellationToken cancellationToken)
    {
        Spell newSpell = MapSpell(request, new Spell { Id = request.Id });
        newSpell.School = await GetSchoolAsync(request.SchoolId, cancellationToken);
        newSpell.Classes = await GetClassesAsync(request.ClassIds, cancellationToken);
        newSpell.Subclasses = await GetSubclassesAsync(request.SubclassIds, cancellationToken);
        return newSpell;
    }

    private async Task UpdateSpellAsync(Spell existingSpell, SpellRequest request, CancellationToken cancellationToken)
    {
        _ = MapSpell(request, existingSpell);
        existingSpell.School = await GetSchoolAsync(request.SchoolId, cancellationToken);

        await UpdateRelationshipsAsync(
            existingSpell.Classes,
            request.ClassIds,
            ids => _dbContext.Classes.Where(c => ids.Contains(c.Id)).ToListAsync(cancellationToken));

        await UpdateRelationshipsAsync(
            existingSpell.Subclasses,
            request.SubclassIds,
            ids => _dbContext.Subclasses.Where(sc => ids.Contains(sc.Id)).ToListAsync(cancellationToken));
    }

    private static Spell MapSpell(SpellRequest request, Spell spell)
    {
        spell.Name = request.Name;
        spell.Desc = request.Desc;
        spell.HigherLevel = request.HigherLevel;
        spell.Range = request.Range;
        spell.Components = request.Components;
        spell.Material = request.Material;
        spell.Ritual = request.Ritual;
        spell.Duration = request.Duration;
        spell.Concentration = request.Concentration;
        spell.CastingTime = request.CastingTime;
        spell.Level = request.Level;
        spell.AttackType = request.AttackType;
        spell.Url = request.Url;
        spell.UpdatedAt = request.UpdatedAt ?? DateTime.Now;
        return spell;
    }

    private async Task<School?> GetSchoolAsync(string? schoolId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(schoolId))
        {
            return null;
        }

        return await _dbContext.Schools.FirstOrDefaultAsync(s => s.Id == schoolId, cancellationToken);
    }

    private async Task<List<Class>> GetClassesAsync(List<string>? classIds, CancellationToken cancellationToken)
    {
        if (classIds is not { Count: > 0 })
        {
            return [];
        }

        return await _dbContext.Classes.Where(c => classIds.Contains(c.Id)).ToListAsync(cancellationToken);
    }

    private async Task<List<Subclass>> GetSubclassesAsync(List<string>? subclassIds, CancellationToken cancellationToken)
    {
        if (subclassIds is not { Count: > 0 })
        {
            return [];
        }

        return await _dbContext.Subclasses.Where(sc => subclassIds.Contains(sc.Id)).ToListAsync(cancellationToken);
    }

    private static async Task UpdateRelationshipsAsync<TEntity>(
        List<TEntity> existingEntities,
        List<string>? requestedIds,
        Func<List<string>, Task<List<TEntity>>> fetchEntitiesAsync)
        where TEntity : class, IHasStringId
    {
        if (requestedIds is null)
        {
            return;
        }

        HashSet<string> requestedIdSet = [.. requestedIds];
        _ = existingEntities.RemoveAll(entity => !requestedIdSet.Contains(entity.Id));

        List<string> existingIds = [.. existingEntities.Select(entity => entity.Id)];
        List<string> missingIds = [.. requestedIdSet.Except(existingIds)];
        if (missingIds.Count == 0)
        {
            return;
        }

        List<TEntity> entitiesToAdd = await fetchEntitiesAsync(missingIds);
        existingEntities.AddRange(entitiesToAdd);
    }
}
