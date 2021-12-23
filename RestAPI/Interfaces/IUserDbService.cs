using RestApi.Entities;

namespace RestApi.Interfaces
{
    public interface IUserDbService
    {
        Task<List<User>> GetAll();
        Task<User> GetById(Guid id);
        Task<User> Create(User User);
        Task<bool> Delete(User User);
        Task<bool> Update(Guid id, User User);
    }
}
