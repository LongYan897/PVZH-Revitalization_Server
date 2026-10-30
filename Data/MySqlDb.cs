using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace PVZH_Server.Data;
public class MySqlDb(IConfiguration configuration)
{
    public MySqlConnection CreateMySqlConnection()
    {
        return new MySqlConnection(configuration.GetConnectionString("MySql"));
    }
}
