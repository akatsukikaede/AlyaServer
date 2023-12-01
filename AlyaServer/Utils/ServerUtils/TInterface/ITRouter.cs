/// <summary>
/// 路由抽象接口
/// 路由里的数据全部都是ITRequest
/// </summary>
public interface ITRouter
{
    //处理conn业务的主hook方法
    void Handle(ITRequest request);
}

