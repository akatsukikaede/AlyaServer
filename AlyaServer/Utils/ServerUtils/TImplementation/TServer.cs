using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;

public class TServer : ITServer
{
    string name="TServer";
    //string ipVersion="tcp";
    string ip="0.0.0.0";
    int port=9000;
    ITMsgHandler msgHandler;

    public TServer(string p_name, string p_ip, int p_port, ITMsgHandler p_msgHandler)
    {
        name = p_name;
        ip = p_ip;
        port = p_port;
        msgHandler = p_msgHandler;
    }

    public void AddRouter(uint msgId, ITRouter router)
    {
        msgHandler.AddRouter(msgId, router);
        Console.WriteLine("添加路由成功，msgId=" + msgId.ToString());
    }

    public void Serve()
    {
        //启动服务器
        Start();
        //启动连接管理系统
        ConnectionManager.Instance.RunCheckTimer();
    }

    public void Start()
    {
        Console.WriteLine("服务器:{0},  IP:{1}, 端口:{2} 启动中...",name, ip, port);
        //还需打印基本信息
        Task.Run(new Action(listenClientConn));
    }

    public void Stop()
    {
        //TODO 将服务器资源、状态以及开辟的链接信息进行停止或者回收
    }

    //创建服务
    Socket CreateServer()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress address = IPAddress.Parse(ip);
        IPEndPoint endPoint = new IPEndPoint(address, port);
        socket.Bind(endPoint);
        socket.Listen(20);
        return socket;
    }

    void listenClientConn()
    {
        Socket serverSocket;
        try
        {
            serverSocket = CreateServer();
        }
        catch (Exception ex)
        {
            Console.WriteLine("创建TCP服务发生错误：" + ex.ToString());
            return;
        }
        uint cid = 0;
        Console.WriteLine("创建TCP服务成功：{0}, 开始监听...",name);
        for (; ; )
        {
            Socket clientSocket;
            try
            {
                clientSocket=serverSocket.Accept();
            }
            catch (Exception ex)
            {
                Console.Write("接收连接错误:" + ex.ToString());
                continue;
            }
            ITConnection dealConn = new TConnection(clientSocket, cid, msgHandler, new TDataPacker());
            //加入连接队列
            ConnectionManager.Instance.AddConnToQueue(dealConn);
            cid++;
            Task.Run(new Action(dealConn.Start));
        }
    }
}

