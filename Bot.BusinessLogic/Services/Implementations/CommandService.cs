using Bot.BusinessLogic.Services.Contracts;
using Bot.Model.DatabaseModels;
using Bot.Model;
using Microsoft.EntityFrameworkCore;

namespace Bot.BusinessLogic.Services.Implementations
{
    public class CommandService : ICommandService
    {
        // Randomly gets 10 books out of the database
        public async Task<List<Book>> GetRandomBooksAsync()
        {
            List<Book> books = new();

            try
            {
                await using (ApplicationContext db = new())
                {
                    // IQueryable and picks 10 random books from database
                    var dbBooks = db.Books
                        .OrderBy(b => EF.Functions.Random())
                        .Take(3);

                    // INumerable
                    books.AddRange(dbBooks.ToList());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return books;
        }

        // Randomly gets 10 books depending on chosen genre
        public async Task<List<Book>> GetBooksByGenreAsync(string genreName)
        {
            List<Book> books = new();

            try
            {
                using (ApplicationContext db = new())
                {
                    // IQueryable - takes 3 random depending on chosen genre
                    var dbBooks = db.Books
                        .OrderBy(b => EF.Functions.Random())
                        .Where(b => b.Genre.Name == genreName)
                        .Take(3);

                    // INumerable
                    books.AddRange(dbBooks.ToList());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return books;
        }
    }
}
