namespace PVZH_Server.Models;
public class FriendRequestData
{
    public required string recipienName { get; set; }
    public required string senderName { get; set; } 
    public required string recipientId { get; set; }
    public required string senderId { get; set; }
}
