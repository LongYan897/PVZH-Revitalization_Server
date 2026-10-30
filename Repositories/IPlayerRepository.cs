using PVZH_Server.Models;

namespace PVZH_Server.Repositories;
public interface IPlayerRepository
{
    Task<Player?> GetPlayerByIdAsync(string PlayerId);
    Task<Player?> GetPlayerByNameAsync(string DisplayName);
    Task<Player?> GetPlayerByEmailAsync(string DisplayEmail);
}
