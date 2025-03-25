namespace MyMachinePlatformClientCore.Service.OMRonService;

public class OMRonEIPProtocol
{
    #region  EIP报文

    public byte[] RegisterCmd = new byte[28]
    {
        0x65, 0x00, //命令 2byte
        0x04, 0x00, //Header后面数据的长度 2byte
        0x00, 0x00, 0x00, 0x00, //会话句柄 4byte
        0x00, 0x00, 0x00, 0x00, //状态默认0 4byte
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //发送方描述默认0 8byte
        0x00, 0x00, 0x00, 0x00, //选项默认0 4byte

        //-------------------------------------------------------CommandSpecificData 指令指定数据 4byte

        0x01, 0x00, //协议版本 2byte

        0x00, 0x00, //选项标记 2byte
    };
    /*报文由三部分组成 Header 24个字节 、CommandSpecificData 16个字节、以及CIP消息（由读取的标签生成）
      实例，读取单个标签名为 TAG1的报文总长度为64个字节**************************************************/
    public byte[] Header = new byte[24]
    {
        0x6F,0x00,//命令 2byte
        0x28,0x00,//长度 2byte（总长度-Header的长度）=40 
        0x00,0x00,0x00,0x00,//会话句柄 4byte预留
        0x00,0x00,0x00,0x00,//状态默认0 4byte
        0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,//发送方描述默认0 8byte
        0x00,0x00,0x00,0x00,//选项默认0 4byte
    };
    public byte[] CommandSpecificData = new byte[16]
    {
        0x00,0x00,0x00,0x00,//接口句柄 CIP默认为0x00000000 4byte
        0x01,0x00,//超时默认0x0001 4byte
        0x02,0x00,//项数默认0x0002 4byte
        0x00,0x00,//空地址项默认0x0000 2byte
        0x00,0x00,//长度默认0x0000 2byte
        0xb2,0x00,//未连接数据项默认为 0x00b2
        0x18,0x00,//后面数据包的长度 24个字节(总长度-Header的长度-CommandSpecificData的长度)
    };
    public byte[] CipMessage = new byte[10] //标签名称为止所以长度不确定
    {
        0x52,0x02,　　   //服务默认0x52  请求路径大小 默认2
        0x20,06,0x24,0x01,//请求路径 默认0x01240620 4byte
        0x0A,0xF0,//超时默认0xF00A 4byte
        0x0A,0x00,//Cip指令长度  服务标识到服务命令指定数据的长度 
        //0x4C,//服务标识固定为0x4C 1byte  
        //0x03,// 节点长度 2byte  规律为 (标签名的长度+2)/2
        //0x91,//扩展符号 默认为 0x91
        //0x04,//标签名的长度
        //0x54,0x41,0x47,0x31,//标签名 ：TAG1转换成ASCII字节 当标签名的长度为奇数时，需要在末尾补0  比如TAG转换成ASCII为0x54,0x41,0x47，
        //需要在末尾补0 变成 0x54,0x41,0x47，0
         
    };
    public byte[] Common = new byte[6]
    {
        0x01,0x00,//服务命令指定数据　默认为0x0001　
        0x01,0x00,0x01,0x00//最后一位是PLC的槽号
    };
    /// <summary>
    /// 
    /// </summary>
    public byte[] WrCommon = new byte[4]
    {
        0x01,0x00,0x01,0x00//最后一位是PLC的槽号
    };



    #endregion
}