using Bot.BusinessLogic.Services.Contracts;
using Bot.Model;
using Bot.Model.DatabaseModels;

namespace Bot.BusinessLogic.Services.Implementations
{
    public class UsersService : IUsersService
    {
        public async Task<User> Get(int id)
        {
            User user = new();

            PrintProccessMessage("Getting the user from the database...");

            try
            {
                using ApplicationContext db = new();

                user = db.Users.FirstOrDefault(u => u.Id == id);

                if (user is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such user with provided Id was found.");
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("The user was taken from the database!");

            return user;
        }

        public async Task<User> GetByTelegramId(long userId)
        {
            User user = new();

            try
            {
                PrintProccessMessage("Getting user by user's telegram Id...");

                using ApplicationContext db = new();

                user = db.Users.FirstOrDefault(u => u.UserId == userId);

                if (user is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(userId), "No such user with provided Id was found.");
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            return user;
        }

        public async Task Create(User user)
        {
            PrintProccessMessage("Creating a new user in the database...");

            try
            {
                if (user is null)
                {
                    throw new ArgumentNullException(nameof(user), "User is null.");
                }

                using ApplicationContext db = new();

                if (!db.Users.Any(u => u.Username == user.Username))
                {
                    await db.Users.AddAsync(user);
                    await db.SaveChangesAsync();
                }
                else
                {
                    PrintProccessMessage("User already exists");
                    return;
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }
        }

        public async Task Delete(int id)
        {
            PrintProccessMessage("Deleting user from the database...");

            try
            {
                using ApplicationContext db = new();

                var user = db.Users.FirstOrDefault(u => u.Id == id);

                if (user is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such user with provided Id was found.");
                }

                db.Users.Remove(user);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("User was successfully deleted from the database.");
        }

        public async Task Update(User user, int id)
        {
            try
            {
                if (user is null)
                {
                    throw new ArgumentNullException(nameof(user), "User was null.");
                }

                PrintProccessMessage("Updating user's info.");

                using ApplicationContext db = new();

                var dbUser = db.Users.FirstOrDefault(u => u.Id == id);

                if (dbUser is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such user with provided Id was found.");
                }

                dbUser = user;

                await db.SaveChangesAsync();

                PrintProccessMessage("User's info was successfully updated.");
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }
        }

        public async Task<bool> UserExists(long userId)
        {
            bool exists = false;

            try
            {
                using ApplicationContext db = new();

                var user = db.Users.FirstOrDefault(u => u.UserId == userId);

                if (user is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(userId), "No such user with provided Id was found.");
                }

                exists = db.Users.Any(u => u == user);
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            return exists;
        }

        #region Console messages
        private static void PrintErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{message}");
            Console.ResetColor();
        }
        private static void PrintProccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{message}");
            Console.ResetColor();
        }
        #endregion
    }
}
