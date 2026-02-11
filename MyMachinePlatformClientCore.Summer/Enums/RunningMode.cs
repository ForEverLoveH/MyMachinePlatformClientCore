using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Enums
{
    /// <summary>机构运行模式</summary>
    public enum RunningMode
    {
        /// <summary>未确定。</summary>
        None,
        /// <summary>流水线 Online 模式。</summary>
        Online,
        /// <summary>空载模式 Offine 模式。</summary>
        Noload,
        /// <summary>调试模式。</summary>
        Debug,
    }
}
