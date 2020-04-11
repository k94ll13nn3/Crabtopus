using Crabtopus.Models;
using Microsoft.EntityFrameworkCore;

namespace Crabtopus.Data
{
    // https://inloop.github.io/sqlite-viewer/
    internal class Database : DbContext
    {
        public DbSet<GameInfo> GameInfos { get; set; }

        public DbSet<Card> Cards { get; set; }

        public DbSet<Tournament> Tournaments { get; set; }

        public DbSet<Deck> Decks { get; set; }

        public DbSet<DeckCard> DeckCards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=crabtopus.db");
    }
}
