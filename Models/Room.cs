namespace PVZH_Server.Models;
public class Room
{
    public required string Id { get; set; }
    public required (string Id, PlayerInfo Info) player1 { get; set; }
    public required (string Id, PlayerInfo Info) player2 { get; set; }
}
