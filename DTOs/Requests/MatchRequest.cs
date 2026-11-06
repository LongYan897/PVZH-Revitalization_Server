using PVZH_Server.Models;

namespace PVZH_Server.DTOs;
public class MatchRequest
{
    public required MatchType Mode { get; set; }
}

public enum MatchType
{
    Relax = 0,
    Rank = 1
}