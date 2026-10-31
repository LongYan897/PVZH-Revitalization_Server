namespace PVZH_Server.DTOs;
public class FRProcessMessage : PlayerMessage
{
    public required FRCode? RequestResult { get; set; }
    public required string? FriendId { get; set; } = null;
}

public enum FRCode
{
    Accept = 0,
    Refuse = 1
}