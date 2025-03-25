using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.ModbusDriver.ModbusServer
{
    /// <summary>
    /// 
    /// </summary>
    public class StructModbusTCP
    {
        public MemAreaModbusTCP Area = MemAreaModbusTCP.ReadCoil;//区域号
        public int m_unBeginWord = 0;//起始位置
        public ushort DBNumber = 0;//区块号
        public ushort m_unWordsCount = 0;//数据大小
        public ushort CacheIndex = 0;
        public byte Bit = 0;//位号
        public DataTypeTCP VarType;//数据类型
        public AreaReadModbusTCPTpye m_PLCArea = AreaReadModbusTCPTpye.ReadCoil;
        public bool m_bBitWrite = false; // Write bit
        public byte[] values;
        public byte[] m_strCmd;
        public byte id = 1;
        public EventWaitHandle _m_hEventFinish = new EventWaitHandle(false, EventResetMode.ManualReset);
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
            try
            {
                byte[] crc16;
                byte[] date;
                int len = 0;
                byte[] _length;
                switch (Area)//寄存器类型(区域号)
                {
                    case MemAreaModbusTCP.ReadCoil://读线圈状态00001-09999 位操作(单个或多个)****功能码01H
                        date = new byte[12];
                        date[0] = Convert.ToByte(id >> 8);	// Slave id high byte//事务元标识符,高字节在前,低字节在后  
                        date[1] = Convert.ToByte(id);// Slave id low byte
                        date[2] = 0x00;//协议标识符,高字节在前,低字节在后  (TCP 00)
                        date[3] = 0x00;
                        date[4] = 0x00;//后续字节长度,高字节在前,低字节在后
                        date[5] = 0x06;					// Message size
                        date[6] = 0x01;					// Slave address
                        date[7] = 0x01;//m_cmdword;读线圈寄存器功能码 01H			// Function code
                        date[8] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                        date[9] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                        date[10] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);			// Number of data to read//寄存器数量高字节
                        date[11] = Convert.ToByte(m_unWordsCount & 0xFF);			// Number of data to read//寄存器数量低字节
                        m_strCmd = date;
                        break;

                    case MemAreaModbusTCP.ReadDiscreteInputs://读离散输入状态10001-19999位操作(单个或多个)****功能码02H
                        date = new byte[12];
                        date[0] = Convert.ToByte(id >> 8);	// Slave id high byte//事务元标识符,高字节在前,低字节在后  
                        date[1] = Convert.ToByte(id);// Slave id low byte
                        date[2] = 0x00;//协议标识符,高字节在前,低字节在后  (TCP 00)
                        date[3] = 0x00;
                        date[4] = 0x00;//后续字节长度,高字节在前,低字节在后
                        date[5] = 0x06;					// Message size
                        date[6] = 0x01;					// Slave address
                        date[7] = 0x02;//功能码02H			// Function code
                        date[8] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                        date[9] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                        date[10] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);			// Number of data to read//寄存器数量高字节
                        date[11] = Convert.ToByte(m_unWordsCount & 0xFF);			// Number of data to read//寄存器数量低字节
                        m_strCmd = date;
                        break;

                    case MemAreaModbusTCP.ReadHoldingRegister://读保持寄存器40001-49999字操作(单个或多个)****功能码03H
                        date = new byte[12];
                        date[0] = Convert.ToByte(id >> 8);	// Slave id high byte//事务元标识符,高字节在前,低字节在后  
                        date[1] = Convert.ToByte(id);// Slave id low byte
                        date[2] = 0x00;//协议标识符,高字节在前,低字节在后  (TCP 00)
                        date[3] = 0x00;
                        date[4] = 0x00;//后续字节长度,高字节在前,低字节在后
                        date[5] = 0x06;					// Message size
                        date[6] = 0x01;					// Slave address
                        date[7] = 0x03;//功能码03H			// Function code
                        date[8] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                        date[9] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                        date[10] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);			// Number of data to read//寄存器数量高字节
                        date[11] = Convert.ToByte(m_unWordsCount & 0xFF);			// Number of data to read//寄存器数量低字节
                        m_strCmd = date;
                        break;

                    case MemAreaModbusTCP.ReadInputRegister:////读输入寄存器30001-39999字操作(单个或多个)****功能码04H
                        date = new byte[12];
                        date[0] = Convert.ToByte(id >> 8);	// Slave id high byte//事务元标识符,高字节在前,低字节在后  
                        date[1] = Convert.ToByte(id);// Slave id low byte
                        date[2] = 0x00;//协议标识符,高字节在前,低字节在后  (TCP 00)
                        date[3] = 0x00;
                        date[4] = 0x00;//后续字节长度,高字节在前,低字节在后
                        date[5] = 0x06;					// Message size
                        date[6] = 0x01;					// Slave address
                        date[7] = 0x04;//功能码04H			// Function code
                        date[8] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                        date[9] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                        date[10] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);			// Number of data to read//寄存器数量高字节
                        date[11] = Convert.ToByte(m_unWordsCount & 0xFF);			// Number of data to read//寄存器数量低字节
                        m_strCmd = date;
                        break;

                    case MemAreaModbusTCP.WriteSingleCoil://写单个线圈00001-09999位操作(单个)****功能码05H    
                        date = new byte[12];
                        date[0] = Convert.ToByte(id >> 8);//事务元标识符,高字节在前,低字节在后  
                        date[1] = Convert.ToByte(id);
                        date[2] = 0x00;//协议标识符,高字节在前,低字节在后  (TCP 00)
                        date[3] = 0x00;
                        date[4] = 0x00;//后续字节长度,高字节在前,低字节在后
                        date[5] = 0x06;// 
                        date[6] = 0x01;//Slave address   
                        date[7] = 0x05;//m_cmdword;//命令字功能码
                        date[8] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                        date[9] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                        if (m_bBitWrite)
                        {
                            date[10] = 0xFF;//数据1高字节=0xFF  数据2低字节=00 数据1高字节
                            date[11] = 0x00;//数据2低字节
                        }
                        else
                        {
                            date[10] = 0x00;
                            date[11] = 0x00;
                        }
                        m_strCmd = date;
                        break;

                    case MemAreaModbusTCP.WriteSingleRegister://写单个保持寄存器40001-49999字操作(单个)****功能码06H
                        date = new byte[12];
                        date[0] = Convert.ToByte(id >> 8);	// Slave id high byte//事务元标识符,高字节在前,低字节在后  
                        date[1] = Convert.ToByte(id);// Slave id low byte
                        date[2] = 0x00;//协议标识符,高字节在前,低字节在后  (TCP 00)
                        date[3] = 0x00;
                        date[4] = 0x00;//后续字节长度,高字节在前,低字节在后
                        date[5] = 0x06;					// Message size
                        date[6] = 0x01;					// Slave address
                        date[7] = 0x06;//功能码06H			// Function code
                        date[8] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                        date[9] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                        date[10] = values[1];		// 
                        date[11] = values[0];		// 
                        m_strCmd = date;
                        break;

                    case MemAreaModbusTCP.WriteMultipleCoils://写多个线圈00001-09999位操作(多个)****功能码0FH              
                        len = values.Length;
                        if (len % 2 > 0) len++;
                        date = new byte[len + 13];
                        date[0] = Convert.ToByte(id >> 8);//事务元标识符,高字节在前,低字节在后  
                        date[1] = Convert.ToByte(id);
                        date[2] = 0x00;   //协议标识符,高字节在前,低字节在后  (TCP 00)
                        date[3] = 0x00;
                        date[4] = 0x00; //后续字节长度,高字节在前,低字节在后
                        date[5] = Convert.ToByte(m_unWordsCount * 2 + 7);  //27个字节
                        date[6] = 0x01;//Slave address  
                        date[7] = 0x0F;//写多个保持寄存器 0FH
                        date[8] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                        date[9] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                        date[10] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);			// Number of data to read//寄存器数量高字节
                        date[11] = Convert.ToByte(m_unWordsCount & 0xFF);			// Number of data to read//寄存器数量低字节
                        date[12] = Convert.ToByte(m_unWordsCount * 2);//字节数
                        m_strCmd = date;
                        break;

                    case MemAreaModbusTCP.WriteMultipleRegister://写多个保持寄存器40001-49999字操作(多个)****功能码10H           
                        len = values.Length;
                        date = new byte[len + 13];
                        date[0] = Convert.ToByte(id >> 8);//事务元标识符,高字节在前,低字节在后  
                        date[1] = Convert.ToByte(id);
                        date[2] = 0x00;   //协议标识符,高字节在前,低字节在后  (TCP 00)
                        date[3] = 0x00;
                        date[4] = 0x00; //后续字节长度,高字节在前,低字节在后
                        date[5] = Convert.ToByte(len + 7);
                        date[6] = 0x01;//Slave address  
                        date[7] = 0x10;//写多个保持寄存器 10H
                        date[8] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                        date[9] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                        date[10] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);			// Number of data to read//寄存器数量高字节
                        date[11] = Convert.ToByte(m_unWordsCount & 0xFF);			// Number of data to read//寄存器数量低字节
                        date[12] = Convert.ToByte(len);//字节数	
                        Buffer.BlockCopy(values, 0, date, 13, len);
                        m_strCmd = date;
                        break;
                }
            }
            catch (Exception ex)
            {
                return ;
            }
        }

    }
}
