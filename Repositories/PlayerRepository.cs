using MySqlConnector;
using PVZH_Server.Data;
using PVZH_Server.Models;
using Dapper;
using PVZH_Server.DTOs;
using System.Text;

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
    Task<List<(string PlayerId,string PlayerName,int Score)>> GetRank(int mode,int page);
    Task UpdateRank(string playerId, int newRank ,int mode);
    Task<int> GetRankPersonally(string playerId, int mode);
    Task<bool> AreFriends(string player1Id,string player2Id);
    Task<bool> HasSentFriendRequest(string requesterId,string friendId);
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
        string sql = "select * From players Where PlayerID = @Id";
        await using var cmd = new MySqlCommand(sql,conn);
        cmd.Parameters.AddWithValue("@Id",playerId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var player = new Player()
            {
                DisplayName = reader.GetString("DisplayName"),
                Password = reader.GetString("Password"),
                PlayerID = reader.GetString("PlayerID"),
                Diamond = reader.GetInt32("Diamond")
            };
            return player;
        }
        return null;
    }
    public async Task<Player?> GetPlayerByNameAsync(string playerName)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.OpenAsync();
        string sql = "select * From players Where DisplayName = @Name";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", playerName);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var player = new Player()
            {
                DisplayName = reader.GetString("DisplayName"),
                Password = reader.GetString("Password"),
                PlayerID = reader.GetString("PlayerID"),
                Diamond = reader.GetInt32("Diamond")
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
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task SendFriendRequest(string senderId,string recipientId)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.ExecuteAsync(
            "insert into friendrequests (FRID,RequesterID,FriendID) value (@Id,@sender,@friend)",
            new {Id = Guid.NewGuid().ToString(),sender = senderId,friend = recipientId}
            );
    }
    public async Task<List<FriendRequestData>> GetFriendRequest(string playerId)
    {
        await using var conn = _db.CreateMySqlConnection();
        var rqs = await conn.QueryAsync<FriendRequestData>(
            """
            select
            frq.RequesterID as senderId,
            frq.FriendID as recipientId,
            rq.DisplayName as senderName,
            fr.DisplayName as recipientName
            from friendrequests frq
            inner join players rq on rq.PlayerID = frq.RequesterID
            inner join players fr on fr.PlayerID = frq.FriendID
            where fr.PlayerID = @playerId
            """
            ,
            new {playerId}
            );
        return [..rqs];
    }
    public async Task DeleteFriendRequest(string senderId,string recipientId)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.ExecuteAsync(
            """
            delete
            from friendrequests
            where RequesterID = @senderId and FriendID = @recipientId
            """,
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
        var rs = await conn.QueryAsync<RelationData>(
            """
            select
            r.Player1 as Player1Id,
            r.Player2 as Player2Id,
            p1.DisplayName as Player1Name,
            p2.DisplayName as Player2Name
            from relationship r
            inner join players p1 on p.PlayerID = r.Player1
            inner join players p2 on p.PlayerID = r.Player2
            where r.Player1 = @playerId or r.Player2 = @playerId
            """,
            new {playerId}
            );
        return [.. rs];
    }
    public async Task<int> GetRankPersonally(string playerId,int mode = 1)
    {
        await using var conn = _db.CreateMySqlConnection();
        var rank = await conn.QueryFirstOrDefaultAsync<int?>(
            "select Score from `rank` where PlayerID = @playerId and Mode = @mode",
            new { playerId,mode }
        );
        return rank ?? 0;
    }
    public async Task UpdateRank(string playerId, int newRank, int mode = 1)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.ExecuteAsync(
            "insert into `rank` (PlayerID,Score,Mode) value (@playerId,@newRank,@mode) on duplicate key update score = @newRank",
            new {playerId,newRank,mode }
            );
    }
    public async Task<List<(string PlayerId, string PlayerName, int Score)>> GetRank(int mode, int page)
    {
        int offset = (page - 1) * 50;
        await using var conn = _db.CreateMySqlConnection();
        var ranks = await conn.QueryAsync<(string PlayerId, string PlayerName, int Score)>(
            """
            select
            r.Score as Score,
            r.PlayerID as PlayerID,
            p.DisplayName as PlayerName
            from `rank` r
            inner join players p on r.PlayerID = p.PlayerID
            where Mode = @mode
            order by Score Desc
            limit 50 offset @offset
            """,
            new { mode, offset }
            );
        return [..ranks];
    }
    public async Task<bool> AreFriends(string player1Id, string player2Id)
    {
        await using var conn = _db.CreateMySqlConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            select
            count(1)
            from relationship
            where (Player1 = @player1Id and Player2 = @player2Id) 
            or (Player1 = @player2Id and Player2 = @player1Id)
            """,
            new {player1Id,player2Id }
            ) > 0;
    }
    public async Task<bool> HasSentFriendRequest(string requesterId, string friendId)
    {
        await using var conn = _db.CreateMySqlConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            select
            count(1)
            from friendrequests
            where RequesterID = @requesterId and FriendID = @friendId
            """,
            new {requesterId,friendId}
            ) > 0;
    }
}
