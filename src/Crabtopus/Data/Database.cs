using Crabtopus.Models;
using Microsoft.EntityFrameworkCore;

namespace Crabtopus.Data
{
    // https://inloop.github.io/sqlite-viewer/
    internal class Database : DbContext
    {
        public DbSet<GameInfo> GameInfos { get; set; } = null!;

        public DbSet<Card> Cards { get; set; } = null!;

        public DbSet<Tournament> Tournaments { get; set; } = null!;

        public DbSet<Deck> Decks { get; set; } = null!;

        public DbSet<DeckCard> DeckCards { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=crabtopus.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DeckCard>().HasKey(dc => new { dc.CardId, dc.DeckId, dc.IsSideboard });
        }
    }
}
