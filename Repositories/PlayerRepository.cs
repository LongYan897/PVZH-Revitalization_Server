using System;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using PVZH_Server.Data;
using PVZH_Server.Models;

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
        string sql = "select PlayerID, DisplayName, Email, Password From Players Where PlayerID = @Id";
        await using var cmd = new MySqlCommand(sql,conn);
        cmd.Parameters.AddWithValue("@Id",PlayerId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var player = new Player() {
                DisplayName = reader.GetString("DisplayName"),
                Email = reader.GetString("Email"),
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
        string sql = "select PlayerID, DisplayName, Email, Password From Players Where PlayerID = @Name";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", PlayerName);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var player = new Player()
            {
                DisplayName = reader.GetString("DisplayName"),
                Email = reader.GetString("Email"),
                Password = reader.GetString("Password"),
                PlayerID = reader.GetString("PlayerID")
            };
            return player;
        }
        return null;
    }
    public async Task<Player?> GetPlayerByEmailAsync(string Email)
    {
        await using var conn = _db.CreateMySqlConnection();
        await conn.OpenAsync();
        string sql = "select PlayerID, DisplayName, Email, Password From Players Where PlayerID = @Email";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", Email);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var player = new Player()
            {
                DisplayName = reader.GetString("DisplayName"),
                Email = reader.GetString("Email"),
                Password = reader.GetString("Password"),
                PlayerID = reader.GetString("PlayerID")
            };
            return player;
        }
        return null;
    }
}
