using Microsoft.IdentityModel.Tokens;
using PVZH_Server.DTOs;
using PVZH_Server.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PVZH_Server.Models;

namespace PVZH_Server.Services;

public interface IPlayerService
{
    Task<LoginMessage> Register(RegisterRequest request);
    Task<LoginMessage> LoginByName(LoginRequest request);
    Task<ProfileMessage> GetPlayerAsync(string playerId);
    Task<FriendRMessage> SendFriendRequest(string playerId,FriendRequest request);
    Task<CheckFRMessages> CheckFriendRequest(string playerId);
    Task<RelationMessages> GetRelationAsync(string playerId);
    Task<FRProcessMessage> ProcessFRMessage(string playerId, ApproveFRRequest request);
    Task<RankMessage> GetRank(string playerId, RankRequest request);
    Task<FriendMessage> SendMessage(string playerId, FriendMessageRequest request);
}
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
    public async Task<ProfileMessage> GetPlayerAsync(string playerId)
    {
        Player? player = await _repository.GetPlayerByIdAsync(playerId);
        if (player == null)
            return new ProfileMessage { Code = PlayerCode.PlayerNotExisted, DisplayName = null,Diamond = 0 };
        return new ProfileMessage
        {
            Code = PlayerCode.Success,
            DisplayName = player.DisplayName,
            Diamond = player.Diamond
        };
    }
    public async Task<FriendRMessage> SendFriendRequest(string playerId,FriendRequest request)
    {
        var recipient = await _repository.GetPlayerByNameAsync(request.FriendName);

        if (recipient == null) return new FriendRMessage() { Code = PlayerCode.PlayerNotExisted, FriendId = null };

        if (playerId == recipient.PlayerID)
            return new FriendRMessage() { Code = PlayerCode.PlayerNotExisted, FriendId = recipient.PlayerID };

        if (await _repository.HasSentFriendRequest(playerId, recipient.PlayerID)|| await _repository.AreFriends(playerId,recipient.PlayerID))
            return new FriendRMessage() { Code = PlayerCode.OperationCannotRepeat, FriendId = recipient.PlayerID };

        await _repository.SendFriendRequest(playerId, recipient.PlayerID);

        return new FriendRMessage() { Code = PlayerCode.Success , FriendId=recipient.PlayerID};
    }
    public async Task<CheckFRMessages> CheckFriendRequest(string playerId)
    {
        var datas = await _repository.GetFriendRequest(playerId);
        var FRMs = new List<CheckFRMessage>();
        foreach (FriendRequestData data in datas)
        {
            FRMs.Add(
                new CheckFRMessage() { SenderName = data.senderName}
                );
        }
        return new CheckFRMessages() { Requests = FRMs };
    }
    public async Task<RelationMessages> GetRelationAsync(string playerId)
    {
        var rds = await _repository.GetRelationsAsync(playerId);
        var rms = new List<RelationMessage>();
        foreach (var rd in rds)
        {
            rms.Add(
                new RelationMessage()
                {
                    Player1Name = rd.Player1Name,
                    Player2Name = rd.Player2Name
                }
                );
        }
        return new RelationMessages() { Relations = rms };
    }
    public async Task<FRProcessMessage> ProcessFRMessage(string playerId, ApproveFRRequest request)
    {
        var friend = await _repository.GetPlayerByNameAsync(request.FriendName);
        if (friend == null || await _repository.HasSentFriendRequest(friend.PlayerID,playerId))
            return new FRProcessMessage() { Code = PlayerCode.PlayerNotExisted, RequestResult = null, FriendId = null };

        if (await _repository.AreFriends(playerId,friend.PlayerID)){
            return new FRProcessMessage() { Code = PlayerCode.OperationCannotRepeat, RequestResult = null, FriendId = null };
        }

        await _repository.DeleteFriendRequest(friend.PlayerID, playerId);
        await _repository.DeleteFriendRequest(playerId, friend.PlayerID);

        if (!request.Accept)
            return new FRProcessMessage() { Code = PlayerCode.Success, RequestResult = FRCode.Refuse,FriendId = friend.PlayerID };

        await _repository.EstablishRelationship(playerId, friend.PlayerID);

        return new FRProcessMessage() { Code = PlayerCode.Success, RequestResult = FRCode.Accept,FriendId = friend.PlayerID };
    }
    public async Task<RankMessage> GetRank(string playerId, RankRequest request)
    {
        if (request.Type == CheckRankType.Person) {
            var player = await _repository.GetPlayerByIdAsync(playerId);
            var rank = new Dictionary<string, int>();
            rank.Add(
                player?.DisplayName??"",
                await _repository.GetRankPersonally(playerId,(int)request.Mode)
                );
            return new RankMessage
            {
                Mode = request.Mode,
                Score = rank
            };
        }
        else
        {
            var ranks = await _repository.GetRank((int)request.Mode, request.Page ?? 1);
            var newRanks = new Dictionary<string, int>();
            foreach (var rank in ranks)
            {
                newRanks.Add(
                    rank.PlayerName,
                    rank.Score
                    );
            }
            return new RankMessage
            {
                Mode = request.Mode,
                Score = newRanks
            };
        }
    }
    public async Task<FriendMessage> SendMessage(string playerId, FriendMessageRequest request)
    {
        var friend = await _repository.GetPlayerByNameAsync(request.FriendName);
        if (friend == null || friend.PlayerID == playerId)
            return new FriendMessage() { Code = PlayerCode.PlayerNotExisted , FriendId = null };
        if (!await _repository.AreFriends(playerId, friend.PlayerID))
            return new FriendMessage() { Code = PlayerCode.PlayerNotExisted, FriendId = friend.PlayerID };
        return new FriendMessage() { Code = PlayerCode.Success, FriendId = friend.PlayerID };
    }
}
