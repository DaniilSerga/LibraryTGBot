using Bot.Model.DatabaseModels;

namespace Bot.BusinessLogic.Services.Contracts
{
    public interface IUsersBooksService
    {
        Task<UserBook> Get(int id);
        Task<List<UserBook>> GetAllUserBooks(int userId);
        Task Create(UserBook userBook);
        Task Update(UserBook userBook, int id);
        Task Delete(int id);
    }
}
