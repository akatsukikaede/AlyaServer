using System.Net;
using System.Net.Sockets;
using System.Text;

class ServerEntity
{
    static void Main(string[] args)
    {
        //全局参数初始化
        GlobalParam.ReadFromFile();
        //TCP服务器初始化
        ITServer tServer = new TServer("tserver!", "0.0.0.0", GlobalParam.Config.ServerPort, new TMessageHandler());
        tServer.AddRouter(0, new ClientAuthRouter());
        tServer.AddRouter(1, new HeartBeatRouter());
        tServer.Serve();
        Console.ReadLine();
    }

}