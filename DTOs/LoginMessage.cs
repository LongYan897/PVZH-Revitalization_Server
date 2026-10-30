using System.IdentityModel.Tokens.Jwt;

namespace PVZH_Server.DTOs;
public class LoginMessage
{
    public required string? DisplayName { get; set; }
    public required string? Token { get; set; }
    public required LoginCode Code { get; set; }
}

public enum LoginCode
{
    Success = 0,
    LoginPlayerNotFoundOrPasswordIncorrect = 1,
    RegisterNameOrEmailExisted = 2
}