namespace PVZH_Server.DTOs;
public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class DisplaynameLoginRequest
{
    public required string DisplayName { get; set; }
    public required string Password { get; set; }
}
