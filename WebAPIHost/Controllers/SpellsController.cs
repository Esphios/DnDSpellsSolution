using ApplicationCore.Dtos;
using ApplicationCore.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpellsController(ISpellRepository spellRepository) : ControllerBase
    {
        private readonly ISpellRepository _spellRepository = spellRepository;

        [HttpGet]
        public async Task<IActionResult> GetAllSpells(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortDirection = "asc",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (Spells, TotalItems, CurrentPage) = await _spellRepository.GetAllSpellsAsync(
                    page, pageSize, filter, sortBy, sortDirection, cancellationToken);

                var response = new
                {
                    spells = Spells,
                    totalItems = TotalItems,
                    currentPage = CurrentPage,
                    totalPages = (int)Math.Ceiling(TotalItems / (double)pageSize)
                };

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request was canceled.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpell(string id, CancellationToken cancellationToken = default)
        {
            var spell = await _spellRepository.GetSpellByIdAsync(id, cancellationToken);
            if (spell == null)
                return NotFound();
            return Ok(spell);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertSpell([FromBody] SpellRequest spell, CancellationToken cancellationToken = default)
        {
            await _spellRepository.AddOrUpdateSpellAsync(spell, cancellationToken);
            return Ok(spell);
        }
    }
}