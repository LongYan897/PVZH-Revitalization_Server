using System;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using PVZH_Server.Data;
using PVZH_Server.Models;
using Dapper;

namespace PVZH_Server.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private MySqlDb _db;
    public PlayerRepository(MySqlDb db)
    {
        _db = db;
    }
    public async Task<Player?> GetPlayerByIdAsync(string PlayerId)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.OpenAsync();
        string sql = "select PlayerID, DisplayName, Password From players Where PlayerID = @Id";
        await using var cmd = new MySqlCommand(sql,conn);
        cmd.Parameters.AddWithValue("@Id",PlayerId);

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
    public async Task<Player?> GetPlayerByNameAsync(string PlayerName)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.OpenAsync();
        string sql = "select PlayerID, DisplayName, Password From players Where DisplayName = @Name";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", PlayerName);

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
}
