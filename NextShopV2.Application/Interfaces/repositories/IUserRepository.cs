using NextShopV2.Domain.Entities.Users;
namespace NextShopV2.Application.Interfaces
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);
        User? GetById(Guid id);
        void Add(User user);
        bool ExistsByEmail(string email);
        void Save();
    }
}