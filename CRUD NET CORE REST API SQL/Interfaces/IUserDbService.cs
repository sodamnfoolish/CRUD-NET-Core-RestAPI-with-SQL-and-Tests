using ProjectRestApi.Entities;

namespace ProjectRestApi.Interfaces
{
    public interface IUserDbService
    {
        Task<List<User>> GetAll();
        Task<User> GetById(Guid id);
        Task<User> Create(User User);
        Task<bool> Delete(Guid id);
        Task<bool> Update(Guid id, User User);
    }
}
