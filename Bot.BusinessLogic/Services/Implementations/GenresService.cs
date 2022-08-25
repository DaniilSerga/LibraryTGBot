using Bot.BusinessLogic.Services.Contracts;
using Bot.Model.DatabaseModels;
using Bot.Model;

namespace Bot.BusinessLogic.Services.Implementations
{
    public class GenresService : IGenresService
    {
        public async Task<List<Genre>> GetAll()
        {
            PrintProccessMessage("Getting all genres from database...");

            List<Genre> genres = new();

            try
            {
                using ApplicationContext db = new();

                genres.AddRange(db.Genres.ToList());

                if (genres is null)
                {
                    throw new Exception("No genres was found");
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("All genres were successfully taken from the database!");
            return genres;
        }

        public async Task<Genre> Get(int id)
        {
            PrintProccessMessage("Taking a genre from the database...");

            Genre genre = new();

            try
            {
                using ApplicationContext db = new();

                genre = db.Genres.FirstOrDefault(g => g.Id == id);

                if (genre is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), "No such genre was found with the provided Id.");
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
            }

            PrintProccessMessage("The genre was successfully taken from the database!");

            return genre;
        }

        #region Console messages
        private void PrintErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{message}\n");
            Console.ResetColor();
        }
        private void PrintProccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();

        }
        #endregion
    }
}
