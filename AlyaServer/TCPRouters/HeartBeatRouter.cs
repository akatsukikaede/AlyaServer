using System;

//心跳包路由
public class HeartBeatRouter : ITRouter
{
    public void Handle(ITRequest request)
    {
        ITConnection conn = request.GetConnection();
        //刷新连接状态
        uint cid = conn.GetConnID();
        int activeResult = ConnectionManager.Instance.ActiveConnData(cid);
        //如果连接已被踢除则无需回传
        if (activeResult == 1)
        {
            return;
        }
        //不做具体处理，直接回传
        byte[] writeBackData = new byte[] { 0x00 };
        conn.SendMsg(1, writeBackData);
    }
}


