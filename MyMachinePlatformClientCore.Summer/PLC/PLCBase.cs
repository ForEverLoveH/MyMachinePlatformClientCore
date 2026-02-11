using HslCommunication;
using HslCommunication.Core;
using Microsoft.Extensions.Logging;
using MyMachinePlatformClientCore.Summer.Common;
using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class PLCBase:Automatic, IPLC, IAutomatic,IDevice,IObject
    {
        private IReadWriteNet _client;
        private bool _isConnected;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public bool IsConnected
        {
            get => this._isConnected;
            protected set
            {
                this._isConnected = value;
                Action<IDevice> connectedChanged = this.IsConnectedChanged;
                if (connectedChanged == null)
                    return;
                connectedChanged((IDevice)this);
            }
        }
        public virtual Action<string>  LogMessageCallback { get; set; }

        /// <summary>是否启用日志记录 当开启时，每次对plc的写入值时都会记录日志,默认false</summary>
        public bool IsEnabledLogger { get; set; }
        public string Name { get ; set  ; }
        public string ID { get  ; set  ; }
        public string Description { get ; set ; }

        /// <inheritdoc />
        public event Action<IDevice> IsConnectedChanged;

        /// <inheritdoc />
        public abstract void Connect();

        /// <inheritdoc />
        public abstract void Disconnect();

        /// <summary>设置操作客户端</summary>
        /// <param name="client"></param>
        protected void SetClient(IReadWriteNet client) => this._client = client;

        /// <summary>
        /// 读取一个<see cref="T:System.Boolean" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回Boolean</returns>
        public virtual bool ReadBoolean(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<bool>(this._client.ReadBool(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Boolean" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回Boolean数组</returns>
        public virtual bool[] ReadBoolean(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<bool[]>(this._client.ReadBool(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Byte" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回Byte</returns>
        public virtual byte ReadByte(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<byte[]>(this._client.Read(address, (ushort)1))[0];
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Byte" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回Byte数组</returns>
        public virtual byte[] ReadByte(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<byte[]>(this._client.Read(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Double" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回double</returns>
        public virtual double ReadDouble(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<double>(this._client.ReadDouble(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Double" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回double数组</returns>
        public virtual double[] ReadDouble(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<double[]>(this._client.ReadDouble(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Single" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回float</returns>
        public virtual float ReadFloat(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<float>(this._client.ReadFloat(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Single" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回float数组</returns>
        public virtual float[] ReadFloat(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<float[]>(this._client.ReadFloat(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Int32" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回Int</returns>
        public virtual int ReadInt(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<int>(this._client.ReadInt32(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Int32" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回Int数组</returns>
        public virtual int[] ReadInt(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<int[]>(this._client.ReadInt32(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Int64" /> (Int64)值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回long(Int64)</returns>
        public virtual long ReadLong(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<long>(this._client.ReadInt64(address));
        }

        /// <summary>
        ///  读取一个<see cref="T:System.Int64" /> (Int64)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回long(Int64)数组</returns>
        public virtual long[] ReadLong(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<long[]>(this._client.ReadInt64(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Int16" /> (Int16)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回short(Int16)</returns>
        public virtual short ReadShort(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<short>(this._client.ReadInt16(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Int16" /> (Int16)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回short(Int16)数组</returns>
        public virtual short[] ReadShort(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<short[]>(this._client.ReadInt16(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.String" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回string</returns>
        public virtual string ReadString(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<string>(this._client.ReadString(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt32" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回uint</returns>
        public virtual uint ReadUInt(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<uint>(this._client.ReadUInt32(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt32" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回uint数组</returns>
        public virtual uint[] ReadUInt(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<uint[]>(this._client.ReadUInt32(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt64" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回ulong</returns>
        public virtual ulong ReadULong(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<ulong>(this._client.ReadUInt64(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt64" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回ulong数组</returns>
        public virtual ulong[] ReadULong(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<ulong[]>(this._client.ReadUInt64(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt16" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回ushort</returns>
        public virtual ushort ReadUShort(string address)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<ushort>(this._client.ReadUInt16(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt16" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回ushort数组</returns>
        public virtual ushort[] ReadUShort(string address, ushort length)
        {
            this.ThrowIfDisconnect();
            return this.HandleResult<ushort[]>(this._client.ReadUInt16(address, length));
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Boolean" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, bool value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Boolean" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, bool[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<bool>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Byte" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, byte value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, (short)value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Byte" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, byte[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<byte>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int16" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, short value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int16" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, short[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<short>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt16" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, ushort value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt16" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, ushort[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<ushort>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int32" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, int value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int32" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, int[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<int>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt32" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, uint value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt32" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, uint[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<uint>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int64" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, long value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int64" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, long[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<long>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt64" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, ulong value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt64" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, ulong[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<ulong>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Single" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, float value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Single" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, float[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<float>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Double" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, double value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Double" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual void Write(string address, double[] values)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, values);
            this.LogWrite<double>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.String" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual void Write(string address, string value)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.String" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        /// <param name="length">数据长度</param>
        public virtual void Write(string address, string value, int length)
        {
            this.ThrowIfDisconnect();
            OperateResult result = this._client.Write(address, value, length);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Byte" /> (byte)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回Byte数组</returns>
        public virtual async Task<byte[]> ReadAsync(string address, ushort length)
        {
            return this.HandleResult<byte[]>(await this._client.ReadAsync(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Boolean" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回bool</returns>
        public virtual async Task<bool> ReadBoolAsync(string address)
        {
            return this.HandleResult<bool>(await this._client.ReadBoolAsync(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Boolean" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回bool数组</returns>
        public virtual async Task<bool[]> ReadBoolAsync(string address, ushort length)
        {
            return this.HandleResult<bool[]>(await this._client.ReadBoolAsync(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Int16" /> (Int16)值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回short(Int16)</returns>
        public virtual async Task<short> ReadInt16Async(string address)
        {
            return this.HandleResult<short>(await this._client.ReadInt16Async(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Int16" /> (Int16)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回short(Int16)数组</returns>
        public virtual async Task<short[]> ReadInt16Async(string address, ushort length)
        {
            return this.HandleResult<short[]>(await this._client.ReadInt16Async(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt16" /> (UInt16)值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回ushort(UInt16)</returns>
        public virtual async Task<ushort> ReadUInt16Async(string address)
        {
            return this.HandleResult<ushort>(await this._client.ReadUInt16Async(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt16" /> (UInt16)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回ushort(UInt16)数组</returns>
        public virtual async Task<ushort[]> ReadUInt16Async(string address, ushort length)
        {
            return this.HandleResult<ushort[]>(await this._client.ReadUInt16Async(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Int32" /> (Int32)值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回int(Int32)</returns>
        public virtual async Task<int> ReadInt32Async(string address)
        {
            return this.HandleResult<int>(await this._client.ReadInt32Async(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Int32" /> (Int32)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回int(Int32)数组</returns>
        public virtual async Task<int[]> ReadInt32Async(string address, ushort length)
        {
            return this.HandleResult<int[]>(await this._client.ReadInt32Async(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt32" /> (UInt32)值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回uint(UInt32)</returns>
        public virtual async Task<uint> ReadUInt32Async(string address)
        {
            return this.HandleResult<uint>(await this._client.ReadUInt32Async(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt32" /> (UInt32)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回uint(UInt32)数组</returns>
        public virtual async Task<uint[]> ReadUInt32Async(string address, ushort length)
        {
            return this.HandleResult<uint[]>(await this._client.ReadUInt32Async(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt32" /> (UInt64)值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回long(UInt64)</returns>
        public virtual async Task<long> ReadInt64Async(string address)
        {
            return this.HandleResult<long>(await this._client.ReadInt64Async(address));
        }

        /// <summary>
        ///  读取一个<see cref="T:System.Int64" /> (Int64)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回long(Int64)数组</returns>
        public virtual async Task<long[]> ReadInt64Async(string address, ushort length)
        {
            return this.HandleResult<long[]>(await this._client.ReadInt64Async(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt64" /> (Int64)值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回ulong(UInt64)</returns>
        public virtual async Task<ulong> ReadUInt64Async(string address)
        {
            return this.HandleResult<ulong>(await this._client.ReadUInt64Async(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.UInt64" /> (Int64)数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回ulong(UInt64)</returns>
        public virtual async Task<ulong[]> ReadUInt64Async(string address, ushort length)
        {
            return this.HandleResult<ulong[]>(await this._client.ReadUInt64Async(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Single" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回float</returns>
        public virtual async Task<float> ReadFloatAsync(string address)
        {
            return this.HandleResult<float>(await this._client.ReadFloatAsync(address));
        }

        /// <summary>
        ///  读取一个<see cref="T:System.Single" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回float数组值</returns>
        public virtual async Task<float[]> ReadFloatAsync(string address, ushort length)
        {
            return this.HandleResult<float[]>(await this._client.ReadFloatAsync(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Double" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <returns>返回dobule</returns>
        public virtual async Task<double> ReadDoubleAsync(string address)
        {
            return this.HandleResult<double>(await this._client.ReadDoubleAsync(address));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.Double" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回dobule数组</returns>
        public virtual async Task<double[]> ReadDoubleAsync(string address, ushort length)
        {
            return this.HandleResult<double[]>(await this._client.ReadDoubleAsync(address, length));
        }

        /// <summary>
        /// 读取一个<see cref="T:System.String" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回string</returns>
        public virtual async Task<string> ReadStringAsync(string address, ushort length)
        {
            return this.HandleResult<string>(await this._client.ReadStringAsync(address, length));
        }

        /// <summary>
        ///  读取一个<see cref="T:System.String" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="length">数据长度</param>
        /// <param name="encoding">编码格式</param>
        /// <returns>返回string</returns>
        public virtual async Task<string> ReadStringAsync(
          string address,
          ushort length,
          Encoding encoding)
        {
            return this.HandleResult<string>(await this._client.ReadStringAsync(address, length, encoding));
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Byte" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, byte[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<byte>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Boolean" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, bool[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<bool>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Boolean" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, bool value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int16" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, short value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int16" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, short[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<short>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt16" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, ushort value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt16" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, ushort[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<ushort>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int32" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, int value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int32" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, int[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<int>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt32" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, uint value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt32" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, uint[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<uint>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int64" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, long value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Int64" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, long[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<long>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt64" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, ulong value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.UInt64" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, ulong[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<ulong>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Single" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, float value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Single" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, float[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<float>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Double" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, double value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.Double" /> 数组值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">数据内容</param>
        public virtual async Task WriteAsync(string address, double[] values)
        {
            OperateResult result = await this._client.WriteAsync(address, values);
            this.LogWrite<double>(address, values);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.String" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        public virtual async Task WriteAsync(string address, string value)
        {
            OperateResult result = await this._client.WriteAsync(address, value);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.String" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        /// <param name="encoding">编码格式</param>
        public virtual async Task WriteAsync(string address, string value, Encoding encoding)
        {
            OperateResult result = await this._client.WriteAsync(address, value, encoding);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.String" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        /// <param name="length">数据长度</param>
        public virtual async Task WriteAsync(string address, string value, int length)
        {
            OperateResult result = await this._client.WriteAsync(address, value, length);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>
        /// 写入一个<see cref="T:System.String" /> 值
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">数据内容</param>
        /// <param name="length">数据长度</param>
        /// <param name="encoding">编码格式</param>
        public virtual async Task WriteAsync(
          string address,
          string value,
          int length,
          Encoding encoding)
        {
            OperateResult result = await this._client.WriteAsync(address, value, length, encoding);
            this.LogWrite(address, (object)value);
            this.ThrowIfFailed(result);
        }

        /// <summary>处理当前返回值信息</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        protected virtual T HandleResult<T>(OperateResult<T> result)
        {
            this.ThrowIfFailed((OperateResult)result);
            return result.Content;
        }

        /// <summary>记录日志</summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        protected void LogWrite(string address, object value)
        {
            if (!this.IsEnabledLogger)
                return;
            string description = this.GetDescription();

            LogMessageCallback?.Invoke($"PLC [{description}] 已写入 地址[{address}] 值[{value}]");
        }

        /// <summary>记录日志</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="values"></param>
        protected void LogWrite<T>(string address, T[] values)
        {
            if (!this.IsEnabledLogger)
                return;
            string description = this.GetDescription();

            LogMessageCallback?.Invoke($"PLC [{description}] 已写入 地址[{address}] 值[{string.Join<T>(",", (IEnumerable<T>)values)}]");
        }

        /// <summary>
        /// 获取当前设备描述 默认为当前<see cref="P:VIA.Integration.Automatic.Name" />
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDescription() => this.Name;

        /// <summary>如果失败则抛出异常</summary>
        /// <param name="result"></param>
        /// <exception cref="T:System.InvalidOperationException"></exception>
        [DebuggerStepThrough]
        protected virtual void ThrowIfFailed(OperateResult result)
        {
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.ToMessageShowString());
        }

        /// <summary>如果PLC未连接则抛出异常</summary>
        /// <exception cref="T:System.InvalidOperationException"></exception>
        [DebuggerStepThrough]
        protected void ThrowIfDisconnect()
        {
            if (this._client == null)
                throw new InvalidOperationException("PLC未连接");
        }
    }
}
