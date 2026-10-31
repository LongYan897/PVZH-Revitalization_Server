using PVZH_Server.Models;

namespace PVZH_Server.DTOs;
public class CheckFRMessages
{
    public required List<CheckFRMessage> Requests { get; set; }
}
