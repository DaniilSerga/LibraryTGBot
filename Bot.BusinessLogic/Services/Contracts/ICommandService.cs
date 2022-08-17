using System.Threading.Tasks;
using Bot.Model.DatabaseModels;

namespace Bot.BusinessLogic.Services.Contracts
{
    public interface ICommandService
    {
        Task<Book> GetBookAsync(int id);
        Task<List<Book>> GetRandomBookAsync();
        Task<List<Book>> GetBooksByGenreAsync(string genreName);
        Task<List<Book>> GetBooksByTitleAsync(string title);
        Task<List<Book>> GetBooksByAuthorAsync(string author);
    }
}
