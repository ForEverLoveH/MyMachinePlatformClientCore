namespace MyMachinePlatformClientCore.Service.ModbusDriver.ModbusRTU;

 public class StructModbusRTUD
 {             
        public MemAreaModbusRTUD Area = MemAreaModbusRTUD.ReadCoil;//区域号
        public int m_unBeginWord = 0 ;//起始位置
        public ushort DBNumber = 0;//区块号
        public ushort m_unWordsCount = 0;//数据大小
        public ushort CacheIndex = 0;
        public byte Bit = 0;//位号
        public DataType VarType;//数据类型
        public AreaReadModbusRTUDTpye m_PLCArea = AreaReadModbusRTUDTpye.ReadCoil;
        public bool m_bBitWrite = false; // Write bit
        public byte[] values;
        public byte[] m_strCmd;

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
            byte[] crc16;
            byte[] date;             
            int len = 0;
            byte[] _length;
            switch (Area)//寄存器类型(区域号)
            {
                case MemAreaModbusRTUD.ReadCoil://读线圈状态00001-09999 位操作(单个或多个)****功能码01H
                    date = new byte[8];
                    date[0] = 0x01;//从站地址
                    date[1] = 0x01;//功能码
                    date[2] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                    date[3] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                    date[4] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);// Number of data to read寄存器数量高字节
                    date[5] = Convert.ToByte(m_unWordsCount& 0xFF);// Number of data to read寄存器数量低字节
                    crc16 = CRCStuff.calculateCRC(ref date, 6);
                    date[6] = crc16[0];// Number of data to read寄存器数量高字节
                    date[7] = crc16[1];// Number of data to read寄存器数量低字节  
                    m_strCmd = date;
                    break;

                case MemAreaModbusRTUD.ReadDiscreteInputs://读离散输入状态10001-19999位操作(单个或多个)****功能码02H
                    date = new byte[8];
                    date[0] = 0x01;//从站地址
                    date[1] = 0x02;//功能码
                    date[2] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                    date[3] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                    date[4] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);// Number of data to read寄存器数量高字节
                    date[5] = Convert.ToByte(m_unWordsCount& 0xFF);// Number of data to read寄存器数量低字节
                    crc16 = CRCStuff.calculateCRC(ref date, 6);
                    date[6] = crc16[0];// Number of data to read寄存器数量高字节
                    date[7] = crc16[1];// Number of data to read寄存器数量低字节
                    m_strCmd = date;
                    break;

                case MemAreaModbusRTUD.ReadHoldingRegister://读保持寄存器40001-49999字操作(单个或多个)****功能码03H
                    date = new byte[8];
                    date[0] = 0x01;//从站地址
                    date[1] = 0x03;//功能码
                    date[2] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                    date[3] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                    date[4] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);// Number of data to read寄存器数量高字节
                    date[5] = Convert.ToByte(m_unWordsCount& 0xFF);// Number of data to read寄存器数量低字节
                    crc16 = CRCStuff.calculateCRC(ref date, 6);
                    date[6] = crc16[0];// Number of data to read寄存器数量高字节
                    date[7] = crc16[1];// Number of data to read寄存器数量低字节
                    m_strCmd = date;
                    break;

                case MemAreaModbusRTUD.ReadInputRegister:////读输入寄存器30001-39999字操作(单个或多个)****功能码04H
                    date = new byte[8];
                    date[0] = 0x01;//从站地址
                    date[1] = 0x04;//功能码
                    date[2] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                    date[3] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                    date[4] = Convert.ToByte((m_unWordsCount >> 8) & 0xFF);// Number of data to read寄存器数量高字节
                    date[5] = Convert.ToByte(m_unWordsCount& 0xFF);// Number of data to read寄存器数量低字节
                    crc16 = CRCStuff.calculateCRC(ref date, 6);
                    date[6] = crc16[0];// Number of data to read寄存器数量高字节
                    date[7] = crc16[1];// Number of data to read寄存器数量低字节
                    m_strCmd = date;
                    break;

                case MemAreaModbusRTUD.WriteSingleCoil://写单个线圈00001-09999位操作(单个)****功能码05H    
                    date = new byte[8];
                    date[0] = 0x01;//从站地址
                    date[1] = 0x05;//功能码
                    date[2] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                    date[3] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                    if (m_bBitWrite)
                    {
                        date[4] = 0xFF;//数据1高字节=0xFF  数据2低字节=00 数据1高字节
                        date[5] = 0x00;//数据2低字节
                    }
                    else
                    {
                        date[4] = 0x00;
                        date[5] = 0x00;
                    }
                    crc16 = CRCStuff.calculateCRC(ref date, 6);
                    date[6] = crc16[0];// Number of data to read寄存器数量高字节
                    date[7] = crc16[1];// Number of data to read寄存器数量低字节
                    m_strCmd = date;
                    break;

                case MemAreaModbusRTUD.WriteSingleRegister://写单个保持寄存器40001-49999字操作(单个)****功能码06H
                    date = new byte[8];
                    date[0] = 0x01;//从站地址
                    date[1] = 0x06;//功能码
                    date[2] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                    date[3] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                    date[4] = values[1];//Number of data to read寄存器数量高字节
                    date[5] = values[0];//Number of data to read寄存器数量低字节
                    crc16 = CRCStuff.calculateCRC(ref date, 6);
                    date[6] = crc16[0];// Number of data to read寄存器数量高字节
                    date[7] = crc16[1];// Number of data to read寄存器数量低字节
                    m_strCmd = date;
                    break;

                case MemAreaModbusRTUD.WriteMultipleCoils://写多个线圈00001-09999位操作(多个)****功能码0FH              
                    len = values.Length;
                    if (len % 2 > 0) len++;
                    date = new byte[len + 9];
                    date[0] = 0x01;//从站地址
                    date[1] = 0x0F;//功能码
                    date[2] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                    date[3] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                    //date[4] = Convert.ToByte(m_unWordsCount >> 8);//Number of data to read寄存器数量高字节
                    //date[5] = Convert.ToByte(m_unWordsCount);//Number of data to read寄存器数量低字节
                   _length = BitConverter.GetBytes((short)(len >> 1));
                    date[4] = _length[1];			// Number of data to read
                    date[5] = _length[0];			// Number of data to read
                    date[6] = (byte)len;//字节数
                    Array.Copy(values, 0, date, 7, values.Length);
                    crc16 = CRCStuff.calculateCRC(ref date, len + 7);
                    date[7 + len] = crc16[0];// CRC校验高字节
                    date[8 + len] = crc16[1];// CRC校验低字节
                    m_strCmd = date;
                    break;

                case MemAreaModbusRTUD.WriteMultipleRegister://写多个保持寄存器40001-49999字操作(多个)****功能码10H           
                    len = values.Length;
                    if (len % 2 > 0)
                    {
                        len++;
                    }
                    date = new byte[len + 9];
                    date[0] = 0x01;//从站地址
                    date[1] = 0x10;//功能码
                    date[2] = Convert.ToByte((m_unBeginWord >> 8) & 0xFF);		// Start address//寄存器起始地址高字节
                    date[3] = Convert.ToByte(m_unBeginWord & 0xFF);				// Start address//寄存器起始地址低字节
                     _length = BitConverter.GetBytes((short)(len >> 1));
                    date[4] = _length[1];			// Number of data to read
                    date[5] = _length[0];			// Number of data to read
                    date[6] = (byte)len;
                    Array.Copy(values, 0, date, 7, values.Length);
                    crc16 = CRCStuff.calculateCRC(ref date, len + 7);
                    date[len + 7] = crc16[0];// Number of data to read寄存器数量高字节
                    date[len + 8] = crc16[1];// Number of data to read寄存器数量低字节
                    m_strCmd = date;
                    break;
            }
        }    
    }