using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Common
{
    public interface IPLC :IDevice
    {
        bool ReadBoolean(string address);

        bool[] ReadBoolean(string address, ushort length);

        byte ReadByte(string address);

        byte[] ReadByte(string address, ushort length);

        short ReadShort(string address);

        short[] ReadShort(string address, ushort length);

        ushort ReadUShort(string address);

        ushort[] ReadUShort(string address, ushort length);

        int ReadInt(string address);

        int[] ReadInt(string address, ushort length);

        uint ReadUInt(string address);

        uint[] ReadUInt(string address, ushort length);

        long ReadLong(string address);

        long[] ReadLong(string address, ushort length);

        ulong ReadULong(string address);

        ulong[] ReadULong(string address, ushort length);

        float ReadFloat(string address);

        float[] ReadFloat(string address, ushort length);

        double ReadDouble(string address);

        double[] ReadDouble(string address, ushort length);

        string ReadString(string address, ushort length);

        void Write(string address, bool value);

        void Write(string address, bool[] values);

        void Write(string address, byte value);

        void Write(string address, byte[] values);

        void Write(string address, short value);

        void Write(string address, short[] values);

        void Write(string address, ushort value);

        void Write(string address, ushort[] values);

        void Write(string address, int value);

        void Write(string address, int[] values);

        void Write(string address, uint value);

        void Write(string address, uint[] values);

        void Write(string address, long value);

        void Write(string address, long[] values);

        void Write(string address, ulong value);

        void Write(string address, ulong[] values);

        void Write(string address, float value);

        void Write(string address, float[] values);

        void Write(string address, double value);

        void Write(string address, double[] values);

        void Write(string address, string value);

        void Write(string address, string value, int length);

        Task<byte[]> ReadAsync(string address, ushort length);

        Task<bool> ReadBoolAsync(string address);

        Task<bool[]> ReadBoolAsync(string address, ushort length);

        Task<short> ReadInt16Async(string address);

        Task<short[]> ReadInt16Async(string address, ushort length);

        Task<ushort> ReadUInt16Async(string address);

        Task<ushort[]> ReadUInt16Async(string address, ushort length);

        Task<int> ReadInt32Async(string address);

        Task<int[]> ReadInt32Async(string address, ushort length);

        Task<uint> ReadUInt32Async(string address);

        Task<uint[]> ReadUInt32Async(string address, ushort length);

        Task<long> ReadInt64Async(string address);

        Task<long[]> ReadInt64Async(string address, ushort length);

        Task<ulong> ReadUInt64Async(string address);

        Task<ulong[]> ReadUInt64Async(string address, ushort length);

        Task<float> ReadFloatAsync(string address);

        Task<float[]> ReadFloatAsync(string address, ushort length);

        Task<double> ReadDoubleAsync(string address);

        Task<double[]> ReadDoubleAsync(string address, ushort length);

        Task<string> ReadStringAsync(string address, ushort length);

        Task<string> ReadStringAsync(string address, ushort length, Encoding encoding);

        Task WriteAsync(string address, byte[] values);

        Task WriteAsync(string address, bool[] values);

        Task WriteAsync(string address, bool value);

        Task WriteAsync(string address, short value);

        Task WriteAsync(string address, short[] values);

        Task WriteAsync(string address, ushort value);

        Task WriteAsync(string address, ushort[] values);

        Task WriteAsync(string address, int value);

        Task WriteAsync(string address, int[] values);

        Task WriteAsync(string address, uint value);

        Task WriteAsync(string address, uint[] values);

        Task WriteAsync(string address, long value);

        Task WriteAsync(string address, long[] values);

        Task WriteAsync(string address, ulong value);

        Task WriteAsync(string address, ulong[] values);

        Task WriteAsync(string address, float value);

        Task WriteAsync(string address, float[] values);

        Task WriteAsync(string address, double value);

        Task WriteAsync(string address, double[] values);

        Task WriteAsync(string address, string value);

        Task WriteAsync(string address, string value, Encoding encoding);

        Task WriteAsync(string address, string value, int length);

        Task WriteAsync(string address, string value, int length, Encoding encoding);
    }
}
