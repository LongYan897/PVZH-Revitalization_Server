using Microsoft.AspNetCore.SignalR;
using PVZH_Server.Hubs;

namespace PVZH_Server.Services;
public class MatchManager : BackgroundService
{
    private readonly IMatchService _matchService;
    private readonly IBattleService _battleService;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly ILogger<MatchManager> _logger;
    public MatchManager(IMatchService matchService, IHubContext<GameHub> hubContext, ILogger<MatchManager> logger,IBattleService battleService)
    {
        _matchService = matchService;
        _hubContext = hubContext;
        _logger = logger;
        _battleService = battleService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("匹配系统启动");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var match = _matchService.TryMatch().Result;
                if (match.player1 != null &&
                    match.player2 != null &&
                    match.player1Id != null &&
                    match.player2Id != null)
                {
                    _logger.LogInformation($"发现比赛: {match.player1.DisplayName} vs {match.player2.DisplayName}");
                    var roomId = Guid.NewGuid().ToString();
                    await _battleService.AddRoom(roomId, match.player1Id, match.player2Id, match.player1, match.player2);
                    await _hubContext.Groups.AddToGroupAsync(match.player1.ConnectionId, roomId);
                    await _hubContext.Groups.AddToGroupAsync(match.player2.ConnectionId, roomId);
                    await _hubContext.Clients.User(match.player1Id).SendAsync("MatchFound", match.player2.DisplayName, roomId);
                    await _hubContext.Clients.User(match.player2Id).SendAsync("MatchFound", match.player1.DisplayName, roomId);
                }
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "错误，等待5s");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
