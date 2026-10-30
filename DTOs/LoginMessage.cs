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
    LoginPlayerNotFound = 1,
    LoginPasswordIncorrect = 2,
    RegisterNameExisted = 3,
    RegisterNameCannotBeWhiteSpace = 4
}