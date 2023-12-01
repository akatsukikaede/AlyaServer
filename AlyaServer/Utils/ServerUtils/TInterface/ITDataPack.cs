
/// <summary>
/// 拆包封包模块
/// 直接面向TCP连接的数据流
/// 处理TCP粘包断包问题
/// </summary>
public interface ITDataPack
{
    //获取包头长度
    uint GetHeadLen();
    //封包方法
    byte[] Pack(ITMessage msg);
    //拆包方法
    ITMessage Unpack(byte[] dataPack);
}

