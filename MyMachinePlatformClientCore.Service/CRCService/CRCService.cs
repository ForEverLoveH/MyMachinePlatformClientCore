namespace MyMachinePlatformClientCore.Service;

public class CRCService
{
    private static readonly ushort[] Table = new ushort[256];

    static CRCService()
    {
        const ushort polynomial = 0x1021;
        for (ushort i = 0; i < Table.Length; ++i)
        {
            ushort value = 0;
            var temp = i;
            for (var j = 0; j < 8; ++j)
            {
                if (((value ^ temp) & 0x0001) != 0)
                {
                    value = (ushort)((value >> 1) ^ polynomial);
                }
                else
                {
                    value >>= 1;
                }
                temp >>= 1;
            }
            Table[i] = value;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static ushort ComputeChecksum( byte[] bytes)
    {
        var crc = 0;
        foreach (var t in bytes)
        {
            var index = (byte)(crc ^ t);
            crc = (crc >> 8) ^ Table[index];
        }
        return (ushort)crc;
    }
    /// <summary>
    /// 
    /// </summary>
    private static ushort lastWaterByte =0;
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static byte[] CreateWaterByte()
    {
        Random random = new Random();
        ushort waterCode = 0;
        do
        {
            waterCode = (ushort)random.Next(ushort.MaxValue + 1); // 生成随机数
        }
        while(waterCode==lastWaterByte);
        byte[] serialNumberBytes = GenerateSerialNumber(waterCode);
        lastWaterByte = waterCode; // 更新上一次的流水码
        return serialNumberBytes;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serialNumber"></param>
    /// <returns></returns>
    static byte[] GenerateSerialNumber(ushort serialNumber)
    {
        // 将流水码转换为2个字节的数组
        byte[] bytes = BitConverter.GetBytes(serialNumber);
        if (BitConverter.IsLittleEndian)
        {
            // 如果是小端字节序，需要反转字节顺序
            Array.Reverse(bytes);
        }
        return bytes;
    }
}