public class ClientAuthRouter : ITRouter
{
    public void Handle(ITRequest request)
    {
        //取得数据
        byte[] data = request.GetData();
        uint cid = request.GetConnection().GetConnID();
        ITConnection conn = request.GetConnection();
        int addResult = 0;
        ulong appId = 0;
        ulong roomId = 0;
        try
        {
            appId = BitConverter.ToUInt64(data, 0);
            roomId = BitConverter.ToUInt64(data, 8);
        }
        catch (Exception ex)
        {
            Console.WriteLine("CID=" + cid.ToString() + " 客户软认证发生错误：" + ex.Message);
            return;
        }
        //AppID校验，需要重写
        //if (appId > 65535)
        //{
        //    //不合法端口
        //    byte[] writeBackPack = new byte[] { 0x02};
        //    conn.SendMsg(0, writeBackPack);
        //    return;
        //}
        ConnectionData connData = new ConnectionData() { TcpConn = conn, AppID = appId,RoomID=roomId, LastActiveTime = DateTime.Now };
        //从待连接队列移除
        ConnectionManager.Instance.RemoveConnFromQueue(conn);
        //添加到实际连接队列中
        addResult = ConnectionManager.Instance.AddConnData(cid, connData);
        switch (addResult)
        {
            //认证成功
            case 0:
                byte[] resultBytes = BitConverter.GetBytes(addResult);
                byte[] writeBackPack = new byte[5];
                writeBackPack[0] = 0x00;
                Buffer.BlockCopy(resultBytes, 0, writeBackPack, 1, 4);
                conn.SendMsg(0,writeBackPack);
                //建立监听
                IRInspect rInspect = new RInspect(cid, roomId);
                //加入监听管理
                RoomInspectManager.Instance.AddInspect(cid, rInspect);
                break;
            //已达最大承载量
            case 1:
                writeBackPack = new byte[] { 0x01};
                conn.SendMsg(0, writeBackPack);
                break;
        }
    }
}

