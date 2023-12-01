
/// <summary>
/// 消息管理抽象接口
/// </summary>
public interface ITMsgHandler
{
    void DoMsgHandler(ITRequest request);

    void AddRouter(uint msgId, ITRouter router);
}
