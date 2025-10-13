using Travel.Core.Entities;

namespace Travel.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user, string password);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> AuthenticateAsync(string email, string password);

        Task UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<int> GetUsersCountAsync();
    }
}
