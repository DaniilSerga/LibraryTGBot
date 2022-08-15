using System.Threading.Tasks;
using Bot.Model.DatabaseModels;

namespace Bot.BusinessLogic.Services.Contracts
{
    public interface ICommandService
    {
        Task<List<Book>> GetRandomBooksAsync();
        Task<List<Book>> GetBooksByGenreAsync(string genreName);
    }
}
