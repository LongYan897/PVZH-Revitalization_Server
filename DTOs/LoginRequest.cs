namespace PVZH_Server.DTOs;

public class LoginRequest
{
    public required string DisplayName { get; set; }
    public required string Password { get; set; }
}
