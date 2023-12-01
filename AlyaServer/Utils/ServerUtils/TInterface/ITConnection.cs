using System.Net;
using System.Net.Sockets;

/// <summary>
/// 定义连接模块的抽象层
/// </summary>
public interface ITConnection
{
	//启动连接，让当前连接开始工作
	void Start();
	//停止链接，结束当前连接工作
	void Stop();
	//获取当前连接所绑定的socket conn
	Socket GetTCPConnection();
	//获取当前连接模块的连接id
	uint GetConnID();
	//获取远程客户端的ip地址
	IPEndPoint? RemoteAddr();
	//发送数据，将数据发送给远程客户端
	void SendMsg(uint msgId, byte[] data);
}

