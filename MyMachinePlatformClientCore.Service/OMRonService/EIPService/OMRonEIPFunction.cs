using System.Text;

namespace MyMachinePlatformClientCore.Service.OMRonService;
/// <summary>
/// 
/// </summary>
public class OMRonEIPFunction
{
    public OMRonEIPProtocol OmRonEipProtocol= new OMRonEIPProtocol();
    /// <summary>
    /// 字符转ascII
    /// </summary>
    /// <param name="server"></param>
    /// <param name="S1"></param>
    /// <returns></returns>
    public byte[] Str_To_ASCII(byte server, string S1)
    {
        byte[] buffer= new byte[1024];
        int offect = 0;
        string[] tagNames = S1.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        buffer[offect++] = server;
        offect++;
        for (int i = 0; i < tagNames.Length; i++)
        {
            buffer[offect++] = 0x91;                        // 固定
            buffer[offect++] = (byte)tagNames[i].Length;    // 节点的长度值
            byte[] nameBytes = Encoding.ASCII.GetBytes(tagNames[i]);
            nameBytes.CopyTo(buffer, offect);
            offect += nameBytes.Length;
            if (nameBytes.Length % 2 == 1) offect++;
        }

        buffer[1] = (byte)((offect - 2) / 2);
        byte[] data = new byte[offect];
        Array.Copy(buffer, 0, data, 0, offect);
        return data;
    }
    /// <summary>
    /// 生成读取命令
    /// </summary>
    /// <param name="message"></param>
    /// <param name="readLength"></param>
    /// <returns></returns>
    public byte[] CreateReadCommand(String message, short readLength=1)
    {
        byte[] tagdata = Str_To_ASCII(0x4c, message);
        byte[] RearCode = new byte[ OmRonEipProtocol.Header.Length +  OmRonEipProtocol.CommandSpecificData.Length
                                                                   +  OmRonEipProtocol.CipMessage.Length +  OmRonEipProtocol.Common.Length + tagdata.Length];
        Array.Copy(OmRonEipProtocol.Header, 0, RearCode, 0, OmRonEipProtocol.Header.Length);
        OmRonEipProtocol.CipMessage[8] = (byte)(tagdata.Length + 2);
        Array.Copy( OmRonEipProtocol.CommandSpecificData, 0, RearCode,  OmRonEipProtocol.Header.Length,  OmRonEipProtocol.CommandSpecificData.Length);

        Array.Copy( OmRonEipProtocol.CipMessage, 0, RearCode,  OmRonEipProtocol.Header.Length +  OmRonEipProtocol.CommandSpecificData.Length,  OmRonEipProtocol.CipMessage.Length);

        Array.Copy(tagdata, 0, RearCode,  OmRonEipProtocol.Header.Length +  OmRonEipProtocol.CommandSpecificData.Length +  OmRonEipProtocol.CipMessage.Length, tagdata.Length);

        Array.Copy( OmRonEipProtocol.Common, 0, RearCode,  OmRonEipProtocol.Header.Length +  OmRonEipProtocol.CommandSpecificData.Length +  OmRonEipProtocol.CipMessage.Length + tagdata.Length,  OmRonEipProtocol.Common.Length);

        #region 旧的长度赋值
        //RearCode[2] = Convert.ToByte(RearCode.Length - p.Header.Length);

        //RearCode[38] = Convert.ToByte(RearCode.Length - p.Header.Length - p.CommandSpecificData.Length);
        #endregion

        #region 新的长度赋值
        short mLength = Convert.ToInt16(RearCode.Length -  OmRonEipProtocol.Header.Length);
        byte[] mByte = BitConverter.GetBytes(mLength);
        RearCode[2] = mByte[0];
        RearCode[3] = mByte[1];

        short nLength = Convert.ToInt16(RearCode.Length -  OmRonEipProtocol.Header.Length -  OmRonEipProtocol.CommandSpecificData.Length);
        byte[] nByte = BitConverter.GetBytes(nLength);
        RearCode[38] = nByte[0];
        RearCode[39] = nByte[1];

        //Read Count
        byte[] rByte = BitConverter.GetBytes(readLength);
        int rIndex = RearCode.Length - 6;
        RearCode[rIndex] = rByte[0];
        RearCode[rIndex + 1] = rByte[1];
        #endregion

        //RearCode[51] = Convert.ToByte((TagCode.Length+2)/2);
        //string length = TagCode.Length.ToString("X2");
        //RearCode[53] = Convert.ToByte(TagCode.Length);
        return RearCode;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public string GetVelue(byte[] date)
    {

            string b = "";
            switch (date[44])
            {
                case 0xc1:
                    if (date[46] == 0x01)//BOOL
                    {
                        b = "true";
                    }
                    else
                    {
                        b = "false";
                    }; break;
                case 0xd0://读string字符串
                    string strData = "";
                    byte[] byteLen = new byte[2];
                    byteLen[0] = date[46];
                    byteLen[1] = date[47];
                    int strLen = BitConverter.ToInt16(byteLen, 0);
                    for (int i = 48; i < 48 + strLen; i++)
                    {
                        strData = strData + (char)date[i];
                    }
                    b = strData;
                    break;
                case 0xd1:
                    b = date[46].ToString();
                    break;//BYTE
                case 0xC3://读有符号单字INT等同于C#的short
                    byte[] dataI = new byte[2];
                    dataI[0] = date[46];
                    dataI[1] = date[47];

                    short Idata = BitConverter.ToInt16(dataI, 0);
                    b = Idata.ToString();
                    break;
                case 0xC4://读有符号双字DINT等同于C#的int32
                    byte[] dataDi = new byte[4];
                    dataDi[0] = date[46];
                    dataDi[1] = date[47];
                    dataDi[2] = date[48];
                    dataDi[3] = date[49];
                    int Didata = BitConverter.ToInt32(dataDi, 0);
                    b = Didata.ToString();
                    break;
                case 0xd2://读无符号单字Word等同于C#的Ushort
                    int c = date[47] * 256 + date[46];
                    b = c.ToString();
                    break;
                case 0xd3://读无符号双字DWord等同于C#的Uint
                    byte[] dataD = new byte[4];
                    dataD[0] = date[46];
                    dataD[1] = date[47];
                    dataD[2] = date[48];
                    dataD[3] = date[49];
                    uint Ddata = BitConverter.ToUInt32(dataD, 0);
                    b = Ddata.ToString();
                    break;
                case 0xca://读real实数
                    byte[] dataF = new byte[4];
                    dataF[0] = date[46];
                    dataF[1] = date[47];
                    dataF[2] = date[48];
                    dataF[3] = date[49];
                    float fdata = BitConverter.ToSingle(dataF, 0);
                    b = fdata.ToString();
                    break;


            }
            return b;

    }
    
    
       /// <summary>
       /// 生成写入命令
       ///  0为PLC word ==ushort
       /// 1为PLC Dword == uint
       /// 2为PLC int == short
       /// 3为PLC Dint = int
       /// 4为PLC real == float/double
       /// 5为PLC string == string
       /// 6为PLC BOOL == bool
       /// 7为PLC Byte == byte
       /// </summary>
       /// <param name="S1"></param>
       /// <param name="omRonEipDataType"></param>
       /// <param name="velue"></param>
       /// <returns></returns>
        public byte[] CreatWirthCode(string S1, OMRonEIPDataType omRonEipDataType, object velue)
        {
            /// dataType：0为PLC word ==ushort;1为PLC Dword == uint;2为PLC int == short;3为PLC Dint == int
            /// 4为PLC real == float/double;5为PLC string == string;6为PLC BOOL == bool;7为PLC Byte == byte

            byte[] TagCode = Str_To_ASCII(0X4D, S1);
            //byte[] TagCode = new byte[TagCodeA.Length];
            //Array.Copy(TagCodeA, 0, TagCode, 0, TagCode.Length);
            byte[] veluecode = new byte[6]
                                {
                                   0x00, 0x00,0x01,0X00,0x00,0x00//[2]变量数
                                };

            var p = OmRonEipProtocol;
            switch (omRonEipDataType)
            {
                case OMRonEIPDataType.Word://写word
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + 2);
                    veluecode[0] = 0xd2;
                    string strWord = velue.ToString();
                    ushort dataWord = Convert.ToUInt16(strWord);
                    byte[] dataWordByte = BitConverter.GetBytes(dataWord);

                    veluecode[4] = dataWordByte[0];
                    veluecode[5] = dataWordByte[1];
                    break;
                case OMRonEIPDataType.Dword://写Dword
                    veluecode = new byte[8]
                                {
                                   0x00, 0x00,0x01,0X00,0x00,0x00,0x00,0x00//[2]变量数
                                };
                    string strDword = velue.ToString();
                    veluecode[0] = 0xd3;
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + 4);
                    uint dataDword = Convert.ToUInt32(strDword);
                    byte[] dataDwordByte = BitConverter.GetBytes(dataDword);
                    veluecode[4] = dataDwordByte[0];
                    veluecode[5] = dataDwordByte[1];
                    veluecode[6] = dataDwordByte[2];
                    veluecode[7] = dataDwordByte[3];
                    break;
                case OMRonEIPDataType.Int://写Int
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + 2);
                    veluecode[0] = 0xc3;
                    string strInt = velue.ToString();
                    short dataInt = Convert.ToInt16(strInt);
                    byte[] dataIntByte = BitConverter.GetBytes(dataInt);

                    veluecode[4] = dataIntByte[0];
                    veluecode[5] = dataIntByte[1];
                    break;
                case OMRonEIPDataType.UInt://写UInt--2023 0327新增
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + 2);
                    veluecode[0] = 0xc7;
                    string strUInt = velue.ToString();
                    ushort dataUInt = Convert.ToUInt16(strUInt);
                    byte[] dataUIntByte = BitConverter.GetBytes(dataUInt);

                    veluecode[4] = dataUIntByte[0];
                    veluecode[5] = dataUIntByte[1];
                    break;
                case OMRonEIPDataType.DInt://写DInt
                    veluecode = new byte[8]
                                {
                                   0x00, 0x00,0x01,0X00,0x00,0x00,0x00,0x00//[2]变量数
                                };
                    veluecode[0] = 0xc4;
                    string strDInt = velue.ToString();
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + 4);
                    int dataDInt = Convert.ToInt32(strDInt);
                    byte[] dataDIntByte = BitConverter.GetBytes(dataDInt);
                    veluecode[4] = dataDIntByte[0];
                    veluecode[5] = dataDIntByte[1];
                    veluecode[6] = dataDIntByte[2];
                    veluecode[7] = dataDIntByte[3];
                    break;
                case OMRonEIPDataType.UDInt://写UDInt
                    veluecode = new byte[8]
                                {
                                   0x00, 0x00,0x01,0X00,0x00,0x00,0x00,0x00//[2]变量数
                                };
                    veluecode[0] = 0xc8;
                    string strUDInt = velue.ToString();
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + 4);
                    uint dataUDInt = Convert.ToUInt32(strUDInt);
                    byte[] dataUDIntByte = BitConverter.GetBytes(dataUDInt);
                    veluecode[4] = dataUDIntByte[0];
                    veluecode[5] = dataUDIntByte[1];
                    veluecode[6] = dataUDIntByte[2];
                    veluecode[7] = dataUDIntByte[3];
                    break;
                case OMRonEIPDataType.Real://写Real
                    veluecode = new byte[8]
                                {
                                   0x00, 0x00,0x01,0X00,0x00,0x00,0x00,0x00//[2]变量数
                                };
                    veluecode[0] = 0xca;
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + 4);
                    string strReal = velue.ToString();
                    float dataReal = Convert.ToSingle(strReal);
                    byte[] dataRealByte = BitConverter.GetBytes(dataReal);
                    veluecode[4] = dataRealByte[0];
                    veluecode[5] = dataRealByte[1];
                    veluecode[6] = dataRealByte[2];
                    veluecode[7] = dataRealByte[3];
                    break;
                case OMRonEIPDataType.String://写String，改版 可写中文
                    string strStringChinese = velue.ToString();
                    byte[] aa = Encoding.GetEncoding("UTF-8").GetBytes(strStringChinese);
                    int lenStringChinese = aa.Length;
                    if (lenStringChinese % 2 != 0)
                    {
                        lenStringChinese = lenStringChinese + 1 + 2;
                    }
                    else
                    {
                        lenStringChinese = lenStringChinese + 2;
                    }
                    veluecode = new byte[4 + lenStringChinese];
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + lenStringChinese);
                    veluecode[0] = 0xd0;//数据类型
                    veluecode[1] = 0x00;//数据类型
                    veluecode[2] = 0x01;//标签个数
                    veluecode[3] = 0x00;

                    int dataLenChinese = aa.Length;
                    if (dataLenChinese % 2 != 0)
                    {
                        dataLenChinese = dataLenChinese + 1;
                    }
                    byte[] byteLenChinese = BitConverter.GetBytes(dataLenChinese);
                    veluecode[4] = byteLenChinese[0];
                    veluecode[5] = byteLenChinese[1];

                    for (int i = 6; i < veluecode.Length; i++)
                    {
                        if (i == veluecode.Length - 2)
                        {
                            if (aa.Length % 2 != 0)
                            {
                                byte value2 = aa[(i - 6)];
                                veluecode[i] = value2;
                                veluecode[++i] = 0x00;
                            }
                            else
                            {
                                byte value = aa[(i - 6)];
                                veluecode[i] = value;
                            }
                        }
                        else
                        {
                            veluecode[i] = aa[(i - 6)];
                        }
                    }

                    break;
                case OMRonEIPDataType.Bool://写Bool
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + 2);
                    veluecode[0] = 0xc1;

                    string strBool = velue.ToString();
                    if (strBool == "1")
                    {
                        veluecode[4] = 0x01;
                    }
                    else
                    {
                        veluecode[4] = 0x00;
                    }
                    break;
                case OMRonEIPDataType.Byte://写Byte
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + 1);
                    string strByte = velue.ToString();
                    veluecode[0] = 0xd1;
                    veluecode[4] = Convert.ToByte(strByte, 16);
                    break;

                case OMRonEIPDataType.WordArr://写Word数组
                    ushort[] WordArr = (ushort[])velue;
                    int lenWordArr = WordArr.Length * 2;
                    veluecode = new byte[4 + lenWordArr];
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + lenWordArr);
                    veluecode[0] = 0xd2;//数据类型
                    veluecode[1] = 0x00;//数据类型
                    veluecode[2] = 0x01;//标签个数
                    veluecode[3] = 0x00;

                    for (int i = 4, j = 0; i < veluecode.Length; j++)
                    {
                        byte[] data = BitConverter.GetBytes(WordArr[j]);
                        veluecode[i] = data[0];
                        veluecode[i + 1] = data[1];
                        i = i + 2;
                    }
                    break;
                case OMRonEIPDataType.DwordArr:
                    uint[] DwordArr = (uint[])velue;
                    int lenDwordArr = DwordArr.Length * 2;
                    veluecode = new byte[4 + lenDwordArr];
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + lenDwordArr);
                    veluecode[0] = 0xd3;//数据类型
                    veluecode[1] = 0x00;//数据类型
                    veluecode[2] = 0x01;//标签个数
                    veluecode[3] = 0x00;

                    for (int i = 4, j = 0; i < veluecode.Length; j++)
                    {
                        byte[] data = BitConverter.GetBytes(DwordArr[j]);
                        veluecode[i] = data[0];
                        veluecode[i + 1] = data[1];
                        i = i + 2;
                    }
                    break;
                case OMRonEIPDataType.IntArr://写short 2023 04 26 修改，测试OK
                    short[] IntArr = (short[])velue;
                    int lenIntArr = IntArr.Length * 2;
                    veluecode = new byte[4 + lenIntArr];
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + lenIntArr);
                    veluecode[0] = 0xc3;//数据类型
                    veluecode[1] = 0x00;//数据类型
                    byte[] shortLength = BitConverter.GetBytes((short)IntArr.Length);
                    veluecode[2] = shortLength[0];//值个数
                    veluecode[3] = shortLength[1];//值个数

                    for (int i = 4, j = 0; i < veluecode.Length; j++)
                    {
                        byte[] data = BitConverter.GetBytes(IntArr[j]);
                        veluecode[i] = data[0];
                        veluecode[i + 1] = data[1];
                        i = i + 2;
                    }
                    break;
                //case DataType.DIntArr://2023 0328修改
                //    int[] DIntArr = (int[])velue;
                //    int lenDIntArr = DIntArr.Length * 4;
                //    veluecode = new byte[4 + lenDIntArr];
                //    p.CipMessage[8] = (byte)(TagCode.Length + 4 + lenDIntArr);
                //    veluecode[0] = 0xc4;//数据类型
                //    veluecode[1] = 0x00;//数据类型
                //    veluecode[2] = 0x01;//标签个数
                //    veluecode[3] = 0x00;

                //    for (int i = 4, j = 0; i < veluecode.Length; j++)
                //    {
                //        byte[] data = BitConverter.GetBytes(DIntArr[j]);
                //        veluecode[i] = data[0];
                //        veluecode[i + 1] = data[1];
                //        veluecode[i + 2] = data[2];
                //        veluecode[i + 3] = data[3];
                //        i = i + 4;
                //    }
                //    break;
                case OMRonEIPDataType.DIntArr://2023 0418修改,
                    int[] DIntArr = (int[])velue;
                    int lenDIntArr = DIntArr.Length * 4;
                    veluecode = new byte[4 + lenDIntArr];
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + lenDIntArr);
                    veluecode[0] = 0xc4;//数据类型
                    veluecode[1] = 0x00;//数据类型
                    byte[] IntLength = BitConverter.GetBytes((short)DIntArr.Length);
                    veluecode[2] = IntLength[0];//值个数
                    veluecode[3] = IntLength[1];//值个数

                    for (int i = 4, j = 0; i < veluecode.Length; j++)
                    {
                        byte[] data = BitConverter.GetBytes(DIntArr[j]);
                        veluecode[i] = data[0];
                        veluecode[i + 1] = data[1];
                        veluecode[i + 2] = data[2];
                        veluecode[i + 3] = data[3];
                        i = i + 4;
                    }
                    break;
                case OMRonEIPDataType.RealArr://2023 0328修改
                    float[] RealArr = (float[])velue;
                    int lenRealArr = RealArr.Length * 4;
                    veluecode = new byte[4 + lenRealArr];
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + lenRealArr);
                    veluecode[0] = 0xca;//数据类型
                    veluecode[1] = 0x00;//数据类型
                    byte[] FloatLength = BitConverter.GetBytes((short)RealArr.Length);
                    veluecode[2] = FloatLength[0];//标签个数
                    veluecode[3] = FloatLength[1];
                    //veluecode[2] = 0x01;//标签个数
                    //veluecode[3] = 0x00;

                    for (int i = 4, j = 0; i < veluecode.Length; j++)
                    {
                        byte[] data = BitConverter.GetBytes(RealArr[j]);
                        veluecode[i] = data[0];
                        veluecode[i + 1] = data[1];
                        veluecode[i + 2] = data[2];
                        veluecode[i + 3] = data[3];
                        i = i + 4;
                    }
                    break;
                case OMRonEIPDataType.StringArr:

                    break;
                case OMRonEIPDataType.BoolArr://修改
                    // p.CipMessage[8] = (byte)(TagCode.Length + 4 + 2);
                    //veluecode[0] = 0xc1;
                    //string strBool = velue.ToString();
                    //if (strBool == "1")
                    //{
                    //    veluecode[4] = 0x01;
                    //}
                    //else
                    //{
                    //    veluecode[4] = 0x00;
                    //}


                    bool[] BoolArr = (bool[])velue;
                    //int lenBoolArr = BoolArr.Length * 2;
                    int lenBoolArr = BoolArr.Length * 1;
                    veluecode = new byte[4 + lenBoolArr];
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + lenBoolArr);
                    veluecode[0] = 0xc1;//数据类型
                    veluecode[1] = 0x00;//数据类型
                    //veluecode[2] = 0x01;//标签个数
                    //veluecode[3] = 0x00;
                    byte[] boolArrLength = BitConverter.GetBytes((short)BoolArr.Length);
                    veluecode[2] = boolArrLength[0];//值个数
                    veluecode[3] = boolArrLength[1];//值个数

                    for (int i = 4, j = 0; i < veluecode.Length; j++)
                    {
                        byte[] data = BitConverter.GetBytes(BoolArr[j]);
                        //veluecode[i] = BoolArr[j].ToString() == "1" ? 0x01 : 0x00;
                        veluecode[i] = data[0];
                        //veluecode[i + 1] = 0;
                        //i = i + 2;
                        i = i + 1;
                    }
                    break;
                case OMRonEIPDataType.ByteArr:

                    byte[] ByteArr = (byte[])velue;
                    int lenByteArr = ByteArr.Length * 2;
                    veluecode = new byte[4 + lenByteArr];
                    p.CipMessage[8] = (byte)(TagCode.Length + 4 + lenByteArr);
                    veluecode[0] = 0xd1;//数据类型
                    veluecode[1] = 0x00;//数据类型
                    veluecode[2] = 0x01;//标签个数
                    veluecode[3] = 0x00;

                    for (int i = 4, j = 0; i < veluecode.Length; j++)
                    {
                        byte[] data = BitConverter.GetBytes((char)ByteArr[j]);
                        veluecode[i] = data[0];
                        veluecode[i + 1] = data[1];
                        i = i + 2;
                    }
                    break;
                case OMRonEIPDataType.Struct:

                    break;
                default:
                    break;
            }




            byte[] WriteCode = new byte[p.Header.Length + p.CommandSpecificData.Length
                           + p.CipMessage.Length + TagCode.Length + veluecode.Length + p.WrCommon.Length];
            //帧头24+ 接口句柄等16+ 请求信息（请求服务代码等）10+ 标签（服务标识、标签等）（4+count）+ 数据（数据类型、数据个数、数据等）（4+count）+PLC槽号4
            Array.Copy(p.Header, 0, WriteCode, 0, p.Header.Length);

            Array.Copy(p.CommandSpecificData, 0, WriteCode, p.Header.Length, p.CommandSpecificData.Length);

            Array.Copy(p.CipMessage, 0, WriteCode, p.Header.Length + p.CommandSpecificData.Length, p.CipMessage.Length);

            Array.Copy(TagCode, 0, WriteCode, p.Header.Length + p.CommandSpecificData.Length + p.CipMessage.Length, TagCode.Length);

            Array.Copy(veluecode, 0, WriteCode, p.Header.Length + p.CommandSpecificData.Length + p.CipMessage.Length + TagCode.Length, veluecode.Length);
            Array.Copy(p.WrCommon, 0, WriteCode, p.Header.Length + p.CommandSpecificData.Length + p.CipMessage.Length + TagCode.Length + veluecode.Length, p.WrCommon.Length);

            #region 旧的长度赋值
            //WriteCode[2] = Convert.ToByte(WriteCode.Length - p.Header.Length);
            //WriteCode[38] = Convert.ToByte(WriteCode.Length - p.Header.Length - p.CommandSpecificData.Length);
            #endregion
            #region 新的长度赋值
            short mLength = Convert.ToInt16(WriteCode.Length - p.Header.Length);
            byte[] mByte = BitConverter.GetBytes(mLength);
            WriteCode[2] = mByte[0];
            WriteCode[3] = mByte[1];

            short nLength = Convert.ToInt16(WriteCode.Length - p.Header.Length - p.CommandSpecificData.Length);
            byte[] nByte = BitConverter.GetBytes(nLength);
            WriteCode[38] = nByte[0];
            WriteCode[39] = nByte[1];
            #endregion
            //WriteCode[48] = Convert.ToByte((TagCode.Length + veluecode.Length +4));
            //if (Vtype == "写入byte")
            //{
            //    WriteCode[48] = Convert.ToByte((TagCode.Length + veluecode.Length + 4-1));
            //}
            //WriteCode[50] = 0x4d;//写功能
            //WriteCode[51] = Convert.ToByte((TagCode.Length + 2) / 2);
            //WriteCode[53] = Convert.ToByte(TagCode.Length);
            return WriteCode;


        }
        

}