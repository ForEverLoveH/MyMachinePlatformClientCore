using System.Text;

namespace MyMachinePlatformClientCore.Service.OMRonService;

public enum MemAreaTCPFINS { CIO, HR, AR, DM, EM, WR }

 public class TaskStructTCPFINS
 {
        #region **** frame send command & response fields

        private Byte[] cmdFS = new Byte[16]
		{
			0x46, 0x49, 0x4E, 0x53,		// 'F' 'I' 'N' 'S' //Header[4]; 默认值
			0x00, 0x00, 0x00, 0x00,		// Expected number of bytes for response //长度为26字节(必须得计算)Length[4];必须得更改
			0x00, 0x00, 0x00, 0x02,		// Command FS  Sending=2 / Receiving=3 //Command[4];功能码
			0x00, 0x00, 0x00, 0x00		// Error code //Error[4];默认值（not use，全为0）
		};

        // FRAME SEND Response array
        //
        private Byte[] respFS = new Byte[16]
			{
			0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00
			};

        /// <summary>
        /// FINS COMMAND (2KB reserved memory)
        /// </summary>
        private Byte[] cmdFins = new Byte[]
		{
			//---- COMMAND HEADER -------------------------------------------------------
			0x80,				// 00 ICF Information control field //ICF
			0x00,				// 01 RSC Reserved
			0x02,				// 02 GTC Gateway count

			0x00,				// 03 DNA Destination network address (0=local network)
			0x00,				// 04 DA1 Destination node number  //必须得更改

			0x00,				// 05 DA2 Destination unit address //必须得更改
			0x00,				//06 SNA Source network address (0=local network) //必须得更改
			0x00,				// 07 SA1 Source node number //必须得更改

			0x00,				// 08 SA2 Source unit address //必须得更改
			0x00,				// 09 SID Service ID
            ////---- COMMAND --------------------------------------------------------------
            //0x00,				// 10 MC Main command
            //0x00,				// 11 SC Subcommand
            ////---- PARAMS ---------------------------------------------------------------
            //0x00,				// 12 reserved area for additional params
            //0x00,				// depending on fins command
            //0x00,
            //0x00,
            //0x00,
            //0x00,
            //0x00,
            //0x00,
            //0x00,
            //0x00,
		};

        /// <summary>
        /// DA1 Destination node number
        /// </summary>
        public Byte DA1
        {
            get { return this.cmdFins[4]; }
            set { this.cmdFins[4] = value; }
        }

        /// <summary>
        /// DA2 Destination unit address
        /// </summary>
        public Byte DA2
        {
            get { return this.cmdFins[5]; }
            set { this.cmdFins[5] = value; }
        }

        /// <summary>
        /// SNA Source network address (0=local network)
        /// </summary>
        public Byte SNA
        {
            get { return this.cmdFins[6]; }
            set { this.cmdFins[6] = value; }
        }

        /// <summary>
        /// SA1 Source node number
        /// </summary>
        public Byte SA1
        {
            get { return this.cmdFins[7]; }
            set { this.cmdFins[7] = value; }
        }

        /// <summary>
        /// SA2 Source unit address
        /// </summary>
        public Byte SA2
        {
            get { return this.cmdFins[8]; }
            set { this.cmdFins[8] = value; }
        }

        /// <summary>
        /// SID Service ID
        /// </summary>
        public Byte SID
        {
            get { return this.cmdFins[9]; }
            set { this.cmdFins[9] = value; }
        }

        /// <summary>
        /// MC Main command
        /// </summary>
        public Byte MC
        {
            get { return this.cmdFins[10]; }
            set { this.cmdFins[10] = value; }
        }

        /// <summary>
        /// SC Subcommand
        /// </summary>
        public Byte SC
        {
            get { return this.cmdFins[11]; }
            set { this.cmdFins[11] = value; }
        }

        /// <summary>
        /// FRAME SEND command length 发送长度为26字节，(必须得计算)Length[4];
        /// </summary>
        public UInt16 FS_LEN
        {
            get
            {
                UInt16 value = cmdFS[6];
                value <<= 8;
                value += Convert.ToUInt16(cmdFS[7]);
                return value;
            }
            set
            {
                this.cmdFS[6] = (Byte)((value >> 8) & 0xFF);
                this.cmdFS[7] = (Byte)(value & 0xFF);
            }
        }

        /// <summary>
        /// FRAME SEND response length接收长度
        /// </summary>
        public UInt16 FSR_LEN
        {
            get
            {
                UInt16 value = respFS[6];
                value <<= 8;
                value += Convert.ToUInt16(respFS[7]);
                return value;
            }
        }

        /// <summary>
        /// FRAME SEND response error
        /// </summary>
        public string FSR_ERR
        {
            get
            {
                return respFS[8].ToString()
                        + respFS[9].ToString()
                        + respFS[10].ToString()
                        + respFS[11].ToString();
            }
        }

        /// <summary>
        /// FRAME SEND response main code error
        /// </summary>
        public Byte FSR_MER
        {
            get { return respFins[12]; }
        }

        /// <summary>
        /// FRAME SEND response subcode error
        /// </summary>
        public Byte FSR_SER
        {
            get { return respFins[13]; }
        }

        // FINS RESPONSE (command)
        //
        private Byte[] respFins = new Byte[2048];

        #endregion **** frame send command & response fields

        public void GenStrCmd()
        {
            uint m_nCmdLen = 0;//指令长度
            uint m_nSendsize = 0;
            uint m_nLengthsdate = 0;
            byte[] strCmd = new byte[100];
            if (m_nReadOrWrite == 0)
            {
                m_nCmdLen = 8;//发送长度
                m_nLengthsdate = m_nCmdLen + 18;
                m_nSendsize = m_nCmdLen + 18 + 8;
                RecvDateLen = m_unWordsCount * 2 + 4 + 18 + 8;
                DataLen = (int)m_nCmdLen + 18 + 8;

                strCmd[0] = 0X01;
                strCmd[1] = 0X01;
                strCmd[2] = Omron_GetFinsCode(m_PLCArea);
                byte[] _adr = BitConverter.GetBytes((short)m_unBeginWord);
                strCmd[3] = _adr[1];				// Start m_unBeginWord寄存器地址高字节
                strCmd[4] = _adr[0];				// Start m_unBeginWord寄存器地址低字节
                strCmd[5] = 0X00;
                byte[] _count = BitConverter.GetBytes((short)m_unWordsCount);
                strCmd[6] = _count[1];
                strCmd[7] = _count[0];
            }
            else
            {
                //写数据
                // Write -- WordsCount is not used!
                if (m_nWordOrBit == 0)
                {
                    if (m_nWordOrDWord == 0)
                    {
                        m_nCmdLen = 10;
                        m_nLengthsdate = m_nCmdLen + 18;
                        m_nSendsize = m_nCmdLen + 26;
                        RecvDateLen = 4 + 26;
                        DataLen = (int)m_nCmdLen + 18 + 8;
                        strCmd[0] = 0X01;//this.MC = 0x01;
                        strCmd[1] = 0X02;//this.SC = 0x02;
                        strCmd[2] = Omron_GetFinsCode(m_PLCArea);//this.cmdFins[F_PARAM] = (Byte)area;
                        byte[] _adr = BitConverter.GetBytes((short)m_unBeginWord);
                        strCmd[3] = _adr[1];				// Start m_unBeginWord寄存器地址高字节this.cmdFins[F_PARAM + 1] = (Byte)((address >> 8) & 0xFF);
                        strCmd[4] = _adr[0];				// Start m_unBeginWord寄存器地址低字节this.cmdFins[F_PARAM + 2] = (Byte)(address & 0xFF);
                        strCmd[5] = 0X00;                   //this.cmdFins[F_PARAM + 3] = bit_position;
                        strCmd[6] = 0X00;                   //this.cmdFins[F_PARAM + 4] = (Byte)((count >> 8) & 0xFF);
                        strCmd[7] = 0X01;                   //this.cmdFins[F_PARAM + 5] = (Byte)(count & 0xFF);
                        byte[] _count = BitConverter.GetBytes((short)m_wWriteValue);//对应数据FrameSend(data);
                        strCmd[8] = _count[1];
                        strCmd[9] = _count[0];
                    }
                    if (m_nWordOrDWord == 1)
                    {
                        m_nCmdLen = 8 + 4;
                        m_nLengthsdate = m_nCmdLen + 18;
                        m_nSendsize = m_nCmdLen + 26;
                        RecvDateLen = 4 + 26;
                        DataLen = (int)m_nCmdLen + 18 + 8;
                        strCmd[0] = 0X01;
                        strCmd[1] = 0X02;
                        strCmd[2] = Omron_GetFinsCode(m_PLCArea);
                        byte[] _adr = BitConverter.GetBytes((short)m_unBeginWord);
                        strCmd[3] = _adr[1];				// Start m_unBeginWord寄存器地址高字节
                        strCmd[4] = _adr[0];
                        strCmd[5] = 0X00;
                        strCmd[6] = 0X00;
                        strCmd[7] = 0X02;
                        byte[] _count = BitConverter.GetBytes(m_dwWriteValue);
                        strCmd[8] = _count[1];
                        strCmd[9] = _count[0];
                        strCmd[10] = _count[3];
                        strCmd[11] = _count[2];
                    }
                    if (m_nWordOrDWord == 2)
                    {
                        m_nCmdLen = 8 + (uint)m_strData.Length % 2 + (uint)m_strData.Length;
                        m_nLengthsdate = m_nCmdLen + 18;
                        m_nSendsize = m_nCmdLen + 26;
                        RecvDateLen = 4 + 26;
                        DataLen = (int)m_nCmdLen + 18 + 8;
                        strCmd[0] = 0X01;
                        strCmd[1] = 0X02;
                        strCmd[2] = Omron_GetFinsCode(m_PLCArea);
                        byte[] _adr = BitConverter.GetBytes((short)m_unBeginWord);
                        strCmd[3] = _adr[1];				// Start m_unBeginWord寄存器地址高字节
                        strCmd[4] = _adr[0];
                        strCmd[5] = 0X00;
                        strCmd[6] = 0X00;

                        strCmd[7] = (byte)(m_strData.Length % 2 + m_strData.Length / 2);
                        byte[] _count = Encoding.ASCII.GetBytes(m_strData);
                        for (int i = 0; i < m_strData.Length; i++)
                        {
                            strCmd[8 + i] = _count[i];
                        }
                    }
                    if (m_nWordOrDWord == 3)
                    {
                        m_nCmdLen = 8 + (uint)m_short.Length * 2;
                        m_nLengthsdate = m_nCmdLen + 18;
                        m_nSendsize = m_nCmdLen + 26;
                        RecvDateLen = 4 + 26;
                        DataLen = (int)m_nCmdLen + 18 + 8;
                        strCmd[0] = 0X01;
                        strCmd[1] = 0X02;
                        strCmd[2] = Omron_GetFinsCode(m_PLCArea);
                        byte[] _adr = BitConverter.GetBytes((short)m_unBeginWord);
                        strCmd[3] = _adr[1];				// Start m_unBeginWord寄存器地址高字节
                        strCmd[4] = _adr[0];
                        strCmd[5] = 0X00;
                        strCmd[6] = 0X00;
                        byte[] _count = new byte[m_short.Length * 2];
                        strCmd[7] = (byte)m_short.Length;

                        for (int i = 0; i < m_short.Length; i++)
                        {
                            _count[2 * i] = (byte)(m_short[i] >> 8 & 0xFF);
                            _count[2 * i + 1] = (byte)(m_short[i] & 0xFF);
                        }
                        for (int i = 0; i < _count.Length; i++)
                        {
                            strCmd[8 + i] = _count[i];
                        }
                    }
                    if (m_nWordOrDWord == 4)
                    {
                        m_nCmdLen = 8 + (uint)m_uint.Length * 4;
                        m_nLengthsdate = m_nCmdLen + 18;
                        m_nSendsize = m_nCmdLen + 26;
                        RecvDateLen = 4 + 26;
                        DataLen = (int)m_nCmdLen + 18 + 8;
                        strCmd[0] = 0X01;
                        strCmd[1] = 0X02;
                        strCmd[2] = Omron_GetFinsCode(m_PLCArea);
                        byte[] _adr = BitConverter.GetBytes((short)m_unBeginWord);
                        strCmd[3] = _adr[1];				// Start m_unBeginWord寄存器地址高字节
                        strCmd[4] = _adr[0];
                        strCmd[5] = 0X00;
                        strCmd[6] = 0X00;
                        byte[] _count = new byte[m_uint.Length * 4];
                        strCmd[7] = (byte)(m_uint.Length * 2);

                        for (int i = 0; i < m_uint.Length; i++)
                        {
                            byte[] src = new byte[4];
                            src[3] = (byte)((m_uint[i] >> 24) & 0xFF);
                            src[2] = (byte)((m_uint[i] >> 16) & 0xFF);
                            src[1] = (byte)((m_uint[i] >> 8) & 0xFF);
                            src[0] = (byte)(m_uint[i] & 0xFF);

                            _count[4 * i + 1] = src[0];
                            _count[4 * i] = src[1];
                            _count[4 * i + 3] = src[3];
                            _count[4 * i + 2] = src[2];
                        }
                        for (int i = 0; i < _count.Length; i++)
                        {
                            strCmd[8 + i] = _count[i];
                        }
                    }
                }
                else
                {
                    m_nCmdLen = 9;
                    m_nLengthsdate = m_nCmdLen + 18;
                    m_nSendsize = m_nCmdLen + 26;
                    RecvDateLen = 4 + 26;
                    DataLen = (int)m_nCmdLen + 18 + 8;
                    //write bit
                    //                     strCmd[0] = 0X01;
                    //                     strCmd[1] = 0X02;
                    //                     strCmd[2] = Omron_GetFinsCode(m_PLCArea);
                    //                     byte[] _adr = BitConverter.GetBytes((short)m_unBeginWord);
                    //                     strCmd[3] = _adr[1];				// Start m_unBeginWord寄存器地址高字节
                    //                     strCmd[4] = _adr[0];
                    //                     byte[] _bit = BitConverter.GetBytes((short)m_unBit);
                    //                     strCmd[5] = 0X00;
                    //                     //strCmd[6] = _adr[0];
                    //                     strCmd[6] = _bit[0];
                    //                     strCmd[7] = 0X01;
                    //                     byte[] _count = BitConverter.GetBytes(m_wWriteValue);
                    //                     strCmd[8] = _count[0];

                    strCmd[0] = 0X01;
                    strCmd[1] = 0X02;
                    strCmd[2] = Omron_GetFinsCode(m_PLCArea);
                    byte[] _adr = BitConverter.GetBytes((short)m_unBeginWord);
                    strCmd[3] = _adr[1];				// Start m_unBeginWord寄存器地址高字节
                    strCmd[4] = _adr[0];
                    byte[] _bit = BitConverter.GetBytes((short)m_unBit);
                    strCmd[5] = _bit[0];
                    strCmd[6] = 0X00;
                    strCmd[7] = 0X01;
                    byte[] _count = BitConverter.GetBytes(m_wWriteValue);
                    strCmd[8] = _count[0];
                }
            }

            FS_LEN = (ushort)m_nLengthsdate;

            //标题命令参数长度
            int cmdFSLen = cmdFS.Length;
            //命令参数长度
            int cmdFinsLen = cmdFins.Length;
            //数据参数长度m_nCmdLen

            //参数传送变量

            byte[] data = new byte[cmdFS.Length + cmdFins.Length + m_nCmdLen];
            Array.Copy(cmdFS, data, cmdFS.Length);
            Array.Copy(cmdFins, 0, data, cmdFS.Length, cmdFins.Length);
            Array.Copy(strCmd, 0, data, cmdFS.Length + cmdFins.Length, m_nCmdLen);
            Array.Copy(data, m_strCmd, data.Length);
        }

        // 0 for read, and 1 for write
        public string m_strData
        {
            get;
            set;
        }

        public short[] m_short
        {
            get;
            set;
        }

        public uint[] m_uint
        {
            get;
            set;
        }

        // 0 for read, and 1 for write
        public int m_nReadOrWrite
        {
            get;
            set;
        }

        // Operate element: 0 for Word, and 1 for bit.
        public int m_nWordOrBit
        {
            get;
            set;
        }

        // Operate element: 0 for Word, and 1 for Dword,2 FOR string
        /// <summary>
        /// 0为word
        /// 1为Dword
        /// 2为string
        /// </summary>
        public int m_nWordOrDWord
        {
            get;
            set;
        }

        public MemAreaTCPFINS m_PLCArea
        {
            get;
            set;
        }

        public Int32 m_unBeginWord // Write or read address
        {
            get;
            set;
        }

        public UInt32 m_unBit // Write bit
        {
            get;
            set;
        }

        public UInt32 m_unWordsCount // Define read word counts
        {
            get;
            set;
        }

        public short m_wWriteValue//单字写入
        {
            get;
            set;
        }

        public Int32 m_dwWriteValue//双字写入
        {
            get;
            set;
        }

        public byte[] m_strCmd = new byte[1000];

        //public byte[] m_strCmd
        //{
        //    get;
        //    set;
        //}
        private Int32 _DataLen = 0;

        public Int32 DataLen
        {
            get { return _DataLen; }
            set { _DataLen = value; }
        }

        public UInt32 RecvDateLen
        {
            get;
            set;
        }

        private EventWaitHandle _m_hEventFinish = new EventWaitHandle(false, EventResetMode.ManualReset);

        public EventWaitHandle m_hEventFinish
        {
            get
            {
                return _m_hEventFinish;
            }
            set
            {
                _m_hEventFinish = value;
            }
        }

        private byte Omron_GetFinsCode(MemAreaTCPFINS dstStartAddr)
        {
            switch (dstStartAddr)
            {
                case MemAreaTCPFINS.CIO:
                    if (Convert.ToBoolean(m_nWordOrBit))//Operate element: 0 for Word, and 1 for bit.
                    {
                        return 0X30;
                    }
                    else
                    {
                        return 0XB0;
                    }
                case MemAreaTCPFINS.HR:
                    if (Convert.ToBoolean(m_nWordOrBit))//Operate element: 0 for Word, and 1 for bit.
                    {
                        return 0X30;
                    }
                    else
                    {
                        return 0XB0;
                    }
                case MemAreaTCPFINS.AR:
                    if (Convert.ToBoolean(m_nWordOrBit))//Operate element: 0 for Word, and 1 for bit.
                    {
                        return 0X33;
                    }
                    else
                    {
                        return 0XB3;
                    }
                case MemAreaTCPFINS.DM:
                    if (Convert.ToBoolean(m_nWordOrBit))//Operate element: 0 for Word, and 1 for bit.
                    {
                        return 0X02;
                    }
                    else
                    {
                        return 0X82;// 0x82;//读DM 区（功能码）
                    }
                case MemAreaTCPFINS.EM:
                    if (Convert.ToBoolean(m_nWordOrBit))//Operate element: 0 for Word, and 1 for bit.
                    {
                        return 0X20;
                    }
                    else
                    {
                        return 0XA0;
                    }
                case MemAreaTCPFINS.WR:
                    if (Convert.ToBoolean(m_nWordOrBit))//Operate element: 0 for Word, and 1 for bit.
                    {
                        return 0X31;
                    }
                    else
                    {
                        return 0XB1;
                    }
                default:
                    return 0X00;
            }
        }
 }