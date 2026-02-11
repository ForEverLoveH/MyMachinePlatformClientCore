using Microsoft.Extensions.Logging;
using MyMachinePlatformClientCore.Summer.Enums;
using MyMachinePlatformClientCore.Summer.Events;
using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Common
{
    public interface IPart : ITreeNode<IPart>, IObject, IRecipeObject
    {
        /// <summary>设备运行状态改变事件。</summary>
        event EventHandler<PartStateChangedEventArgs> StatusChanged;

        /// <summary>设备元器件枚举数。</summary>
        IEnumerable<IAutomatic> Automatics { get; }

        ILogger Logger { get; }

        IServiceProvider Service { get; }

        /// <summary>元器件索引器。</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IAutomatic this[string name] { get; }

        /// <summary>报警处理。</summary>
        void OnAlarmChanged(AlarmEventArgs e);

        /// <summary>报警状态改变事件</summary>
        event EventHandler<AlarmEventArgs> AlarmStatusChanged;

        /// <summary>暂停运动任务线程。</summary>
        void Pause();

        /// <summary>唤醒运动任务线程。</summary>
        void Resume();

        /// <summary>异步运行运动任务线程。</summary>
        /// <param name="runningMode">运行模式。</param>
        void Run(RunningMode runningMode);

        /// <summary>异步停止运动任务线程。</summary>
        void Stop();
    }
}
