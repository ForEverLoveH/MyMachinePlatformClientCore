using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Events
{
    public class CancelEventArgs<T> : EventArgs
    {
        public T Target { get; set; }

        public bool Cancel { get; set; }
    }
}
