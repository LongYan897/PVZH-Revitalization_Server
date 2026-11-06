using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PVZH_Server.DTOs;
using PVZH_Server.Services;
using System.Security.Claims;

namespace PVZH_Server.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly IPlayerService _playerService;
    private readonly IMatchService _matchService;
    public GameHub(IPlayerService playerService, IMatchService matchService)
    {
        _playerService = playerService;
        _matchService = matchService;
    }
    private string UserId => Context.UserIdentifier ?? "";
    private string UserName => Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "未知玩家";
    public async Task<FriendRMessage> SendFriendRequest(FriendRequest request)
    {
        Console.WriteLine($"正在发送请求 {request.FriendName}");
        var message = await _playerService.SendFriendRequest(UserId, request);
        if (message.Code == PlayerCode.Success) await Clients.User(message.FriendId??"").SendAsync("CheckFriendRequest");
        return new FriendRMessage()
        { Code = message.Code, FriendId = null };
    }
    public async Task<CheckFRMessages> GetFriendRequest()
    {
        Console.WriteLine($"正在获取好友请求，请求者{UserName}");
        return await _playerService.CheckFriendRequest(UserId);
    }
    public async Task<FRProcessMessage> ApproveFriendRequest(ApproveFRRequest request)
    {
        Console.WriteLine($"正在处理请求，请求对象{request.FriendName}, 请求者{UserName}");
        var frpm = await _playerService.ProcessFRMessage(UserId,request);
        if (frpm.RequestResult == FRCode.Accept) 
            await Clients.User(frpm.FriendId??"").SendAsync("ApproveFriendRequest",true);
        else await Clients.User(frpm.FriendId ?? "").SendAsync("ApproveFriendRequest", false);
        Console.WriteLine($"请求处理完毕，请求码{frpm.Code}，结果{frpm.RequestResult}");
        return new FRProcessMessage()
        {
            Code = frpm.Code,
            FriendId = null,
            RequestResult = frpm.RequestResult
        };
    }
    public async Task<RelationMessages> GetRelation() {
        Console.WriteLine($"玩家{UserName}请求查看关系");
        return await _playerService.GetRelationAsync(UserId); 
    } 
    public async Task<RankMessage> GetRank(RankRequest request)
    {
        Console.WriteLine($"玩家{UserName}请求查看排行榜");
        return await _playerService.GetRank(UserId, request);
    }
    public async Task<FriendMessage> SendMessage(FriendMessageRequest request)
    {
        Console.WriteLine($"玩家{UserName}请求发送消息给{request.FriendName}");
        var result = await _playerService.SendMessage(UserId, request);
        if (result.Code == PlayerCode.Success)
        {
            await Clients.User(result.FriendId ?? "")
                .SendAsync("ReceiveMessage",request.Message, UserName);
        }
        return new FriendMessage()
        { Code = result.Code, FriendId = null };
    }
    public async Task JoinMatch(MatchRequest request)
    {
        try
        {
            Console.WriteLine($"用户{UserName}尝试加入匹配，匹配模式：{request.Mode}");
            if (await _matchService.IsInQueue(UserId))
            {
                await _matchService.LeaveQueue(UserId);
            }
            var rank = await _playerService.GetRank(UserId,
                new RankRequest()
                {
                    Mode = request.Mode,
                    Type = CheckRankType.Person,
                    Page = null
                });
            await _matchService.JoinQueue(
                (int)request.Mode,
                new Models.PlayerInfo()
                {
                    DisplayName = UserName,
                    Score = rank.Score.TryGetValue(UserId, out var score) ? score : 0,
                    ConnectionId = Context.ConnectionId
                }
                , UserId);
            Console.WriteLine($"请求处理完毕，加入队列？：{await _matchService.IsInQueue(UserId)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"玩家{UserName}请求加入匹配时发生异常: {ex.Message}");
        }
    }
    public async Task LeaveMatch()
    {
        await _matchService.LeaveQueue(UserId);
    }
}