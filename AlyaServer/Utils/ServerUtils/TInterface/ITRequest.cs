using System;

/// <summary>
/// Request接口
/// 将客户端请求的连接信息和请求的数据装到Request中
/// </summary>
public interface ITRequest
{
    //得到当前连接
    ITConnection GetConnection();
    //得到请求的消息数据
    byte[] GetData();
    //获得当前消息id
    uint GetMsgID();
}

