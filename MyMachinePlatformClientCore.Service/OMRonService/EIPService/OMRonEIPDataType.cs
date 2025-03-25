namespace MyMachinePlatformClientCore.Service.OMRonService;

public enum OMRonEIPDataType
{
    /// <summary> PLC word ==ushort 无符号单字
    /// 
    /// </summary>
    Word = 0,
    /// <summary> PLC Dword == uint 无符号双字
    /// 
    /// </summary>
    Dword,
    /// <summary> PLC int == short 有符号单字
    /// 
    /// </summary>
    Int,
    /// <summary> PLC uint == ushort 无符号单字
    /// 
    /// </summary>
    UInt,
    /// <summary> PLC Dint = int 有符号双字
    /// 
    /// </summary>
    DInt,
    /// <summary> PLC UDInt = uint 无符号双字
    /// 
    /// </summary>
    UDInt,
    /// <summary> PLC real == float/double 浮点数
    /// 
    /// </summary>
    Real,
    /// <summary> PLC string == string 字符串
    /// 
    /// </summary>
    String,
    /// <summary> PLC BOOL == bool  布尔类型
    /// 
    /// </summary>
    Bool,
    /// <summary> PLC Byte == byte 字节
    /// 
    /// </summary>
    Byte,
    /// <summary> PLC Struct==结构体
    /// 
    /// </summary>
    Struct,
    /// <summary> PLC word ==ushort 无符号单字数组
    /// 
    /// </summary>
    WordArr,
    /// <summary> PLC Dword == uint 无符号双字数组
    /// 
    /// </summary>
    DwordArr,
    /// <summary> PLC int == short 有符号单字数组
    /// 
    /// </summary>
    IntArr,
    /// <summary> PLC Dint = int 有符号双字数组
    /// 
    /// </summary>
    DIntArr,
    /// <summary> PLC real == float/double 浮点数数组
    /// 
    /// </summary>
    RealArr,
    /// <summary> PLC string == string 字符串数组
    /// 
    /// </summary>
    StringArr,
    /// <summary> PLC BOOL == bool  布尔类型数组
    /// 
    /// </summary>
    BoolArr,
    /// <summary> PLC Byte == byte 字节数组
    /// 
    /// </summary>
    ByteArr,
}