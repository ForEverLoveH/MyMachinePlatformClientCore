using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.OMRonService 
{ 
    /// 
    /// </summary>
    public class OmronStructHostLink
    {
        string[] chAreaChar = new string[5] { "L", "R", "H", "D", "E" };
        string[] chAreaWordFINS = new string[5] { "B1", "B0", "B2", "82", "98" };
        string[] chAreaBitFINS = new string[5] { "31", "30", "32", "02", "20" };
        public int m_nReadOrWrite = 0;     // 0 for read, and 1 for write
        public int m_nWordOrBit = 0;    // Operate element: 0 for Word, and 1 for bit.
        public MemAreaHostLink m_PLCArea = MemAreaHostLink.WR;
        public uint m_unBeginWord = 0; // Write or read address       
        public UInt32 m_unBit = 0; // Write bit
        public UInt32 m_unWordsCount = 0; // Define read word counts      
        public Int32 m_dwWriteValue = 0;//双字写入
        public short m_wWriteValue = 0;
        public int m_nWordOrDWord = 0;// Operate element: 0 for Word, and 1 for bit.2   
        public byte[] m_strCmd = new byte[1000];
        private Int32 _DataLen = 0;
        public Int32 DataLen
        {
            get { return _DataLen; }
            set { _DataLen = value; }
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
        public void GenStrCmd()
        {
            string strCmd = null;
            if (0 == m_nReadOrWrite)
            {
                // Read -- Only read a Word!
                if (m_PLCArea == MemAreaHostLink.WR)
                {
                    // Cannot read WR area with HostLink
                    strCmd = string.Format("@00FA0000000000101{0}{1:X4}{2:X4}", chAreaWordFINS[(int)m_PLCArea], m_unBeginWord, m_unWordsCount);
                }
                else
                {
                    strCmd = string.Format("@00R{0}{1:D4}{2:D4}", chAreaChar[(int)m_PLCArea], m_unBeginWord, m_unWordsCount);
                }
            }
            else
            {
                // Write -- WordsCount is not used!
                if (m_nWordOrBit == 0)
                {
                    // Operate word:
                    if (m_nWordOrDWord == 0)
                    {
                        strCmd = string.Format("@00FA0000000000102{0}{1:X4}000001{2:X2}{3:X2}", chAreaWordFINS[(int)m_PLCArea], m_unBeginWord, m_wWriteValue / 256, m_wWriteValue % 256);
                    }
                    else
                    {
                        int a, b;
                        a = m_dwWriteValue / (256 * 256);//高字节
                        b = m_dwWriteValue % (256 * 256);//低字节				
                        strCmd = string.Format("@00FA0000000000102{0}{1:X4}000002{2:X2}{3:X2}{4:X2}{5:X2}", chAreaWordFINS[(int)m_PLCArea], m_unBeginWord, b / 256, b % 256, a / 256, a % 256);
                    }
                }
                else
                {
                    // Operate bit:              
                    strCmd = string.Format("@00FA0000000000102{0}{1:X4}{2:X2}0001{3:X2}", chAreaWordFINS[(int)m_PLCArea], m_unBeginWord, m_unBit, m_wWriteValue);
                }
            }
            string strCmd1;
            string strCmd2;
            strCmd1 = string.Format("{0:X2}*\r", FCS(Encoding.Default.GetBytes(strCmd)));
            strCmd2 = strCmd + strCmd1;
            m_strCmd = Encoding.Default.GetBytes(strCmd2);
            _DataLen = m_strCmd.Length;
        }
        public byte FCS(byte[] strFCS)
        {
            int nLength = strFCS.Length;
            byte chResult = strFCS[0];
            int i;
            for (i = 1; i < nLength; i++)
            {
                chResult ^= strFCS[i];
            }
            return chResult;
        }
    }

}
