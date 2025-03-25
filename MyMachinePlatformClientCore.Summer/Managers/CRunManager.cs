using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MyMachinePlatformClientCore.Summer.Common;
 

namespace MyMachinePlatformClientCore.Summer.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class CRunManager
    {
        /// <summary>
        /// 
        /// </summary>
        private  ConcurrentDictionary<string, CRun> _cRuns = new ConcurrentDictionary<string, CRun>();
         
        public  void Add(string key,CRun cRun)
        {
            _cRuns.TryAdd(key, cRun);
        }

        private void Remove(string key)
        {
            _cRuns.TryRemove(key, out CRun cRun);
        }

        private bool isStart;
        public bool IsStart
        {
            get => isStart;
            set
            {
                isStart = value;
                if (_cRuns.Values.Count > 0)
                {
                    foreach (var cRun in _cRuns.Values)
                    {
                        cRun.IsStarted = value;
                    }
                }
            }
        }
        private bool isStop; 
        public bool IsStop
        {
            get => isStop;
            set => isStop = value;
        }

        public void Stop()
        {
            if (isStop)
            {
                if (_cRuns.Count > 0)
                {
                    foreach (var cRun in _cRuns.Values)
                    {
                        cRun.Stop();
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if (isStart)
            {
                if (_cRuns.Count > 0)
                {
                    foreach (var cRun in _cRuns.Values)
                    {
                         cRun.Start();
                    }
                }
            }
        }

        public void Init()
        {
            HotPressProcess hotPressProcess = new HotPressProcess("热压");
            Add("热压", hotPressProcess);
           HotPressProcess1  hotPressProcess1 = new HotPressProcess1("热压1");
            Add("热压1", hotPressProcess1);
            IsStart = true;
            Start();
        }
    }
}
