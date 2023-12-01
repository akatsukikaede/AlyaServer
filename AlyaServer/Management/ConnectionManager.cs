using System.Timers;

public class ConnectionData
{
    //TCP客户端连接
    public required ITConnection TcpConn;
    //AppID,由系统管理，未经验证的AppID无法进行使用
    public ulong AppID;
    //RoomID，监听的房间ID
    public ulong RoomID;
    //上次激活时间戳
    public DateTime LastActiveTime;
}

public class ConnectionPara
{
    //最大并发数
    public int MaxCons=10;
}

//连接管理组件，通过单例模式调用，在系统中有且只能有一个
public class ConnectionManager
{
    //单例模式调用
    private static ConnectionManager instance = new ConnectionManager();
    /// <summary>
    /// 连接管理组件对外单例调用
    /// </summary>
    public static ConnectionManager Instance
    {
        get
        {
            return instance;
        }
    }
    //用于处理已连接用户的线程锁
    private object locker = new object();
    //用于处理待连接用户的线程锁
    private object locker2 = new object();
    private System.Timers.Timer checkTimer=new System.Timers.Timer();
    //Conn表，以ConnID为Key
    public Dictionary<uint, ConnectionData> ConnTable;
    //待连接队列
    private List<ITConnection> ConnQueue;
    //参数
    public ConnectionPara Para;
    //构造函数
    public ConnectionManager()
    {
        ConnTable = new Dictionary<uint, ConnectionData>();
        ConnQueue = new List<ITConnection>();
        Para = new ConnectionPara();
    }

    /// <summary>
    /// 加入连接队列
    /// </summary>
    /// <param name="conn">待加入的连接</param>
    public void AddConnToQueue(ITConnection conn)
    {
        lock (locker2)
        {
            ConnQueue.Add(conn);
        }
        //10秒后如果连接还在队列则踢掉这个连接
        _ = Task.Run(async () =>
        {
            // 在异步方法中延迟10秒
            await Task.Delay(10000);
            lock (locker2)
            {
                if (ConnQueue.Contains(conn))
                {
                    conn.Stop();
                    ConnQueue.Remove(conn);
                    Console.WriteLine("CID=" + conn.GetConnID().ToString() + "认证超时，已停止连接");
                }
            }
        });
    }

    /// <summary>
    /// 移除出连接队列
    /// </summary>
    /// <param name="conn"></param>
    public void RemoveConnFromQueue(ITConnection conn)
    {
        lock (locker2)
        {
            ConnQueue.Remove(conn);
        }
    }

    /// <summary>
    /// 接收到心跳帧，激活对应的Conn
    /// </summary>
    /// <param name="connId"></param>
    /// <returns>返回结果 0：激活成功 1：CID不存在</returns>
    public int ActiveConnData(uint connId)
    {
        lock (locker)
        {
            if (!ConnTable.ContainsKey(connId))
            {
                Console.WriteLine("不存在CID=" + connId.ToString() + "的连接，该连接可能已掉线");
                return 1;
            }
            ConnTable[connId].LastActiveTime = DateTime.Now;
            Console.WriteLine("CID=" + connId.ToString() + "连接状态更新,时间戳：" + ConnTable[connId].LastActiveTime.ToString("yyyy-MM-dd-HH:mm:ss"));
            return 0;
        }
    }

    /// <summary>
    /// 添加Conn
    /// </summary>
    /// <param name="connId">连接Id</param>
    /// <param name="connData">连接数据</param>
    /// <returns>添加结果，1表示异常，0表示添加成功</returns>
    public int AddConnData(uint connId, ConnectionData connData)
    {
        lock (locker)
        {
            //连接数超限
            if (ConnTable.Keys.Count >= Para.MaxCons)
            {
                Console.WriteLine("当前服务器已达到最大承载量，无法加入");
                return 1;
            }
            //连接已存在
            if (ConnTable.ContainsKey(connId))
            {
                Console.WriteLine("已存在CID=" + connId.ToString() + "的连接，添加失败");
                return 2;
            }
            ConnTable.Add(connId, connData);
            Console.WriteLine("CID=" + connId.ToString() + "连接添加成功");
            return 0;
        }
    }

    //执行定期连接列表检查
    public void RunCheckTimer()
    {
        Console.WriteLine("连接管理系统启动中.....");
        //设置定时间隔(毫秒为单位)
        int interval = 30000;
        checkTimer = new System.Timers.Timer(interval);
        //设置执行一次（false）还是一直执行(true)
        checkTimer.AutoReset = true;
        //设置是否执行System.Timers.Timer.Elapsed事件
        checkTimer.Enabled = true;
        //绑定Elapsed事件
        checkTimer.Elapsed += checkConn;
        //启动计时器
        checkTimer.Start();
        Console.WriteLine("连接管理系统已启动");
    }

   /// <summary>
   /// 检查连接的时效性，如果超时则踢掉该连接,超时时限为30s,该方法每30秒执行一次
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>

    private void checkConn(Object source, ElapsedEventArgs e)
    {
        lock (locker)
        {
            //用于存储过期的cid
            List<uint> deprecatedCid = new List<uint>();
            DateTime curTime = DateTime.Now;
            foreach (uint cid in ConnTable.Keys)
            {
                DateTime clientTime = ConnTable[cid].LastActiveTime;
                //超时连接
                if ((curTime - clientTime).TotalSeconds > 30)
                {
                    deprecatedCid.Add(cid);
                }
            }
            Console.WriteLine("当前在线客户端"+ConnTable.Keys.Count()+"个，时间戳：" + curTime.ToString("yyyy-MM-dd-HH:mm:ss"));
            //踢掉超时连接
            for (int i = 0; i < deprecatedCid.Count; i++)
            {
                //停掉该连接
                ConnTable[deprecatedCid[i]].TcpConn.Stop();
                DateTime lastTime = ConnTable[deprecatedCid[i]].LastActiveTime;
                //从列表中移除
                ConnTable.Remove(deprecatedCid[i]);
                //从监听中移除
                RoomInspectManager.Instance.RemoveInspect(deprecatedCid[i]);
                Console.WriteLine("CID=" + deprecatedCid[i].ToString() + "连接已离线,上次在线时间：" + lastTime.ToString("yyyy-MM-dd-HH:mm:ss"));
            }
        }
    }
}



