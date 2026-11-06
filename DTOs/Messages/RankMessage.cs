namespace PVZH_Server.DTOs;
public class RankMessage
{
    public required Dictionary<string,int> Score { get; set; }
    public required MatchType Mode { get; set; }
}
