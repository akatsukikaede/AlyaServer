using System;
public class RoomInspectManager
{
    //单例模式调用
    private static RoomInspectManager instance = new RoomInspectManager();
    /// <summary>
    /// 连接管理组件对外单例调用
    /// </summary>
    public static RoomInspectManager Instance
    {
        get
        {
            return instance;
        }
    }

    //以cid为Key获取Inspect
    public Dictionary<uint, IRInspect> RoomInspectTable=new Dictionary<uint, IRInspect>();

    //添加监听线路，与ConnectionManager的ConnTable保持同步,连接添加完成之后就添加监听
    public void AddInspect(uint p_cid, IRInspect p_rInspect)
    {
        RoomInspectTable.Add(p_cid, p_rInspect);
        Console.WriteLine("CID=" + p_cid.ToString() + "监听添加成功");
        //添加之后启动监听
        p_rInspect.Start();
    }

    //移除监听，与ConnnectionManager的ConnTable保持同步，有连接掉线就移除监听
    public void RemoveInspect(uint p_cid)
    {
        //先停止监听
        RoomInspectTable[p_cid].Stop();
        //移除列表
        RoomInspectTable.Remove(p_cid);
        Console.WriteLine("CID=" + p_cid.ToString() + "监听移除成功");
    }
}
