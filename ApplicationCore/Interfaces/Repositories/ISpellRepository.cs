using ApplicationCore.Dtos;
using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface ISpellRepository
    {
        Task<(IEnumerable<Spell> Spells, int TotalItems, int CurrentPage)> GetAllSpellsAsync(
            int page,
            int pageSize,
            string? filter,
            string? sortBy,
            string? sortDirection,
            CancellationToken cancellationToken);

        Task<Spell?> GetSpellByIdAsync(string id, CancellationToken cancellationToken);

        Task AddOrUpdateSpellAsync(SpellRequest request, CancellationToken cancellationToken);
    }
}