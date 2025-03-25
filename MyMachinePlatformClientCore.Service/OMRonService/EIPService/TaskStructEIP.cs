namespace MyMachinePlatformClientCore.Service.OMRonService;

public class TaskStructEIP
{
    private OMRonEIPFunction f= new OMRonEIPFunction();
    
      #region **** frame send command & response fields
        Byte[] cmdFS = new Byte[16]
        {
            0x46, 0x49, 0x4E, 0x53,		// 'F' 'I' 'N' 'S' //Header[4]; 默认值
			0x00, 0x00, 0x00, 0x00,		// Expected number of bytes for response //长度为26字节(必须得计算)Length[4];必须得更改
			0x00, 0x00, 0x00, 0x02,		// Command FS  Sending=2 / Receiving=3 //Command[4];功能码
			0x00, 0x00, 0x00, 0x00		// Error code //Error[4];默认值（not use，全为0）
		};

        // FRAME SEND Response array
        //
        Byte[] respFS = new Byte[16]
            {
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
            };
        /// <summary>
        /// FINS COMMAND (2KB reserved memory)
        /// </summary>
        Byte[] cmdFins = new Byte[]
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
        Byte[] respFins = new Byte[2048];
        #endregion
        public void GenStrCmd(short ReadCount = 1)
        {
            Registercmd = f.OmRonEipProtocol.RegisterCmd;//标签注册信息
            if (m_nReadOrWrite == 0)
            {
                m_strCmd = f.CreateReadCommand(m_strTag, ReadCount);
            }
            else
            {
                m_strCmd = f.CreatWirthCode(m_strTag, m_nDataType, m_wWriteObjValue);
            }
        }
         /// <summary>EIP显式通讯专用--判断当前通讯变量是否已注册，是，则无需重复注册，否，则进行注册
        /// 
        /// </summary>
        public bool IsRegistered = false;

        public byte[] RegisterMsg = new byte[4];

        public string m_strTag { get; set; }

        // 0 for read, and 1 for write
        public string m_strData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short[] m_short
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public uint[] m_uint
        {
            get;
            set;
        }

        /// <summary>0 for read, and 1 for write
        /// 
        /// </summary>
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

        /// <summary>组合数组的当前下标
        /// 
        /// </summary>
        public int m_ArrayIndex = 0;
        /// <summary>要读取的数据数组的总长度
        /// 
        /// </summary>
        public int m_ArrayCount = 0;
        /// <summary>读取长度
        /// 
        /// </summary>
        public int m_ReadCount = 0;

        /// <summary>是否组合数组并合包
        /// 
        /// </summary>
        public bool isComArr = false;


        /// <summary>数据类型
        /// 
        /// </summary>
        public OMRonEIPDataType m_nDataType
        {
            get;
            set;
        }
        public MemAreaEIP m_PLCArea
        {
            get;
            set;
        }
        public Int32 m_unBeginWord // Write or read address
        {
            get;
            set;
        }
        /// <summary>写bit第几位
        /// 
        /// </summary>
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

        /// <summary>写入的数据值
        /// 
        /// </summary>
        public object m_wWriteObjValue
        {
            get;
            set;
        }

        /// <summary>//标签注册信息
        /// 
        /// </summary>
        public byte[] Registercmd
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

}