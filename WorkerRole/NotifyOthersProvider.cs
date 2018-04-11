using Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    public class NotifyOthersProvider : INotifyOthers
    {
        public void Notify(string value)
        {
            Trace.TraceInformation("Primljen je artikal koji ima vrednost: {0}.", value);
        }
    }
}
