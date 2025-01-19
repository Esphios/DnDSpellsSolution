using ApplicationCore.Dtos;
using ApplicationCore.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HangfireJobs.Services
{
    public class SpellUpsertJob(HttpClient httpClient, ApplicationDbContext dbContext, ILogger<SpellUpsertJob> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly ILogger<SpellUpsertJob> _logger = logger;

        // In-memory dictionaries to avoid duplications when re-inserting
        private readonly Dictionary<string, School> _schools = [];
        private readonly Dictionary<string, Class> _classes = [];
        private readonly Dictionary<string, Subclass> _subclasses = [];
        private readonly Dictionary<string, Damage> _damages = [];
        private readonly Dictionary<string, DamageType> _damageTypes = [];
        private readonly Dictionary<string, DamageAtSlotLevel> _damageAtSlotLevels = [];

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Spell refresh job started at: {time}", DateTimeOffset.Now);

            try
            {
                // 1) Completely delete existing data via raw SQL
                await DeleteAllDataAsync(stoppingToken);

                // 2) Fetch the main spells index from the DnD 5e API
                var response = await _httpClient.GetStringAsync("https://www.dnd5eapi.co/api/spells", stoppingToken);
                var spellData = JsonConvert.DeserializeObject<SpellAPIResponse>(response);
                if (spellData?.Results == null || spellData.Results.Count == 0)
                {
                    _logger.LogWarning("No spells returned from the API.");
                    return;
                }

                var newSpells = new List<Spell>();

                // 3) For each spell, fetch details and build new objects
                foreach (var spellSummary in spellData.Results)
                {
                    try
                    {
                        var detailResponse = await _httpClient.GetStringAsync(
                            $"https://www.dnd5eapi.co{spellSummary.Url}",
                            stoppingToken
                        );

                        var fullSpell = JsonConvert.DeserializeObject<Spell>(detailResponse);
                        if (fullSpell == null)
                        {
                            _logger.LogWarning("Spell detail could not be parsed: {spellName}", spellSummary.Name);
                            continue;
                        }

                        // Make sure they're non-null
                        fullSpell.Classes ??= [];
                        fullSpell.Subclasses ??= [];
                        fullSpell.Damage ??= new();

                        // School
                        if (fullSpell.School != null)
                            fullSpell.School = GetOrCreateSchool(fullSpell.School);

                        // Classes
                        var classList = new List<Class>();
                        foreach (var c in fullSpell.Classes)
                            classList.Add(GetOrCreateClass(c));
                        fullSpell.Classes = classList;

                        // Subclasses
                        var subclassList = new List<Subclass>();
                        foreach (var sc in fullSpell.Subclasses)
                            subclassList.Add(GetOrCreateSubclass(sc));
                        fullSpell.Subclasses = subclassList;

                        // Damage (DamageType, DamageAtSlotLevel)
                        if (fullSpell.Damage != null)
                            fullSpell.Damage = GetOrCreateDamage(fullSpell.Damage);

                        newSpells.Add(fullSpell);
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError(ex2, "Error processing spell {spellName}", spellSummary.Name);
                    }
                }

                // 4) Insert the distinct references from dictionaries
                _dbContext.Schools.AddRange(_schools.Values);
                _dbContext.Classes.AddRange(_classes.Values);
                _dbContext.Subclasses.AddRange(_subclasses.Values);
                _dbContext.DamageTypes.AddRange(_damageTypes.Values);
                _dbContext.DamageAtSlotLevels.AddRange(_damageAtSlotLevels.Values);
                _dbContext.Damages.AddRange(_damages.Values);

                // 5) Insert all spells
                _dbContext.Spells.AddRange(newSpells);

                // 6) Save once
                await _dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Spell refresh completed successfully at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing spells");
            }
        }

        /// <summary>
        /// Deletes all data from the spell-related tables, in an order that avoids FK conflicts.
        /// If you see foreign key errors, rearrange this order to first delete “sub” or “many-to-many” tables.
        /// </summary>
        private async Task DeleteAllDataAsync(CancellationToken ct)
        {
            _logger.LogInformation("Deleting all spell-related data via raw SQL...");

            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Subclasses]", ct);
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Spells]", ct);
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Damages]", ct);
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [DamageTypes]", ct);
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [DamageAtSlotLevels]", ct);
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Schools]", ct);
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Classes]", ct);
            _logger.LogInformation("All related tables cleared via DELETE.");
        }

        // =============================================
        // Helpers for in-memory deduplication
        // =============================================
        private School GetOrCreateSchool(School s)
        {
            if (string.IsNullOrWhiteSpace(s.Id))
                s.Id = Guid.NewGuid().ToString();

            if (!_schools.TryGetValue(s.Id, out var existing))
            {
                existing = new School
                {
                    Id = s.Id,
                    Name = s.Name,
                    Url = s.Url
                };
                _schools[s.Id] = existing;
            }
            return existing;
        }

        private Class GetOrCreateClass(Class c)
        {
            if (string.IsNullOrWhiteSpace(c.Id))
                c.Id = Guid.NewGuid().ToString();

            if (!_classes.TryGetValue(c.Id, out var existing))
            {
                existing = new Class
                {
                    Id = c.Id,
                    Name = c.Name,
                    Url = c.Url
                };
                _classes[c.Id] = existing;
            }
            return existing;
        }

        private Subclass GetOrCreateSubclass(Subclass sc)
        {
            if (string.IsNullOrWhiteSpace(sc.Id))
                sc.Id = Guid.NewGuid().ToString();

            if (!_subclasses.TryGetValue(sc.Id, out var existing))
            {
                existing = new Subclass
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Url = sc.Url
                };
                _subclasses[sc.Id] = existing;
            }
            return existing;
        }

        private Damage GetOrCreateDamage(Damage dmg)
        {
            if (string.IsNullOrWhiteSpace(dmg.Id))
                dmg.Id = Guid.NewGuid().ToString();

            if (dmg.DamageType != null)
                dmg.DamageType = GetOrCreateDamageType(dmg.DamageType);

            if (dmg.DamageAtSlotLevel != null)
                dmg.DamageAtSlotLevel = GetOrCreateDamageAtSlotLevel(dmg.DamageAtSlotLevel);

            if (!_damages.TryGetValue(dmg.Id, out var existing))
            {
                existing = new Damage
                {
                    Id = dmg.Id,
                    DamageType = dmg.DamageType,
                    DamageAtSlotLevel = dmg.DamageAtSlotLevel
                };
                _damages[dmg.Id] = existing;
            }
            return existing;
        }

        private DamageType GetOrCreateDamageType(DamageType dt)
        {
            if (string.IsNullOrWhiteSpace(dt.Id))
                dt.Id = Guid.NewGuid().ToString();

            if (!_damageTypes.TryGetValue(dt.Id, out var existing))
            {
                existing = new DamageType
                {
                    Id = dt.Id,
                    Name = dt.Name,
                    Url = dt.Url
                };
                _damageTypes[dt.Id] = existing;
            }
            return existing;
        }

        private DamageAtSlotLevel GetOrCreateDamageAtSlotLevel(DamageAtSlotLevel dasl)
        {
            if (string.IsNullOrWhiteSpace(dasl.Id))
                dasl.Id = Guid.NewGuid().ToString();

            if (!_damageAtSlotLevels.TryGetValue(dasl.Id, out var existing))
            {
                existing = new DamageAtSlotLevel
                {
                    Id = dasl.Id,
                    _0 = dasl._0,
                    _1 = dasl._1,
                    _2 = dasl._2,
                    _3 = dasl._3,
                    _4 = dasl._4,
                    _5 = dasl._5,
                    _6 = dasl._6,
                    _7 = dasl._7,
                    _8 = dasl._8,
                    _9 = dasl._9
                };
                _damageAtSlotLevels[dasl.Id] = existing;
            }
            return existing;
        }
    }
}