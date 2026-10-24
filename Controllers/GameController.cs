using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using PVZH_Server.Data;

namespace PVZH_Server.Controllers
{
    // 1. 定义一个类，用来接收客户端发来的 JSON 数据
    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly MySqlDb _db;

        public GameController(MySqlDb db)
        {
            _db = db;
        }

        // 之前的测试接口保留着，方便以后排查网络
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("服务器连通成功！");
        }

        // 数据库连通性检查：打开连接数一下 users 表
        [HttpGet("ping-db")]
        public async Task<IActionResult> PingDb()
        {
            try
            {
                await using var conn = await _db.OpenAsync();
                await using var cmd = new MySqlCommand("SELECT COUNT(*) FROM users", conn);
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return Ok(new { Success = true, Message = "数据库连接成功", UserCount = count });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { Success = false, Message = "数据库连接失败：" + e.Message });
            }
        }

        // 2. 登录接口：去 MySQL 的 users 表里查
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { Success = false, Message = "账号或密码不能为空" });
            }

            await using var conn = await _db.OpenAsync();
            await using var cmd = new MySqlCommand(
                "SELECT id, username, display_name, gold FROM users WHERE username=@u AND password=@p LIMIT 1",
                conn);
            cmd.Parameters.AddWithValue("@u", request.Username);
            cmd.Parameters.AddWithValue("@p", request.Password);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Ok(new
                {
                    Success = true,
                    Message = "登录成功",
                    UserId = reader.GetInt32(reader.GetOrdinal("id")),
                    DisplayName = reader.GetString(reader.GetOrdinal("display_name")),
                    Gold = reader.GetInt32(reader.GetOrdinal("gold"))
                });
            }

            // 返回 400 错误码和提示
            return BadRequest(new
            {
                Success = false,
                Message = "账号或密码错误"
            });
        }
    }
}
