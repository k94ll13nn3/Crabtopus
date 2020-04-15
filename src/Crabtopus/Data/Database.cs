using Crabtopus.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Crabtopus.Data
{
    internal class Database : DbContext
    {
        private readonly string _connectionString;

        public Database(IOptions<ApplicationSettings> options)
        {
            _connectionString = options.Value.SqliteConnectionString;
        }

        public DbSet<GameInfo> GameInfos { get; set; } = null!;

        public DbSet<Card> Cards { get; set; } = null!;

        public DbSet<Tournament> Tournaments { get; set; } = null!;

        public DbSet<Deck> Decks { get; set; } = null!;

        public DbSet<DeckCard> DeckCards { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(_connectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DeckCard>().HasKey(dc => new { dc.CardId, dc.DeckId, dc.IsSideboard });

            modelBuilder.Entity<Card>()
              .Property(b => b.Colors)
              .UsePropertyAccessMode(PropertyAccessMode.Property);

            modelBuilder.Entity<Card>()
              .Property(b => b.Types)
              .UsePropertyAccessMode(PropertyAccessMode.Property);
        }
    }
}
