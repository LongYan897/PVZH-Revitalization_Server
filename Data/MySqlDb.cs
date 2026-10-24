using MySqlConnector;

namespace PVZH_Server.Data;

/// <summary>
/// MySQL 连接工厂：连接串来自 appsettings.Development.json 的 ConnectionStrings:MySql。<br/>
/// 用法：await using var conn = await db.OpenAsync();
/// </summary>
public class MySqlDb(IConfiguration config)
{
    private readonly string _connStr = config.GetConnectionString("MySql")
        ?? throw new InvalidOperationException("缺少连接串 ConnectionStrings:MySql（见 appsettings.Development.json）");

    public async Task<MySqlConnection> OpenAsync()
    {
        var conn = new MySqlConnection(_connStr);
        await conn.OpenAsync();
        return conn;
    }
}
