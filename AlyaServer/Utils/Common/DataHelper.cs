using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class DataHelper 
{
    public static byte[] CRC16(byte[] data)
    {
        //crc计算赋初始值
        int crc = 0xffff;
        for (int i = 0; i < data.Length; i++)
        {
            crc = crc ^ data[i];
            for (int j = 0; j < 8; j++)
            {
                int temp;
                temp = crc & 1;
                crc = crc >> 1;
                crc = crc & 0x7fff;
                if (temp == 1)
                {
                    crc = crc ^ 0xa001;
                }
                crc = crc & 0xffff;
            }
        }
        //CRC寄存器的高低位进行互换
        byte[] crc16 = new byte[2];
        //CRC寄存器的高8位变成低8位，
        crc16[1] = (byte)((crc >> 8) & 0xff);
        //CRC寄存器的低8位变成高8位
        crc16[0] = (byte)(crc & 0xff);
        return crc16;
    }

    public static byte[] StructToBytes<T>(T obj)
    {
        int size = Marshal.SizeOf<T>();
        byte[] bytes = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(obj, ptr, false);
        Marshal.Copy(ptr, bytes, 0, size);
        Marshal.FreeHGlobal(ptr);

        return bytes;
    }

    public static T BytesToStruct<T>(byte[] bytes)
    {
        int size = Marshal.SizeOf<T>();
        if (bytes.Length < size)
        {
            return default(T);
        }

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(bytes, 0, ptr, size);
        T obj = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);

        return obj;
    }
}
