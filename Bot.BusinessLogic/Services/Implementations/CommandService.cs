using Bot.BusinessLogic.Services.Contracts;
using Bot.Model.DatabaseModels;
using Bot.Model;
using Microsoft.EntityFrameworkCore;

namespace Bot.BusinessLogic.Services.Implementations
{
    public class CommandService : ICommandService
    {
        // Randomly gets book out of the database
        public async Task<List<Book>> GetRandomBookAsync()
        {
            List<Book> books = new();

            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("taking random book...");
                Console.ResetColor();

                await using (ApplicationContext db = new())
                {
                    // IQueryable and picks 10 random books from database
                    //var dbBooks = db.Books
                    //    .OrderBy(b => EF.Functions.Random())
                    //    .Take(1);

                    Random rand = new Random();
                    int toSkip = rand.Next(0, db.Books.Count());
                    var dbBooks = db.Books.Skip(toSkip).Take(1).First();

                    // INumerable
                    books.Add(dbBooks);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return books;
        }

        // Randomly gets 3 books depending on chosen genre
        public async Task<List<Book>> GetBooksByGenreAsync(string genreName)
        {
            List<Book> books = new();

            try
            {
                await using (ApplicationContext db = new())
                {
                    // IQueryable - takes 3 random depending on chosen genre
                    var dbBooks = db.Books
                        .OrderBy(b => EF.Functions.Random())
                        .Where(b => b.Genre == genreName)
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
