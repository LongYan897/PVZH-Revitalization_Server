using PVZH_Server.DTOs;

namespace PVZH_Server.Services;
public interface IPlayerService
{
    Task<LoginMessage> Register(RegisterRequest request);
    Task<LoginMessage> LoginByEmail(LoginRequest request);
    Task<LoginMessage> LoginByName(DisplaynameLoginRequest request);
}
