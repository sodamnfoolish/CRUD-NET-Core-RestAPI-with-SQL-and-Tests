using RestApi.Entities;
using RestApi.Dtos;

namespace RestApi.Interfaces
{
    public interface IUserDbService
    {
        Task<List<User>> GetAll();
        Task<User> GetById(Guid id);
        Task<User> Create(User user);
        Task<User> Delete(User user);
        Task<User> Update(User user);
    }
}
