namespace PVZH_Server.DTOs;
public class RankRequest
{
    public required MatchType Mode { get; set; }
    public required CheckRankType Type { get; set; }
    public required int? Page { get; set; }
}

public enum CheckRankType
{
    Person = 0,
    Ranked = 1
}