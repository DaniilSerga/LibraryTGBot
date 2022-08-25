using Bot.Model.DatabaseModels;

namespace Bot.BusinessLogic.Services.Contracts
{
    public interface IGenresService
    {
        Task<List<Genre>> GetAll();
        Task<Genre> Get(int id);
    }
}
