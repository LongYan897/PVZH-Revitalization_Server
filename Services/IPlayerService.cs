using PVZH_Server.DTOs;
using PVZH_Server.Models;

namespace PVZH_Server.Services;
public interface IPlayerService
{
    Task<LoginMessage> Register(RegisterRequest request);
    Task<LoginMessage> LoginByName(LoginRequest request);
    Task<ProfileInfo> GetPlayerAsync(string playerId);
}
