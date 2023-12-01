using System;
using System.Text;
using BLiveAPI;
using Newtonsoft.Json.Linq;

public class RInspect : IRInspect
{
    private uint cid;
    private ulong roomId;
    private BLiveApi api;
    bool inspected = false;

    public RInspect(uint p_cid, ulong p_roomId)
    {
        cid = p_cid;
        roomId = p_roomId;
        api = new BLiveApi();
    }

    public void Start()
    {
        //建立直播间监听
        Task.Run(new Action(listenRoom));
    }

    public void Stop()
    {
        //先关闭监听
        var task=api.Close();
        task.Wait();
        //再关闭客户端连接
        //分量种情况，一种是先断开connection，一种是先断开inspect需要做判断
        if (ConnectionManager.Instance.ConnTable.ContainsKey(cid))
        {
            ITConnection conn = ConnectionManager.Instance.ConnTable[cid].TcpConn;
            conn.Stop();
        }
    }

    async void listenRoom()
    {
        //30秒后如果未监听成功则强行关闭连接
        _ = Task.Run(async () =>
        {
            // 在异步方法中延迟10秒
            await Task.Delay(30000);
            if (!inspected)
            {
                if (!ConnectionManager.Instance.ConnTable.ContainsKey(cid))
                {
                    Stop();
                    return;
                }
                ITConnection conn = ConnectionManager.Instance.ConnTable[cid].TcpConn;
                try
                {
                    conn.SendMsg(0, new byte[] { 0x03 });
                }
                catch
                {
                    Console.WriteLine("cid={0}发送监听失败信息失败", cid);
                    Stop();
                    return;
                }
                Console.WriteLine("cid={0}监听直播间{1}超时失败", cid, roomId);
                Stop();
            }
        });
        try
        {
            api.OpAuthReply += inspectAuthEvent;
            api.DanmuMsg += DanmuMsgEvent;
            api.SendGift += GiftMsgEvent;
            api.InteractWord += InteractWorldMsgEvent;
            api.SuperChatMessage += SCMsgEvent;
            var task=api.Connect(roomId, 3, GlobalParam.Config.CookieSess);
            task.Wait();
        }
        catch
        {
            if (!ConnectionManager.Instance.ConnTable.ContainsKey(cid))
            {
                Stop();
                return;
            }
            ITConnection conn = ConnectionManager.Instance.ConnTable[cid].TcpConn;
            try
            {
                conn.SendMsg(0, new byte[] { 0x03 });
            }
            catch
            {
                Console.WriteLine("cid={0}监听直播间{1}中断", cid, roomId);
                Stop();
                return;
            }
            Console.WriteLine("cid={0}监听直播间{1}中断", cid, roomId);
            Stop();
        }
    }

    void inspectAuthEvent(object sender, (JObject authReply, ulong? roomId, byte[] rawData) e)
    {
        if (!ConnectionManager.Instance.ConnTable.ContainsKey(cid))
        {
            Stop();
            return;
        }
        ITConnection conn = ConnectionManager.Instance.ConnTable[cid].TcpConn;
        inspected = true;
        try
        {
            conn.SendMsg(0, new byte[] { 0x04 });
        }
        catch
        {
            Console.WriteLine("cid{0}发送认证成功信息失败,可能是因为客户端已掉线");
            Stop();
            return;
        }

        Console.WriteLine("cid={0}监听直播间{1}开始", cid, e.roomId);
    }

    //转发弹幕消息
    void DanmuMsgEvent(object sender, (string msg, ulong userId, string userName, int guardLevel, string face, JObject jsonRawData, byte[] rawData) e)
    {
        if (!ConnectionManager.Instance.ConnTable.ContainsKey(cid))
        {
            Stop();
            return;
        }
        ITConnection conn = ConnectionManager.Instance.ConnTable[cid].TcpConn;
        string jsonStr = e.jsonRawData.ToString();
        byte[] data = Encoding.UTF8.GetBytes(jsonStr);
        try
        {
            conn.SendMsg(2, data);
        }
        catch
        {
            Console.WriteLine("cid{0}、直播间{1}转发失败，客户端连接已中断",cid,roomId);
        }
    }

    //转发礼物消息
    void GiftMsgEvent(object sender,(JObject giftInfo, JObject blindInfo, string coinType, ulong userId, string userName, int guardLevel, string face, JObject jsonRawData, byte[] rawData)e)
    {
        if (!ConnectionManager.Instance.ConnTable.ContainsKey(cid))
        {
            Stop();
            return;
        }
        ITConnection conn = ConnectionManager.Instance.ConnTable[cid].TcpConn;
        string jsonStr = e.jsonRawData.ToString();
        byte[] data = Encoding.UTF8.GetBytes(jsonStr);
        try
        {
            conn.SendMsg(2, data);
        }
        catch
        {
            Console.WriteLine("cid{0}、直播间{1}转发失败，客户端连接已中断", cid, roomId);
        }
    }

    //新观众进入直播间
    void InteractWorldMsgEvent(object sender, (int privilegeType, ulong userId, string userName, JObject jsonRawData, byte[] rawData) e)
    {
        if (!ConnectionManager.Instance.ConnTable.ContainsKey(cid))
        {
            Stop();
            return;
        }
        ITConnection conn = ConnectionManager.Instance.ConnTable[cid].TcpConn;
        string jsonStr = e.jsonRawData.ToString();
        byte[] data = Encoding.UTF8.GetBytes(jsonStr);
        try
        {
            conn.SendMsg(2, data);
        }
        catch
        {
            Console.WriteLine("cid{0}、直播间{1}转发失败，客户端连接已中断", cid, roomId);
        }
    }

    //转发SC信息
    void SCMsgEvent(object sender, (string message, ulong id, int price, ulong userId, string userName, int guardLevel, string face, JObject jsonRawData, byte[] rawData) e)
    {
        if (!ConnectionManager.Instance.ConnTable.ContainsKey(cid))
        {
            Stop();
            return;
        }
        ITConnection conn = ConnectionManager.Instance.ConnTable[cid].TcpConn;
        string jsonStr = e.jsonRawData.ToString();
        byte[] data = Encoding.UTF8.GetBytes(jsonStr);
        try
        {
            conn.SendMsg(2, data);
        }
        catch
        {
            Console.WriteLine("cid{0}、直播间{1}转发失败，客户端连接已中断", cid, roomId);
        }
    }
}

