/// <summary>
/// 服务器接口
/// </summary>
public interface ITServer
{
    //启动
    void Start();
    //停止
    void Stop();
    //运行
    void Serve();
    //路由功能，给当前服务注册一个路由方法，供客户端连接处理使用
    void AddRouter(uint msgId, ITRouter router);
       
}


