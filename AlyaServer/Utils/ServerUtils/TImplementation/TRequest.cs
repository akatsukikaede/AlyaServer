using System;

public class TRequest:ITRequest
{
    ITConnection conn;
    ITMessage msg;

	public TRequest(ITConnection p_conn, ITMessage p_msg)
	{
        conn = p_conn;
        msg = p_msg;
	}

    //得到当前连接
    public ITConnection GetConnection()
    {
        return conn;
    }

    //得到当前请求的消息数据
    public byte[] GetData()
    {
        return msg.GetData();
    }

    //得到当前请求的消息ID
    public uint GetMsgID()
    {
        return msg.GetMsgId();
    }
}
