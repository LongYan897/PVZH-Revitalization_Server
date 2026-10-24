using PVZH_Server.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. 注册控制器服务
builder.Services.AddControllers();

// 2. 注册 Swagger 服务 (把 AddEndpointsApiExplorer 删掉，SwaggerGen 包含了它)
builder.Services.AddSwaggerGen();

// 3. MySQL 连接（连接串在 appsettings.Development.json 的 ConnectionStrings:MySql）
builder.Services.AddSingleton<MySqlDb>();

// 允许所有跨域请求（仅用于开发测试）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();

// 4. 配置 Swagger 测试页面
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 本地开发只跑 http（见 launchSettings 的 http profile）：
// 打开 HttpsRedirection 会把 http 请求 307 重定向到 https，
// 而 Godot 的 HttpRequest 不会自动跟随重定向，客户端只会拿到 307。
// 正式部署需要 https 时再打开这一行。
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
