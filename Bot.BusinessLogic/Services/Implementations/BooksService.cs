using Bot.BusinessLogic.Services.Contracts;
using Bot.Model;
using Bot.Model.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace Bot.BusinessLogic.Services.Implementations
{
    public class BooksService : IBooksService
    {
        public async Task<Book> GetBook(int id)
        {
            PrintProccessMessage("Taking book from database...");

            Book book = new();

            try
            {
                using ApplicationContext db = new();

                book = await db.Books
                    .Include(b => b.Author)
                    .Include(b => b.Genre)
                    .Where(b => b.Id == id)
                    .FirstAsync();

                if (book is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such book with provided Id was found.");
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("The book was successfultt taken!");

            return book;
        }

        public async Task CreateBook(Book book)
        {
            if (book is null)
            {
                PrintErrorMessage("Book is null. Stack trace: CreateBook(Book book)");
                return;
            }

            PrintProccessMessage("Creating a new book in database...");

            try
            {
                using ApplicationContext db = new();

                await db.Books.AddAsync(book);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("Successfully created a new book!");
        }

        public async Task DeleteBook(int id)
        {
            PrintProccessMessage("Deleting the book from database...");

            try
            {
                using ApplicationContext db = new();

                var book = db.Books.FirstOrDefault(b => b.Id == id);

                if (book is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such book with provided Id was found. Stack trace: DeleteBook(int id).");
                }

                db.Books.Remove(book);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("The book was deleted successfully!");
        }

        public async Task UpdateBook(Book book, int id)
        {
            if (book is null)
            {
                PrintErrorMessage("The book was null");
                return;
            }

            PrintProccessMessage("Updating the book's data...");

            try
            {
                using ApplicationContext db = new();

                var dbBook = db.Books.FirstOrDefault(b => b.Id == id);

                if (dbBook is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such book with provided Id was found.");
                }

                dbBook = book;

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("The book's data was saved successfully!");
        }

        public async Task<Book> GetRandomBookAsync()
        {
            PrintProccessMessage("Taking a random book from the database.");

            Book book = new();

            try
            {
                using ApplicationContext db = new();

                Random rand = new();
                int toSkip = rand.Next(0, db.Books.Count());

                book = await db.Books.Skip(toSkip)
                        .Take(1)
                        .Include(b => b.Genre)
                        .Include(b => b.Author)
                        .FirstAsync();
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("The book was successfultt taken!");

            return book;
        }

        public async Task<Book> GetRandomBookByGenreAsync(string genreName)
        {
            Book book = new();

            try
            {
                if (string.IsNullOrEmpty(genreName))
                {
                    PrintErrorMessage("Genre name was null or empty.");
                }

                PrintProccessMessage("Taking a random book by the provided genre...");

                using ApplicationContext db = new();

                Random rand = new();

                var dbBooks = db.Books
                    .Include(b => b.Genre)
                    .Include(b => b.Author)
                    .Where(b => b.Genre.Name == genreName)
                    .ToList();

                int randomId = rand.Next(0, dbBooks.Count - 1);

                book = dbBooks[randomId];
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("The book was successfullt taken!");

            return book;
        }

        #region Console messages
        private static void PrintErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{message}\n");
            Console.ResetColor();
        }
        private static void PrintProccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{message}\n");
            Console.ResetColor();
        }
        #endregion
    }
}
