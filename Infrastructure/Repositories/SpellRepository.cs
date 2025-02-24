using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Dtos;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class SpellRepository(ApplicationDbContext dbContext) : ISpellRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<(IEnumerable<Spell> Spells, int TotalItems, int CurrentPage)> GetAllSpellsAsync(
            int page, int pageSize, string? filter, string? sortBy, string? sortDirection, CancellationToken cancellationToken)
        {
            var query = _dbContext.Spells
                .Include(s => s.Classes)
                .Include(s => s.School)
                .AsNoTracking();

            // Apply filter if provided
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(s => s.Name.Contains(filter));
            }

            // Apply sorting
            query = ApplySorting(query, sortBy, sortDirection);

            // Get total count for pagination
            int totalItems = await query.CountAsync(cancellationToken);

            // Apply pagination
            var spells = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return (spells, totalItems, page);
        }

        private static IQueryable<Spell> ApplySorting(IQueryable<Spell> query, string? sortBy, string? sortDirection)
        {
            // Default sort
            if (string.IsNullOrEmpty(sortBy))
            {
                sortBy = "name";
            }

            bool isAscending = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("asc", StringComparison.CurrentCultureIgnoreCase);

            // Apply sorting based on column
            query = sortBy.ToLower() switch
            {
                "name" => isAscending
                                        ? query.OrderBy(s => s.Name)
                                        : query.OrderByDescending(s => s.Name),
                "level" => isAscending
                                        ? query.OrderBy(s => s.Level)
                                        : query.OrderByDescending(s => s.Level),
                "class" => isAscending
                                        ? query.OrderBy(s => 
                                            s.Classes.OrderBy(c => c.Name)
                                                     .Select(c => c.Name)
                                                     .FirstOrDefault() ?? string.Empty)
                                        : query.OrderByDescending(s => 
                                            s.Classes.OrderBy(c => c.Name)
                                                     .Select(c => c.Name)
                                                     .FirstOrDefault() ?? string.Empty),
                _ => isAscending
                                        ? query.OrderBy(s => s.Name)
                                        : query.OrderByDescending(s => s.Name),
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
            // 1) Attempt to find the existing Spell
            var existingSpell = await _dbContext.Spells
                .Include(s => s.Classes)
                .Include(s => s.Subclasses)
                .Include(s => s.School)  // optional if referencing School
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            // 2) If not found, create a brand-new Spell
            if (existingSpell is null)
            {
                var newSpell = new Spell
                {
                    Id = request.Id,
                    Name = request.Name,
                    Desc = request.Desc,
                    HigherLevel = request.HigherLevel,
                    Range = request.Range,
                    Components = request.Components,
                    Material = request.Material,
                    Ritual = request.Ritual,
                    Duration = request.Duration,
                    Concentration = request.Concentration,
                    CastingTime = request.CastingTime,
                    Level = request.Level,
                    AttackType = request.AttackType,
                    Url = request.Url,
                    UpdatedAt = request.UpdatedAt ?? DateTime.Now
                };

                // 2a) If user supplies a SchoolId, link the corresponding School
                if (!string.IsNullOrEmpty(request.SchoolId))
                {
                    var school = await _dbContext.Schools
                        .FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken);
                    if (school != null)
                        newSpell.School = school;
                }

                // 2b) Many-to-many: link to Classes
                if (request.ClassIds?.Count > 0)
                {
                    var classesToAdd = await _dbContext.Classes
                        .Where(c => request.ClassIds.Contains(c.Id))
                        .ToListAsync(cancellationToken);
                    newSpell.Classes = classesToAdd;
                }

                // 2c) Many-to-many: link to Subclasses
                if (request.SubclassIds?.Count > 0)
                {
                    var subclassesToAdd = await _dbContext.Subclasses
                        .Where(sc => request.SubclassIds.Contains(sc.Id))
                        .ToListAsync(cancellationToken);
                    newSpell.Subclasses = subclassesToAdd;
                }

                // Finally add the new Spell
                _dbContext.Spells.Add(newSpell);
            }
            else
            {
                // 3) Update existing Spell - implementation remains the same
                // ... (existing code unchanged)
                existingSpell.Name = request.Name;
                existingSpell.Desc = request.Desc;
                existingSpell.HigherLevel = request.HigherLevel;
                existingSpell.Range = request.Range;
                existingSpell.Components = request.Components;
                existingSpell.Material = request.Material;
                existingSpell.Ritual = request.Ritual;
                existingSpell.Duration = request.Duration;
                existingSpell.Concentration = request.Concentration;
                existingSpell.CastingTime = request.CastingTime;
                existingSpell.Level = request.Level;
                existingSpell.AttackType = request.AttackType;
                existingSpell.Url = request.Url;
                existingSpell.UpdatedAt = request.UpdatedAt ?? DateTime.Now;

                // School reference
                if (!string.IsNullOrEmpty(request.SchoolId))
                {
                    var school = await _dbContext.Schools
                        .FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken);
                    existingSpell.School = school;
                }
                else
                {
                    existingSpell.School = null;
                }

                // 3a) Many-to-many for Classes
                if (request.ClassIds != null)
                {
                    var existingClassIds = existingSpell.Classes.Select(cls => cls.Id).ToList();
                    var newClassIds = request.ClassIds;

                    // Remove missing
                    var removedClassIds = existingClassIds.Except(newClassIds).ToList();
                    existingSpell.Classes.RemoveAll(c => removedClassIds.Contains(c.Id));

                    // Add newly introduced
                    var addedClassIds = newClassIds.Except(existingClassIds).ToList();
                    if (addedClassIds.Count > 0)
                    {
                        var classesToAdd = await _dbContext.Classes
                            .Where(c => addedClassIds.Contains(c.Id))
                            .ToListAsync(cancellationToken);
                        foreach (var cls in classesToAdd)
                            existingSpell.Classes.Add(cls);
                    }
                }

                // 3b) Many-to-many for Subclasses
                if (request.SubclassIds != null)
                {
                    var existingSubclassIds = existingSpell.Subclasses.Select(sc => sc.Id).ToList();
                    var newSubclassIds = request.SubclassIds;

                    // Remove missing
                    var removedSubclassIds = existingSubclassIds.Except(newSubclassIds).ToList();
                    existingSpell.Subclasses.RemoveAll(sc => removedSubclassIds.Contains(sc.Id));

                    // Add newly introduced
                    var addedSubclassIds = newSubclassIds.Except(existingSubclassIds).ToList();
                    if (addedSubclassIds.Count > 0)
                    {
                        var subclassesToAdd = await _dbContext.Subclasses
                            .Where(sc => addedSubclassIds.Contains(sc.Id))
                            .ToListAsync(cancellationToken);
                        foreach (var sc in subclassesToAdd)
                            existingSpell.Subclasses.Add(sc);
                    }
                }
            }

            // 4) Save changes
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}