using Bot.Model.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace Bot.Model
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;

        public ApplicationContext()
        {
            //Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=199.247.18.191;Initial catalog=LibraryTgBot;User ID=sa;Password=Lord_1774");
            optionsBuilder.UseSqlServer(@"Server=199.247.18.191;Initial catalog=TgScribeDb;User ID=sa;Password=Lord_1774");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Genre>()
                .HasIndex(g => g.Name)
                .IsUnique();

            builder.Entity<Author>()
                .HasIndex(a => a.Name)
                .IsUnique();
        }
    }
}