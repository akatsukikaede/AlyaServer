public class TMessageHandler:ITMsgHandler
{
    Dictionary<uint, ITRouter> apis = new Dictionary<uint, ITRouter>();

    public TMessageHandler()
	{
        apis = new Dictionary<uint, ITRouter>();
	}

    //往api中添加路由
    public void AddRouter(uint msgId, ITRouter router)
    {
        //判断当前api是否已经存在
        if (apis.ContainsKey(msgId))
        {
            throw new Exception("TCP服务器api中已存在对应msgId=" + msgId.ToString() + "的路由，因此无法添加此路由");
        }
        apis.Add(msgId, router);
        Console.WriteLine("TCP服务器添加MsgID=" + msgId.ToString() + "路由成功!");
    }

    //调度/执行对应Router消息处理方法
    public void DoMsgHandler(ITRequest request)
    {
        //从request中取得msgId
        ITRouter handler = apis[request.GetMsgID()];
        if (handler == null)
        {
            Console.WriteLine("TCP服务器api中未能查找到msgId=" + request.GetMsgID().ToString() + "的路由");
            return;
        }
        Console.WriteLine("TCP服务器接收到来自connId="+ request.GetConnection().GetConnID().ToString()+ " msgId=" + request.GetMsgID().ToString() + "的请求");
        //执行路由
        handler.Handle(request);
    }
}


