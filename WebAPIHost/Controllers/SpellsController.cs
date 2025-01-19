using ApplicationCore.Dtos;
using ApplicationCore.Entities;
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
        public async Task<IActionResult> GetAllSpells()
        {
            var spells = await _spellRepository.GetAllSpellsAsync();
            return Ok(spells);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpell(string id)
        {
            var spell = await _spellRepository.GetSpellByIdAsync(id);
            if (spell == null)
                return NotFound();
            return Ok(spell);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertSpell([FromBody] SpellRequest spell)
        {
            await _spellRepository.AddOrUpdateSpellAsync(spell);
            return Ok(spell);
        }
    }
}
