using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.ModbusService
{
    public enum FunctionCode
    {
        /// <summary>
        /// 01读取单个线圈
        /// </summary>
        ReadCoils,

        /// <summary>
        /// 02读取输入线圈/离散量线圈
        /// </summary>
        ReadInputs,

        /// <summary>
        /// 03读取保持寄存器
        /// </summary>
        ReadHoldingRegisters,

        /// <summary>
        /// 04读取输入寄存器
        /// </summary>
        ReadInputRegisters,

        /// <summary>
        /// 05写单个线圈
        /// </summary>
        WriteSingleCoilAsync,

        /// <summary>
        /// 06写单个输入线圈/离散量线圈/单个寄存器
        /// </summary>
        WriteSingleRegisterAsync,

        /// <summary> 
        /// 0x0F写多个线圈   0x0F=15
        /// </summary>
        WriteMultipleCoilsAsync,

        /// <summary>
        /// 0x10写多个保持寄存器  0x10=16
        /// </summary>
        WriteMultipleRegistersAsync

    }
}
