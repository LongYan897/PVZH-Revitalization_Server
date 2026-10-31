namespace PVZH_Server.DTOs;
public class PlayerMessage
{
    public required PlayerCode Code { get; set; }
}

public enum PlayerCode
{
    Success = 0,
    PlayerNotExisted = 1,
    OperationCannotRepeat = 2,
}
