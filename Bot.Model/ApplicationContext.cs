using Bot.Model.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace Bot.Model
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Book> Books { get; set; } = null!;

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=199.247.18.191;Initial catalog=LibraryTgBot;User ID=sa;Password=Lord_1774");
            optionsBuilder.UseSqlServer(@"Server=localhost;Database=TgLibraryDB;Trusted_Connection=True;");
        }
    }
}