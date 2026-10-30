using Microsoft.IdentityModel.Tokens;
using PVZH_Server.DTOs;
using PVZH_Server.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MailKit;

namespace PVZH_Server.Services;

public class PlayerService : IPlayerService
{
    private string GenerateToken(string DisplayName,string PlayerID)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier,PlayerID),
            new Claim(ClaimTypes.Name,DisplayName)
        };
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("DST9DrpB21VRcN47y3nohum3tssuJppyqkn7DFHkXRKHT5jfGziGwm1dLwqs+B6Gq4dM7zr6BTNeNl42/KFDbw=="));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "PVZHR",
            audience: "PVZHR_PLAYER",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private readonly IPlayerRepository _repository;
    private readonly IConfiguration _configuration;
    public PlayerService(IPlayerRepository repository,IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }
    public async Task<LoginMessage> LoginByEmail(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<LoginMessage> LoginByName(DisplaynameLoginRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<LoginMessage> Register(RegisterRequest request)
    {
        if (await _repository.GetPlayerByNameAsync(request.DisplayName) != null 
            || await _repository.GetPlayerByEmailAsync(request.Email) != null) 
            return new LoginMessage() {DisplayName = null,Token = null,Code = LoginCode.RegisterNameOrEmailExisted  };
        var playerID = Guid.NewGuid().ToString();

        return new LoginMessage()
        {
            DisplayName = request.DisplayName,
            Token = GenerateToken(request.DisplayName, playerID),
            Code = LoginCode.Success
        };
    }
}
