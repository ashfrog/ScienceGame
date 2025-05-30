using System;
using System.Linq;

public class CRC
{
    /// <summary>
    /// 判断数据中crc是否正确
    /// </summary>
    /// <param name="datas">传入的数据后两位是crc</param>
    /// <returns></returns>
    public static bool IsCrcOK(byte[] datas)
    {
        if (datas == null || datas.Length < 2)
        {
            return false;
        }

        int length = datas.Length - 2;

        byte[] bytes = new byte[length];
        Array.Copy(datas, 0, bytes, 0, length);
        byte[] getCrc = GetModbusCrc16(bytes);

        if (getCrc[0] == datas[length] && getCrc[1] == datas[length + 1])
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 传入数据添加两位crc
    /// </summary>
    /// <param name="datas"></param>
    /// <returns></returns>
    public static byte[] GetCRCDatas(byte[] datas)
    {
        int length = datas.Length;
        byte[] crc16 = GetModbusCrc16(datas);
        byte[] crcDatas = new byte[length + 2];
        Array.Copy(datas, crcDatas, length);
        Array.Copy(crc16, 0, crcDatas, length, 2);
        return crcDatas;
    }

    /// <summary>
    /// modbuscrc16
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] GetModbusCrc16(byte[] bytes)
    {
        byte crcRegister_H = 0xFF, crcRegister_L = 0xFF;// 预置一个值为 0xFFFF 的 16 位寄存器

        byte polynomialCode_H = 0xA0, polynomialCode_L = 0x01;// 多项式码 0xA001

        for (int i = 0; i < bytes.Length; i++)
        {
            crcRegister_L = (byte)(crcRegister_L ^ bytes[i]);

            for (int j = 0; j < 8; j++)
            {
                byte tempCRC_H = crcRegister_H;
                byte tempCRC_L = crcRegister_L;

                crcRegister_H = (byte)(crcRegister_H >> 1);
                crcRegister_L = (byte)(crcRegister_L >> 1);
                // 高位右移前最后 1 位应该是低位右移后的第 1 位：如果高位最后一位为 1 则低位右移后前面补 1
                if ((tempCRC_H & 0x01) == 0x01)
                {
                    crcRegister_L = (byte)(crcRegister_L | 0x80);
                }

                if ((tempCRC_L & 0x01) == 0x01)
                {
                    crcRegister_H = (byte)(crcRegister_H ^ polynomialCode_H);
                    crcRegister_L = (byte)(crcRegister_L ^ polynomialCode_L);
                }
            }
        }
        return new byte[] { crcRegister_L, crcRegister_H };
    }


    // 16进制字符串转byte数组
    public static byte[] HexStringToByteArray(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return null;

        hex = hex.Replace(" ", ""); // 移除可能存在的空格
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    // byte数组转16进制字符串
    public static string ByteArrayToHexString(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        return BitConverter.ToString(bytes).Replace("-", "");
    }

}