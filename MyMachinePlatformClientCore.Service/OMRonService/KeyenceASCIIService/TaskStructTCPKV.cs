using System.Text;

namespace MyMachinePlatformClientCore.Service.OMRonService;

 public class TaskStructTCPKV
 {

        private const int BlockSize = 0x0010;

        private byte NetwrokNumber { get; set; }        // 网络编号
        private byte PcNumber { get; set; }             // PC编号/PLC编号
        private uint IoNumber { get; set; }             // 请求目标模块IO编号
        private byte ChannelNumber { get; set; }        // 请求目标模块站编号
        private uint CpuTimer { get; set; }             // CPU监视定时器

        public byte[] onOffBits { get; set; }    // 发送位数据
        public byte[] iData { get; set; }    // 发送字数据
        /// <summary>// 发送字数据
        /// 
        /// </summary>
        public object[] m_dData { get; set; }

        public Dictionary<int, byte> iAddressAndOnOffMap { get; set; } // 发送随机位数据
        public int RcvDataLength { get; set; }//接收数据的个数
        public uint MainCommand { get; set; }           //主命令
        public uint SubCommand { get; set; }            //子命令 值是0表示按字读取(1个字=16位),如果值是1就按位读取；

        /// <summary>//数据读取的起始地址
        /// 
        /// </summary>
        public uint m_unBeginWord { get; set; }

        /// <summary>// Define read word counts //数据个数
        /// 
        /// </summary>
        public uint m_unWordsCount { get; set; }

        /// <summary>0 for read, and 1 for write,选择读或写
        /// 
        /// </summary>
        public int m_nReadOrWrite
        {
            get;
            set;
        }
        public int mark
        {
            get;
            set;
        }


        /// <summary>// Operate element: 0 for Word, and 1 for Dword,2 FOR string，选择写的数据类型
        /// 0为word
        /// 1为Dword
        /// 2为string
        /// 3为写float
        /// 4为写double
        /// </summary>
        public int m_nWriteDataType
        {
            get;
            set;
        }


        /// <summary>用于写数据选择写位或写字
        /// write Operate element: 0 for Word, and 1 for bit.
        /// </summary>
        public int m_nWordOrBit
        {
            get;
            set;
        }

        /// <summary>地址区域
        /// 
        /// </summary>
        public MemAreaTCPKV m_PLCArea
        {
            get;
            set;
        }



        public UInt32 m_unBit // Write bit
        {
            get;
            set;
        }

        public byte[] m_strCmd;         //发送数据字节
        public TaskStructTCPKV()
        {

        }
        public void GenStrCmd()
        {
            StringBuilder data = new StringBuilder();
            if (m_nReadOrWrite == 0)
            {
                RcvDataLength = (int)m_unWordsCount * 7 + 1;
                data.AppendFormat(string.Format("{0:D4} ", "RDS"));//固定的指令
                string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需读取的地址区域
                data.AppendFormat("{0}{1}.S ", addArea, m_unBeginWord);//读取时均按有符号字读取：.S表示有符号字
                data.AppendFormat("{0}", m_unWordsCount);

                byte[] strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString());
                m_strCmd = new byte[strCmd.Length + 2];
                Array.Copy(strCmd, m_strCmd, strCmd.Length);
                m_strCmd[strCmd.Length] = 0x0d;
                m_strCmd[strCmd.Length + 1] = 0x0a;

            }
            else if (m_nReadOrWrite == 26)
            {
                RcvDataLength = (int)m_unWordsCount * 6 + 1;
                data.AppendFormat(string.Format("{0:D4} ", "RDS"));//固定的指令
                string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需读取的地址区域
                data.AppendFormat("{0}{1}.U ", addArea, m_unBeginWord);//读取时按无符号字读取：U表示无符号字.S表示有符号字
                data.AppendFormat("{0}", m_unWordsCount);

                byte[] strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString());
                m_strCmd = new byte[strCmd.Length + 2];
                Array.Copy(strCmd, m_strCmd, strCmd.Length);
                m_strCmd[strCmd.Length] = 0x0d;
                m_strCmd[strCmd.Length + 1] = 0x0a;

            }
            else
            {
                RcvDataLength = 4;
                //写数据
                // Write -- WordsCount is not used!
                if (m_nWordOrBit == 0)
                {
                    if (m_nWriteDataType == 0)//写单字
                    {
                        data.AppendFormat(string.Format("{0:D4} ", "WRS"));//固定的指令
                        string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需写值的地址区域
                        data.AppendFormat("{0}{1}.S ", addArea, m_unBeginWord);//写入时均按有符号字写入：.S表示有符号字
                        data.AppendFormat("{0} ", m_unWordsCount);

                        foreach (short t in m_dData)
                        {
                            data.AppendFormat("{0} ", t);

                        }
                        data = new StringBuilder(Convert.ToString(data).Trim());

                        byte[] strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString().TrimEnd());
                        m_strCmd = new byte[strCmd.Length + 2];
                        Array.Copy(strCmd, m_strCmd, strCmd.Length);
                        m_strCmd[strCmd.Length] = 0x0d;
                        m_strCmd[strCmd.Length + 1] = 0x0a;
                    }
                    if (m_nWriteDataType == 1)//写双字
                    {
                        data.AppendFormat(string.Format("{0:D4} ", "WRS"));//固定的指令
                        string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需写值的地址区域 
                        data.AppendFormat("{0}{1}.L ", addArea, m_unBeginWord);//写入时均按有符号字写入：.L表示有符号字
                        data.AppendFormat("{0} ", m_unWordsCount);

                        foreach (var t in m_dData)
                        {
                            data.AppendFormat("{0} ", t);
                        }

                        byte[] strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString().TrimEnd());
                        m_strCmd = new byte[strCmd.Length + 2];
                        Array.Copy(strCmd, m_strCmd, strCmd.Length);
                        m_strCmd[strCmd.Length] = 0x0d;
                        m_strCmd[strCmd.Length + 1] = 0x0a;
                    }
                    if (m_nWriteDataType == 2)//写字符串
                    {
                        data.AppendFormat(string.Format("{0:D4} ", "WRS"));//固定的指令
                        string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需写值的地址区域
                        data.AppendFormat("{0}{1}.H ", addArea, m_unBeginWord);////写入时均按有十六进制写入：.H表示16进制
                        data.AppendFormat("{0} ", m_dData.Length);

                        foreach (var t in m_dData)
                        {
                            data.AppendFormat("{0} ", t);
                        }

                        byte[] strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString().TrimEnd());
                        m_strCmd = new byte[strCmd.Length + 2];
                        Array.Copy(strCmd, m_strCmd, strCmd.Length);
                        m_strCmd[strCmd.Length] = 0x0d;
                        m_strCmd[strCmd.Length + 1] = 0x0a;
                    }
                    if (m_nWriteDataType == 3 || m_nWriteDataType == 4)//=3写float =4写Double
                    {
                        data.AppendFormat(string.Format("{0:D4} ", "WRS"));//固定的指令
                        string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需写值的地址区域
                        data.AppendFormat("{0}{1}.H ", addArea, m_unBeginWord);//写入时均按有十六进制写入：.H表示16进制
                        data.AppendFormat("{0} ", m_dData.Length);

                        foreach (var t in m_dData)
                        {
                            data.AppendFormat("{0} ", t);
                        }
                        byte[] strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString().TrimEnd());
                        m_strCmd = new byte[strCmd.Length + 2];
                        Array.Copy(strCmd, m_strCmd, strCmd.Length);
                        m_strCmd[strCmd.Length] = 0x0d;
                        m_strCmd[strCmd.Length + 1] = 0x0a;
                    }
                    if (m_nWriteDataType == 5)// =5 写short 数组
                    {
                        data.AppendFormat(string.Format("{0:D4} ", "WRS"));//固定的指令
                        string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需写值的地址区域
                        data.AppendFormat("{0}{1}.S ", addArea, m_unBeginWord);//写入时均按有符号字写入：.S表示有符号字
                        data.AppendFormat("{0} ", m_dData.Length);

                        foreach (var t in m_dData)
                        {
                            data.AppendFormat("{0} ", t);
                        }
                        byte[] strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString().TrimEnd());
                        m_strCmd = new byte[strCmd.Length + 2];
                        Array.Copy(strCmd, m_strCmd, strCmd.Length);
                        m_strCmd[strCmd.Length] = 0x0d;
                        m_strCmd[strCmd.Length + 1] = 0x0a;
                    }
                    //if (m_nWriteDataType == 4)//写Double
                    //{
                    //    data.AppendFormat(string.Format("{0:D4} ", "WRS"));//固定的指令
                    //    string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需写值的地址区域
                    //    data.AppendFormat("{0}{1}.H ", addArea, m_unBeginWord);//写入时均按有十六进制写入：.H表示16进制
                    //    data.AppendFormat("{0} ", m_dData.Length);

                    //    foreach (string t in m_dData)
                    //    {
                    //        data.AppendFormat("{0} ", t);
                    //    }
                    //    byte[] strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString().TrimEnd());
                    //    m_strCmd = new byte[strCmd.Length + 2];
                    //    Array.Copy(strCmd, m_strCmd, strCmd.Length);
                    //    m_strCmd[strCmd.Length] = 0x0d;
                    //    m_strCmd[strCmd.Length + 1] = 0x0a;
                    //}
                }
                else//!=0写位
                {
                    if ((int)m_PLCArea >= (int)MemAreaTCPKV.DM && (int)m_PLCArea <= (int)MemAreaTCPKV.CM)//此范围内的地址区不支持写位：DM、EM、FM、ZF、W、TM、Z、CS、CM
                    {
                        data.AppendFormat(string.Format("{0:D4}", "RDS"));//固定的指令
                        string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需读取的地址区域
                        data.AppendFormat("{0}{1}.U ", addArea, m_unBeginWord);//读取时均按无符号字读取：.U表示无符号字
                        data.AppendFormat("{0}", m_unWordsCount);
                        m_strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString());
                    }
                    else//此范围内的地址区支持写位：R、B、MR、LR、CR、VB、VM
                    {
                        data.AppendFormat(string.Format("{0:D4}", "WRS"));//固定的指令
                        string addArea = Enum.GetName(typeof(MemAreaTCPKV), m_PLCArea);//获取需写值的地址区域
                        data.AppendFormat("{0}{1} ", addArea, m_unBeginWord);//写入时均按有符号字写入：.S表示有符号字
                        data.AppendFormat("{0}", m_unWordsCount);

                        foreach (bool t in m_dData)
                        {
                            short bit = Convert.ToInt16(t);
                            data.AppendFormat("{0} ", bit.ToString());
                        }
                        m_strCmd = ASCIIEncoding.ASCII.GetBytes(data.ToString().TrimEnd());
                    }

                }
            }
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