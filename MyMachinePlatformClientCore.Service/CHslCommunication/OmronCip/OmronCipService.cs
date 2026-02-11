using HslCommunication.Profinet.Omron;
using MyMachinePlatformClientCore.Log.MyLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.IService.CHslCommunication;

namespace MyMachinePlatformClientCore.Service.CHslCommunication
{

    /// <summary>
    /// 欧姆龙Cip服务
    /// </summary>
    public class OmronCipService : IPLCService, IDisposable
    {
        private string ipaddress;
        private int port;
        private int timeOut;


        private OmronCipNet omronCipNet;

        private bool isConnect;


        public bool IsConnect
        {
            get { return isConnect; }
        }

        public Action<LogMessage> LogMessageCallBack { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress">ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="timeOut">超时时间</param>
        /// <param name="station">站号</param>
        public OmronCipService(string ipaddress, int port, int timeOut, Action<LogMessage> logMessageCallBack = null)
        {
            this.ipaddress = ipaddress;
            this.port = port;
            this.timeOut = timeOut;
            this.LogMessageCallBack = logMessageCallBack;
            omronCipNet = new OmronCipNet()
            {
                IpAddress = ipaddress,
                Port = port,
                ConnectTimeOut = timeOut,    // 连接超时时间

            };

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public async Task<bool> StartService()
        {
            try
            {
                if (omronCipNet == null || isConnect) return false;
                var connectResult = await omronCipNet.ConnectServerAsync();
                if (connectResult.IsSuccess)
                {
                    isConnect = true;
                    LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO, "欧姆龙Cip连接成功"));
                    //启动计时器监控是否断开链接


                    return true;
                }
                else
                {
                    LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在链接欧姆龙Cip时，发生异常，异常信息为;" + connectResult.Message));
                    return false;
                }

            }
            catch (Exception ex)
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在链接欧姆龙Cip时，发生异常，异常信息为;" + ex.Message));
                return false;
            }
        }

        #region  读取

        /// <summary>
        ///
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public async Task<byte[]> ReadByte(string address, ushort length)
        {
            if (omronCipNet == null || !isConnect) return null;
            var result = await omronCipNet.ReadAsync(address, length);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return null;
            }
        }
        /// <summary>
        /// 读取Int16
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<short> ReadInt16(string address)
        {
            if (omronCipNet == null || !isConnect) return 0;
            var result = await omronCipNet.ReadInt16Async(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<ushort> ReadUInt16(string address)
        {
            if (omronCipNet == null || !isConnect) return 0;
            var result = await omronCipNet.ReadUInt16Async(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {

                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return 0;
            }
        }
        /// <summary>
        /// 读取Int32
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<int> ReadInt32(string address)
        {
            if (omronCipNet == null || !isConnect) return 0;
            var result = await omronCipNet.ReadInt32Async(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return 0;
            }
        }
        /// <summary>
        /// 读取UInt32
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<uint> ReadUInt32(string address)
        {
            if (omronCipNet == null || !isConnect) return 0;
            var result = await omronCipNet.ReadUInt32Async(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return 0;
            }
        }
        /// <summary>
        /// 读取Float
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<float> ReadFloat(string address)
        {
            if (omronCipNet == null || !isConnect) return 0;
            var result = await omronCipNet.ReadFloatAsync(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return 0;
            }
        }
        /// <summary>
        /// 读取Double
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<double> ReadDouble(string address)
        {
            if (omronCipNet == null || !isConnect) return 0;
            var result = await omronCipNet.ReadDoubleAsync(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return 0;
            }
        }
        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public async Task<string> ReadString(string address, ushort length)
        {
            if (omronCipNet == null || !isConnect) return null;
            var result = await omronCipNet.ReadStringAsync(address, length);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return null;
            }
        }
        /// <summary>
        /// 读取UInt64
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<ulong> ReadUInt64(string address)
        {
            if (omronCipNet == null || !isConnect) return 0;
            var result = await omronCipNet.ReadUInt64Async(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return 0;
            }
        }
        /// <summary>
        /// 读取Int64
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<long> ReadInt64(string address)
        {
            if (omronCipNet == null || !isConnect) return 0;
            var result = await omronCipNet.ReadInt64Async(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "在读取欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return 0;
            }
        }

        #endregion
        #region 写入数据
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public async Task<bool> Write(string address, ushort value)
        {
            if (omronCipNet == null || !isConnect) return false;
            var result = await omronCipNet.WriteAsync(address, value);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return false;
            }
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> Write(string address, int value)
        {
            if (omronCipNet == null || !isConnect) return false;
            var result = await omronCipNet.WriteAsync(address, value);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> Write(string address, short value)
        {
            if (omronCipNet == null || !isConnect) return false;
            var result = await omronCipNet.WriteAsync(address, value);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return false;
            }
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> Write(string address, short[] value)
        {
            if (omronCipNet == null || !isConnect) return false;
            var result = await omronCipNet.WriteAsync(address, value);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return false;
            }
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> Write(string address, float value)
        {
            if (omronCipNet == null || !isConnect) return false;
            var result = await omronCipNet.WriteAsync(address, value);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> Write(string address, double value)
        {
            if (omronCipNet == null || !isConnect) return false;
            var result = await omronCipNet.WriteAsync(address, value);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return false;
            }
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public async Task<bool> Write(string address, uint value)
        {
            if (omronCipNet == null || !isConnect) return false;
            var result = await omronCipNet.WriteAsync(address, value);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return false;
            }
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public async Task<bool> Write(string address, byte[] value)
        {
            if (omronCipNet == null || !isConnect) return false;
            var result = await omronCipNet.WriteAsync(address, value);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return false;
            }
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> Write(string address, ulong value)
        {
            if (omronCipNet == null || !isConnect) return false;
            var result = await omronCipNet.WriteAsync(address, value);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Cip时，发生异常，异常信息为;" + result.Message));
                return false;
            }
        }
        #endregion
        /// <summary>
        /// 断开连接
        /// </summary>
        public async Task<bool> StopService()
        {
            if (omronCipNet != null)
            {
                var res = await omronCipNet.ConnectCloseAsync();
                if (res.IsSuccess)
                {
                    omronCipNet = null;
                    isConnect = false;
                    return true;
                }
                else
                {
                    LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在断开欧姆龙Cip连接时，发生异常，异常信息为;" + res.Message));
                    return false;
                }
            }
            return false;
        }

        public void Dispose()
        {
            StopService();
        }
    }
}
