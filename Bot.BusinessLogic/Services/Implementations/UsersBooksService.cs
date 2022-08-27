using Bot.BusinessLogic.Services.Contracts;
using Bot.Model.DatabaseModels;
using Bot.Model;

namespace Bot.BusinessLogic.Services.Implementations
{
    public class UsersBooksService : IUsersBooksService
    {
        public async Task<UserBook> Get(int id)
        {
            UserBook userBook = new();

            try
            {
                using ApplicationContext db = new();
                userBook = db.UsersBooks.FirstOrDefault(u => u.Id == id);

                if (userBook is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such user's book was found.");
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            return userBook;
        }

        public async Task<List<UserBook>> GetAllUserBooks(int userId)
        {
            PrintProccessMesage("Getting all books depending on user id...");

            List<UserBook> userBooks = new();

            try
            {
                using ApplicationContext db = new();

                userBooks.AddRange(db.UsersBooks.Where(u => u.UserId == userId));

                if (userBooks.Count == 0 || userBooks is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(userId), "Invalid user Id.");
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMesage("All books were got!");

            return userBooks;
        }

        public async Task Create(UserBook userBook)
        {
            try
            {
                if (userBook is null)
                {
                    throw new ArgumentNullException(nameof(userBook), "User's book is null.");
                }

                PrintProccessMesage("Adding new user's book to the database...");

                using ApplicationContext db = new();

                await db.UsersBooks.AddAsync(userBook);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMesage("The user's book was successfully saved!");
        }

        public async Task Delete(int id)
        {
            try
            {
                PrintProccessMesage("Deleting user's book...");

                using ApplicationContext db = new();

                var userBook = db.UsersBooks.FirstOrDefault(u => u.Id == id);

                if (userBook is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such user's book was found.");
                }

                db.UsersBooks.Remove(userBook);

                await db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMesage("User's book was successfully deleted!");
        }

        public async Task Update(UserBook userBook, int id)
        {
            try
            {
                if (userBook is null)
                {
                    throw new ArgumentNullException(nameof(userBook), "User's book was null.");
                }

                using ApplicationContext db = new();

                var dbUserBook = db.UsersBooks.FirstOrDefault(u => u.Id == id);

                if (dbUserBook is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such user's book with provided Id was found.");
                }

                dbUserBook = userBook;

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }
        }

        #region Console messages
        private static void PrintErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{message}\n");
            Console.ResetColor();
        }
        private static void PrintProccessMesage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{message}\n");
            Console.ResetColor();
        }
        #endregion
    }
}
