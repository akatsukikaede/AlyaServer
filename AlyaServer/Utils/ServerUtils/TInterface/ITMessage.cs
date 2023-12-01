public interface ITMessage
{
    //获取消息的ID
    uint GetMsgId();
    //获取消息的长度
    uint GetMsgLen();
    //获取消息的内容
    byte[] GetData();
    //设置消息的ID
    void SetMsgId(uint id);
    //设置消息内容
    void SetData(byte[] data);
    //设置消息长度
    void SetDataLen(uint len);
}
