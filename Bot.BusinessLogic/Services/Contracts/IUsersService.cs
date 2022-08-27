using Bot.Model.DatabaseModels;

namespace Bot.BusinessLogic.Services.Contracts
{
    public interface IUsersService
    {
        Task<User> Get(int id);
        Task Create(User user);
        Task Delete(int id);
        Task Update(User user, int id);
    }
}
