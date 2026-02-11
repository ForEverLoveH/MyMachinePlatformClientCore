using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Enums
{
    public enum  SiemensS7Type
    {
        S1200 = 1,
        //
        // 摘要:
        //     S300系列
        S300,
        //
        // 摘要:
        //     S400系列
        S400,
        //
        // 摘要:
        //     S1500系列
        S1500,
        //
        // 摘要:
        //     200的smart系列
        S200Smart,
        //
        // 摘要:
        //     200系统，需要额外配置以太网模块
        S200
    }
}
