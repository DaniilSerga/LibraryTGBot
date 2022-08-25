using Bot.Model.DatabaseModels;

namespace Bot.BusinessLogic.Services.Contracts
{
    public interface IBooksService
    {
        #region CRUD
        Task CreateBook(Book book);
        Task<Book> GetBook(int id);
        Task UpdateBook(Book book, int id);
        Task DeleteBook(int id);
        #endregion

        Task<Book> GetRandomBookAsync();
        Task<Book> GetRandomBookByGenreAsync(string genreName);
    }
}
