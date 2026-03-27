using ApplicationCore.Dtos;
using ApplicationCore.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HangfireJobs.Services;

public class SpellUpsertJob(HttpClient httpClient, ApplicationDbContext dbContext, ILogger<SpellUpsertJob> logger)
{
    private const string SpellIndexPath = "/api/spells";

    private readonly HttpClient _httpClient = httpClient;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<SpellUpsertJob> _logger = logger;

    private readonly Dictionary<string, School> _schools = [];
    private readonly Dictionary<string, Class> _classes = [];
    private readonly Dictionary<string, Subclass> _subclasses = [];
    private readonly Dictionary<string, Damage> _damages = [];
    private readonly Dictionary<string, DamageType> _damageTypes = [];
    private readonly Dictionary<string, DamageAtSlotLevel> _damageAtSlotLevels = [];

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Spell refresh job started at: {Time}", DateTimeOffset.Now);
        }

        try
        {
            await DeleteAllDataAsync(stoppingToken);

            string response = await _httpClient.GetStringAsync(SpellIndexPath, stoppingToken);
            SpellApiResponse? spellData = JsonConvert.DeserializeObject<SpellApiResponse>(response);
            if (spellData?.Results == null || spellData.Results.Count == 0)
            {
                _logger.LogWarning("No spells returned from the API.");
                return;
            }

            List<Spell> newSpells = await BuildSpellsAsync(spellData.Results, stoppingToken);

            _dbContext.Schools.AddRange(_schools.Values);
            _dbContext.Classes.AddRange(_classes.Values);
            _dbContext.Subclasses.AddRange(_subclasses.Values);
            _dbContext.DamageTypes.AddRange(_damageTypes.Values);
            _dbContext.DamageAtSlotLevels.AddRange(_damageAtSlotLevels.Values);
            _dbContext.Damages.AddRange(_damages.Values);
            _dbContext.Spells.AddRange(newSpells);

            _ = await _dbContext.SaveChangesAsync(stoppingToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Spell refresh completed successfully at: {Time}", DateTimeOffset.Now);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing spells");
        }
    }

    private async Task DeleteAllDataAsync(CancellationToken ct)
    {
        _logger.LogInformation("Deleting all spell-related data via raw SQL...");

        _ = await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Subclasses]", ct);
        _ = await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Spells]", ct);
        _ = await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Damages]", ct);
        _ = await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [DamageTypes]", ct);
        _ = await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [DamageAtSlotLevels]", ct);
        _ = await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Schools]", ct);
        _ = await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Classes]", ct);

        _logger.LogInformation("All related tables cleared via DELETE.");
    }

    private async Task<List<Spell>> BuildSpellsAsync(IEnumerable<SpellSummary> spellSummaries, CancellationToken stoppingToken)
    {
        List<Spell> newSpells = [];

        foreach (SpellSummary spellSummary in spellSummaries)
        {
            Spell? spell = await TryBuildSpellAsync(spellSummary, stoppingToken);
            if (spell != null)
            {
                newSpells.Add(spell);
            }
        }

        return newSpells;
    }

    private async Task<Spell?> TryBuildSpellAsync(SpellSummary spellSummary, CancellationToken stoppingToken)
    {
        try
        {
            string detailResponse = await _httpClient.GetStringAsync(spellSummary.Url, stoppingToken);
            Spell? fullSpell = JsonConvert.DeserializeObject<Spell>(detailResponse);
            if (fullSpell == null)
            {
                _logger.LogWarning("Spell detail could not be parsed: {SpellName}", spellSummary.Name);
                return null;
            }

            NormalizeSpellReferences(fullSpell);
            return fullSpell;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing spell {SpellName}", spellSummary.Name);
            return null;
        }
    }

    private void NormalizeSpellReferences(Spell spell)
    {
        spell.Classes ??= [];
        spell.Subclasses ??= [];
        spell.Damage ??= new();

        if (spell.School != null)
        {
            spell.School = GetOrCreateSchool(spell.School);
        }

        spell.Classes = [.. spell.Classes.Select(GetOrCreateClass)];
        spell.Subclasses = [.. spell.Subclasses.Select(GetOrCreateSubclass)];

        if (spell.Damage != null)
        {
            spell.Damage = GetOrCreateDamage(spell.Damage);
        }
    }

    private School GetOrCreateSchool(School school)
    {
        if (string.IsNullOrWhiteSpace(school.Id))
        {
            school.Id = Guid.NewGuid().ToString();
        }

        if (!_schools.TryGetValue(school.Id, out School? existing))
        {
            existing = new School
            {
                Id = school.Id,
                Name = school.Name,
                Url = school.Url
            };
            _schools[school.Id] = existing;
        }

        return existing;
    }

    private Class GetOrCreateClass(Class @class)
    {
        if (string.IsNullOrWhiteSpace(@class.Id))
        {
            @class.Id = Guid.NewGuid().ToString();
        }

        if (!_classes.TryGetValue(@class.Id, out Class? existing))
        {
            existing = new Class
            {
                Id = @class.Id,
                Name = @class.Name,
                Url = @class.Url
            };
            _classes[@class.Id] = existing;
        }

        return existing;
    }

    private Subclass GetOrCreateSubclass(Subclass subclass)
    {
        if (string.IsNullOrWhiteSpace(subclass.Id))
        {
            subclass.Id = Guid.NewGuid().ToString();
        }

        if (!_subclasses.TryGetValue(subclass.Id, out Subclass? existing))
        {
            existing = new Subclass
            {
                Id = subclass.Id,
                Name = subclass.Name,
                Url = subclass.Url
            };
            _subclasses[subclass.Id] = existing;
        }

        return existing;
    }

    private Damage GetOrCreateDamage(Damage damage)
    {
        if (string.IsNullOrWhiteSpace(damage.Id))
        {
            damage.Id = Guid.NewGuid().ToString();
        }

        if (damage.DamageType != null)
        {
            damage.DamageType = GetOrCreateDamageType(damage.DamageType);
        }

        if (damage.DamageAtSlotLevel != null)
        {
            damage.DamageAtSlotLevel = GetOrCreateDamageAtSlotLevel(damage.DamageAtSlotLevel);
        }

        if (!_damages.TryGetValue(damage.Id, out Damage? existing))
        {
            existing = new Damage
            {
                Id = damage.Id,
                DamageType = damage.DamageType,
                DamageAtSlotLevel = damage.DamageAtSlotLevel
            };
            _damages[damage.Id] = existing;
        }

        return existing;
    }

    private DamageType GetOrCreateDamageType(DamageType damageType)
    {
        if (string.IsNullOrWhiteSpace(damageType.Id))
        {
            damageType.Id = Guid.NewGuid().ToString();
        }

        if (!_damageTypes.TryGetValue(damageType.Id, out DamageType? existing))
        {
            existing = new DamageType
            {
                Id = damageType.Id,
                Name = damageType.Name,
                Url = damageType.Url
            };
            _damageTypes[damageType.Id] = existing;
        }

        return existing;
    }

    private DamageAtSlotLevel GetOrCreateDamageAtSlotLevel(DamageAtSlotLevel damageAtSlotLevel)
    {
        if (string.IsNullOrWhiteSpace(damageAtSlotLevel.Id))
        {
            damageAtSlotLevel.Id = Guid.NewGuid().ToString();
        }

        if (!_damageAtSlotLevels.TryGetValue(damageAtSlotLevel.Id, out DamageAtSlotLevel? existing))
        {
            existing = new DamageAtSlotLevel
            {
                Id = damageAtSlotLevel.Id,
                _0 = damageAtSlotLevel._0,
                _1 = damageAtSlotLevel._1,
                _2 = damageAtSlotLevel._2,
                _3 = damageAtSlotLevel._3,
                _4 = damageAtSlotLevel._4,
                _5 = damageAtSlotLevel._5,
                _6 = damageAtSlotLevel._6,
                _7 = damageAtSlotLevel._7,
                _8 = damageAtSlotLevel._8,
                _9 = damageAtSlotLevel._9
            };
            _damageAtSlotLevels[damageAtSlotLevel.Id] = existing;
        }

        return existing;
    }
}
