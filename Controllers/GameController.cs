using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg.Sig;
using PVZH_Server.DTOs;
using PVZH_Server.Repositories;
using PVZH_Server.Services;
using System.Security.Claims;

namespace PVZH_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IPlayerService _playerService;
    public GameController(IPlayerService playerService)
    {
        _playerService = playerService;
    }
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] DTOs.RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.DisplayName) || string.IsNullOrWhiteSpace(request.DisplayName))
            return Ok(
                new LoginMessage() {
                    Code = LoginCode.RegisterNameCannotBeWhiteSpace,
                    DisplayName = request.DisplayName,
                    Token = null
                }
            );
        return Ok(await _playerService.Register(request));
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] DTOs.LoginRequest request)
    {
        return Ok(await _playerService.LoginByName(request));
    }
    [Authorize]
    [HttpGet("get/profile")]
    public async Task<IActionResult> GetProfile()
    {
        var playerId = User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(playerId)) return Unauthorized();
        return Ok(await _playerService.GetPlayerAsync(playerId));
    }
}
