using ApplicationCore.Dtos;
using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface ISpellRepository
    {
        Task<Spell?> GetSpellByIdAsync(string id);
        Task<IEnumerable<Spell>> GetAllSpellsAsync();
        Task AddOrUpdateSpellAsync(SpellRequest spell);
    }
}
