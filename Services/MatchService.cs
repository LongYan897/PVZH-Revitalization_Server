using PVZH_Server.DTOs;
using PVZH_Server.Models;
using System.Collections.Concurrent;

namespace PVZH_Server.Services;
public interface IMatchService
{
    Task JoinQueue(int mode, PlayerInfo playerInfo, string playerId);
    Task LeaveQueue(string playerId);
    Task<(string? player1Id,string? player2Id,PlayerInfo? player1,PlayerInfo? player2)> TryMatch();
    Task<bool> IsInQueue(string playerId);
    
}
public class MatchService : IMatchService
{
    private readonly ConcurrentDictionary<string, (int Mode, PlayerInfo Info)> _matchQueue 
        = new ConcurrentDictionary<string, (int Mode, PlayerInfo info)>();
    
    public Task<bool> IsInQueue(string playerId)
    {
        return Task.FromResult(_matchQueue.ContainsKey(playerId));
    }
    public Task JoinQueue(int mode, PlayerInfo playerInfo, string playerId)
    {
        _matchQueue[playerId] = (mode,playerInfo);
        return Task.CompletedTask;
    }
    public Task LeaveQueue(string playerId)
    {
        _matchQueue.TryRemove(playerId, out _);
        return Task.CompletedTask;
    }
    public Task<(string? player1Id, string? player2Id, PlayerInfo? player1, PlayerInfo? player2)> TryMatch()
    {
        if (_matchQueue.Count < 2) return Task.FromResult<(string? player1Id, string? player2Id, PlayerInfo? player1, PlayerInfo? player2)>
                ((null, null,null,null));
        var queue = _matchQueue.ToList();
        for (int i = 0; i < queue.Count; i++)
        {
            var player1 = queue[i];

            for (int j = i + 1; j < queue.Count; j++)
            {
                var player2 = queue[j];

                if (player1.Value.Mode == player2.Value.Mode &&
                    Math.Abs(player1.Value.Info.Score - player2.Value.Info.Score) <= 5)
                {
                    if (_matchQueue.TryRemove(player1.Key, out _) && _matchQueue.TryRemove(player2.Key, out _))
                    {
                        return Task.FromResult<(string? player1Id, string? player2Id, PlayerInfo? player1, PlayerInfo? player2)>
                            ((player1.Key,
                            player2.Key,
                            player1.Value.Info,
                            player2.Value.Info));
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return Task.FromResult<(string? player1Id, string? player2Id, PlayerInfo? player1, PlayerInfo? player2)>
            ((null, null, null, null));
    }
}
