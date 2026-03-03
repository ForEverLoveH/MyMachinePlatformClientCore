


using MyMachinePlatformClientCore.Common.Integration;
using MyMachinePlatformClientCore.Common.Options;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Summer
{
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOption"></typeparam>
    public interface IModbusMaster<TOption> : IDevice, IAutomatic,IHasOption<TOption>   where TOption :  IOption
    {
        /// <summary>
        /// 
        /// </summary>
        event Action<LogMessage>? LogMessageDataCallBack;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionCode"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="startAddress"></param>
        /// <param name="coilsBuffer"></param>
        /// <param name="registerBuffer"></param>
        /// <returns></returns>
        Task WriteData(FunctionCode functionCode, byte slaveAddress, ushort startAddress, bool[]? coilsBuffer = null,
            ushort[]? registerBuffer = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionCode"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="startAddress"></param>
        /// <param name="numberOfPoints"></param>
        /// <returns></returns>
        Task<(bool[]?, ushort[]?)> ReadData(FunctionCode functionCode, byte slaveAddress, ushort startAddress,
            ushort numberOfPoints);
    }
}