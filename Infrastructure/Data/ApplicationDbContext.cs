using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Spell> Spells { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Damage> Damages { get; set; }
        public DbSet<DamageAtSlotLevel> DamageAtSlotLevels { get; set; }
        public DbSet<DamageType> DamageTypes { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<Subclass> Subclasses { get; set; }
    }
}
