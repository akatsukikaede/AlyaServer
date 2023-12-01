
public class TMessage:ITMessage
{
    uint id;
    uint dataLen;
    byte[] data;

    //创建消息
	public TMessage(uint p_id, byte[] p_data)
	{
        id = p_id;
        dataLen = (uint)p_data.Length;
        data = p_data;
	}
    //获取消息内容
    public byte[] GetData()
    {
        return data ;
    }
    //获取消息ID
    public uint GetMsgId()
    {
        return id; ;
    }
    //获取消息长度
    public uint GetMsgLen()
    {
        return dataLen;
    }
    //设置消息内容
    public void SetData(byte[] p_data)
    {
        data= p_data;
    }
    //设置消息长度
    public void SetDataLen(uint len)
    {
        dataLen=len;
    }
    //设置消息ID
    public void SetMsgId(uint p_id)
    {
        id=p_id;
    }
}
