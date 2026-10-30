using Microsoft.IdentityModel.Tokens;
using PVZH_Server.DTOs;
using PVZH_Server.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Resend;
using PVZH_Server.Models;

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
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]??""));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMonths(1),
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

    public async Task<LoginMessage> LoginByName(LoginRequest request)
    {
        Player? player = await _repository.GetPlayerByNameAsync(request.DisplayName);
        if (player == null)
            return new LoginMessage() { Code = LoginCode.LoginPlayerNotFound, DisplayName = request.DisplayName, Token = null };
        if (player.Password != request.Password)
            return new LoginMessage() { Code = LoginCode.LoginPasswordIncorrect, DisplayName = request.DisplayName, Token = null };
        return new LoginMessage()
        {
            Code = LoginCode.Success,
            DisplayName = request.DisplayName,
            Token = GenerateToken(request.DisplayName, player.PlayerID)
        };
    }

    public async Task<LoginMessage> Register(RegisterRequest request)
    {
        if (await _repository.GetPlayerByNameAsync(request.DisplayName) != null) 
            return new LoginMessage() {DisplayName = null,Token = null,Code = LoginCode.RegisterNameExisted  };

        var playerID = Guid.NewGuid().ToString();
        await _repository.AddPlayer(request.DisplayName,request.Password,playerID);
        return new LoginMessage()
        {
            DisplayName = request.DisplayName,
            Token = GenerateToken(request.DisplayName, playerID),
            Code = LoginCode.Success
        };
    }
    public async Task<ProfileInfo> GetPlayerAsync(string playerId)
    {
        Player? player = await _repository.GetPlayerByIdAsync(playerId);
        if (player == null)
            return new ProfileInfo { Code = PlayerCode.PlayerNotExisted, DisplayName = null };
        return new ProfileInfo
        {
            Code = PlayerCode.Success,
            DisplayName = player.DisplayName
        };
    }
}
