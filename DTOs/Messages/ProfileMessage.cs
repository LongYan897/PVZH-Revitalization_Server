namespace PVZH_Server.DTOs;
public class ProfileMessage : PlayerMessage
{
    public required string? DisplayName { get; set; }
    public required int Diamond { get; set; }
}
