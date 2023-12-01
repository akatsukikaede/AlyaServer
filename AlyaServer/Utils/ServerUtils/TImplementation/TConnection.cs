
using System.Net;
using System.Net.Sockets;
using BusterWood.Channels;

public class TConnection:ITConnection
{
    //当前连接的socket tcp套接字
    Socket conn;
    //连接的ID
    uint connID;
    //当前连接状态
    bool isClosed;
    //告知当前连接已经停止的Channel(由Reader告知Writer退出信号)
    bool exit;
    //Channel<bool> exitChan;
    //无缓冲管道，用于读写线程之间的通信
    Channel<byte[]> msgChan;
    //消息的管理MsgID和对应的处理业务API
    ITMsgHandler msgHandler;
    //拆封包模组
    ITDataPack dataPacker;

    public TConnection(Socket p_conn, uint p_connId, ITMsgHandler p_msgHandler,ITDataPack p_dataPacker)
	{
        conn = p_conn;
        connID = p_connId;
        isClosed = false;
        msgHandler = p_msgHandler;
        msgChan = new Channel<byte[]>();
        exit = false;
        dataPacker = p_dataPacker;
	}

    //连接业务读方法
    void startReader()
    {
        Console.WriteLine("Reader线程正在运行");
        for(; ; )
        {
            //包头固定8字节
            int HeadLength = (int)dataPacker.GetHeadLen();
            //创建拆包对象
            //ITDataPack dp = new TDataPacker();
            //读取包头8字节
            byte[] headBytes = new byte[HeadLength];
            ITMessage msg;
            try
            {
                while (HeadLength > 0)
                {
                    byte[] recvBytes1 = new byte[8];
                    //将本次传输已经接收到的字节数置0
                    int iBytesHead = 0;
                    //如果当前需要接收的字节数大于缓存区大小，则按缓存区大小进行接收，相反则按剩余需要接收的字节数进行接收
                    if (HeadLength >= recvBytes1.Length)
                    {
                        iBytesHead = conn.Receive(recvBytes1, recvBytes1.Length, 0);
                    }
                    else
                    {
                        iBytesHead = conn.Receive(recvBytes1, HeadLength, 0);
                    }
                    //将接收到的字节数保存
                    recvBytes1.CopyTo(headBytes, headBytes.Length - HeadLength);
                    //减去已经接收到的字节数
                    HeadLength -= iBytesHead;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取数据发生错误:" + ex.Message);
                break;
            }
            //拆包
            try
            {
                msg = dataPacker.Unpack(headBytes);
            }
            catch(Exception ex)
            {
                Console.WriteLine("拆包过程发生错误:" + ex.Message);
                break;
            }
            //根据包头数据再次读取，拆解出数据包内容
            int dataLen = (int)msg.GetMsgLen();
            //存储消息体的所有字节数
            byte[] data = new byte[dataLen];
            try
            {
                while (dataLen > 0)
                {
                    byte[] recvBytes2 = new byte[dataLen < 1024 ? dataLen : 1024];
                    //将本次传输已经接收到的字节数置0
                    int iBytesBody = 0;
                    //如果当前需要接收的字节数大于缓存区大小，则按缓存区大小进行接收，相反则按剩余需要接收的字节数进行接收
                    if (dataLen >= recvBytes2.Length)
                    {
                        iBytesBody = conn.Receive(recvBytes2, recvBytes2.Length, 0);
                    }
                    else
                    {
                        iBytesBody = conn.Receive(recvBytes2, dataLen, 0);
                    }

                    //将接收到的字节数保存
                    recvBytes2.CopyTo(data, data.Length - dataLen);
                    //减去已经接收到的字节数
                    dataLen -= iBytesBody;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取数据包过程发生错误:" + ex.Message);
                break;
            }
            msg.SetData(data);
            //建立Request
            ITRequest request = new TRequest(this, msg);
            //处理request
            Task.Run(new Action(() => msgHandler.DoMsgHandler(request)));
        }
        Stop();
        Console.WriteLine("[Reader 已退出!], connID="+ connID.ToString());
    }

    //启动写线程，专门用于给客户端发送消息
    void startWriter()
    {
        Console.WriteLine("Writer线程开始运行");
        //不断读channel的消息，并会写给客户端
        for (; ; )
        {
            //退出
            //bool exit =exitChan.Receive();
            if (exit)
            {
                break ;
            }
            //有数据则写至客户端
            try
            {
                byte[] data = msgChan.Receive();
                conn.Send(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("客户端消息发送错误：" + ex.Message);
                //也许可以不终止，但为了安全还是终止写线程
                break;
            }
        }
        Console.WriteLine ("[conn writer 已退出!], connID="+ connID.ToString());
    }

    //获得当前连接的ID
    public uint GetConnID()
    {
        return connID;
    }

    //获取当前连接所绑定的socket conn
    public Socket GetTCPConnection()
    {
        return conn;
    }

    //获取远程客户端的tcp状态
    public IPEndPoint? RemoteAddr()
    {
        return (IPEndPoint?)conn.RemoteEndPoint;
    }

    //回写消息至客户端
    public void SendMsg(uint msgId, byte[] data)
    {
        if (isClosed)
        {
            throw new Exception("客户端连接已关闭");
        }
        TDataPacker dp = new TDataPacker();
        //将消息进行封包
        byte[] binaryMsg=new byte[8];
        try
        {
            binaryMsg = dp.Pack(new TMessage(msgId, data));
        }
        catch
        {
            Console.WriteLine("封包发生错误，MsgId=" + msgId.ToString());
            throw;
        }
        //进行发送
        msgChan.SendAsync(binaryMsg);
    }

    //启动连接，让当前的连接准备开始工作
    public void Start()
    {
        Console.WriteLine("conn已启动：ConnId=" + connID.ToString());
        //启动从当前连接的读数据业务
        Task.Run(new Action(startReader));
        //启动从当前连接写数据的业务
        Task.Run(new Action(startWriter));
    }

    //停止连接，结束当前连接的工作
    public void Stop()
    {
        
        //如果当前连接已经关闭
        if (isClosed)
        {
            return;
        }
        Console.WriteLine("Conn 已停止.. ConID=" + connID.ToString());
        isClosed = true;
        //告知Writer关闭
        exit = true;
        //exitChan.SendAsync(true);

        //资源回收
        //exitChan.Close();
        msgChan.Close();
        //关闭socket连接
        conn.Close();
    }
}

