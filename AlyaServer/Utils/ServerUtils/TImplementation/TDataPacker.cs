public class TDataPacker : ITDataPack
{
    //包头长度固定8字节
    public uint GetHeadLen()
    {
        return 8;
    }

    //封包方法
    public byte[] Pack(ITMessage msg)
    {
        //创建一个存放byte[]的缓冲;
        byte[] dataBuffer = new byte[msg.GetMsgLen() + GetHeadLen()];
        Buffer.BlockCopy(BitConverter.GetBytes(msg.GetMsgLen()), 0, dataBuffer, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(msg.GetMsgId()), 0, dataBuffer, 4, 4);
        Buffer.BlockCopy(msg.GetData(), 0, dataBuffer, 8, (int)msg.GetMsgLen());
        return dataBuffer;
    }

    //拆包方法，该方法仅拆出Head，Data部分根据Head信息的dataLen再次读取即可
    public ITMessage Unpack(byte[] dataPack)
    {
        uint msgLen = BitConverter.ToUInt32(dataPack, 0);
        uint msgId = BitConverter.ToUInt32(dataPack, 4);
        byte[] data = new byte[msgLen];
        //需要判断大小是否超过限制
        return new TMessage(msgId, data);
    }
}

