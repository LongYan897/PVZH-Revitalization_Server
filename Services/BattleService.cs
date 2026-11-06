using PVZH_Server.Models;
using System.Collections.Concurrent;

namespace PVZH_Server.Services;
public interface IBattleService
{
    Task AddRoom(string roomId, string player1Id, string player2Id, PlayerInfo player1, PlayerInfo player2);
    Task RemoveRoom(string roomId);
    Task<string> GetRoom(string playerId);
}
public class BattleService : IBattleService
{
    private readonly List<Room> _rooms
        = new List<Room>();
    public Task AddRoom(string roomId, string player1Id, string player2Id, PlayerInfo player1, PlayerInfo player2)
    {
        _rooms.Add(new Room()
        {
            Id = roomId,
            player1 = (player1Id, player1),
            player2 = (player2Id, player2)
        });
        return Task.CompletedTask;
    }

    public Task<string> GetRoom(string playerId)
    {
        var room = _rooms.FirstOrDefault(r => r.player2.Id == playerId || r.player1.Id == playerId);
        return Task.FromResult<string>(room?.Id ?? string.Empty);
    }

    public Task RemoveRoom(string roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room != null)
        {
            _rooms.Remove(room);
        }
        return Task.CompletedTask;
    }
}
