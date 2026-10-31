using MySqlConnector;
using PVZH_Server.Data;
using PVZH_Server.Models;
using Dapper;
using PVZH_Server.DTOs;

namespace PVZH_Server.Repositories;

public interface IPlayerRepository
{
    Task<Player?> GetPlayerByIdAsync(string playerId);
    Task<Player?> GetPlayerByNameAsync(string DisplayName);
    Task AddPlayer(string playerName, string password, string playerId);
    Task SendFriendRequest(string senderId, string recipientId);
    Task<List<FriendRequestData>> GetFriendRequest(string playerId);
    Task DeleteFriendRequest(string senderId, string recipientId);
    Task EstablishRelationship(string player1Id,string player2Id);
    Task<List<RelationData>> GetRelationsAsync(string playerId);

}
public class PlayerRepository : IPlayerRepository
{
    private MySqlDb _db;
    public PlayerRepository(MySqlDb db)
    {
        _db = db;
    }
    public async Task<Player?> GetPlayerByIdAsync(string playerId)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.OpenAsync();
        string sql = "select PlayerID, DisplayName, Password From players Where PlayerID = @Id";
        await using var cmd = new MySqlCommand(sql,conn);
        cmd.Parameters.AddWithValue("@Id",playerId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var player = new Player() {
                DisplayName = reader.GetString("DisplayName"),
                Password = reader.GetString("Password"),
                PlayerID = reader.GetString("PlayerID")
            };
            return player;
        }
        return null;
    }
    public async Task<Player?> GetPlayerByNameAsync(string playerName)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.OpenAsync();
        string sql = "select PlayerID, DisplayName, Password From players Where DisplayName = @Name";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", playerName);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var player = new Player()
            {
                DisplayName = reader.GetString("DisplayName"),
                Password = reader.GetString("Password"),
                PlayerID = reader.GetString("PlayerID")
            };
            return player;
        }
        return null;
    }

    public async Task AddPlayer(string playerName,string password,string playerId)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.OpenAsync();
        string sql = "insert into players (DisplayName,Password,PlayerID) value (@playerName,@password,@playerId)";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@playerName",playerName);
        cmd.Parameters.AddWithValue("@password", password);
        cmd.Parameters.AddWithValue("@playerId", playerId);

        await cmd.ExecuteReaderAsync();
    }
    public async Task SendFriendRequest(string senderId,string recipientId)
    {
        await using var conn = _db.CreateMySqlConnection();
        var result = await conn.ExecuteAsync(
            "insert into friendrequests (FRID,RequesterID,FriendID) value (@Id,@sender,@friend)",
            new {Id = Guid.NewGuid().ToString(),sender = senderId,friend = recipientId}
            );
        Console.WriteLine($"处理{result}行");
    }
    public async Task<List<FriendRequestData>> GetFriendRequest(string playerId)
    {
        await using var conn = _db.CreateMySqlConnection();
        var query = await conn.QueryAsync(
            "select RequesterID,FriendID from friendRequests where FriendID = @playerId",
            new {playerId}
            );
        var requests = new List<FriendRequestData>();
        foreach (var row in query)
        {
            requests.Add(
                new FriendRequestData() { 
                    recipientId = row.FriendID,
                    senderId = row.RequesterID
                }
                );
        }
        return requests;
    }
    public async Task DeleteFriendRequest(string senderId,string recipientId)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.ExecuteAsync(
            "delete from friendrequests where RequesterID = @senderId and FriendID = @recipientId",
            new { senderId,recipientId}
            );
    }
    public async Task EstablishRelationship(string player1Id, string player2Id)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.ExecuteAsync(
            "insert into relationship (ID,Player1,Player2) value (@Id,@player1,@player2)",
            new {Id = Guid.NewGuid().ToString(),player1 = player1Id,player2 = player2Id}
            );
    }
    public async Task<List<RelationData>> GetRelationsAsync(string playerId)
    {
        await using var conn = _db.CreateMySqlConnection();
        var query = await conn.QueryAsync(
            "select * from relationship where Player1 = @playerId or Player2 = @playerId",
            new {playerId}
            );
        var rds = new List<RelationData>();
        foreach (var row in query)
        {
            rds.Add(
                new RelationData()
                {
                    Player1Id = row.Player1,
                    Player2Id = row.Player2
                }
                );
        }
        return rds;
    }
}
