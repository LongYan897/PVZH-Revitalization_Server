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
    public GameHub(IPlayerService playerService)
    {
        _playerService = playerService;
    }
    public async Task<FriendRMessage> SendFriendRequest(FriendRequest request)
    {
        Console.WriteLine($"正在发送请求 {request.FriendName}");
        var message = await _playerService.SendFriendRequest(Context.UserIdentifier??"", request);
        if (message.Code == PlayerCode.Success) await Clients.User(message.FriendId??"").SendAsync("CheckFriendRequest");
        return new FriendRMessage()
        { Code = message.Code, FriendId = null };
    }
    public async Task<CheckFRMessages> GetFriendRequest()
    {
        Console.WriteLine($"正在获取好友请求，请求者{Context.UserIdentifier ?? ""}");
        return await _playerService.CheckFriendRequest(Context.UserIdentifier ?? "");
    }
    public async Task<FRProcessMessage> ApproveFriendRequest(ApproveFRRequest request)
    {
        Console.WriteLine($"正在处理请求，请求对象{request.FriendName}, 请求者{Context.User?.FindFirst(ClaimTypes.Name)?.Value}");
        var frpm = await _playerService.ProcessFRMessage(Context.UserIdentifier??"",request);
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
        Console.WriteLine($"玩家{Context.UserIdentifier}请求查看关系");
        return await _playerService.GetRelationAsync(Context.UserIdentifier ?? ""); 
    }
}