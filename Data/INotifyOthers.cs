using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    [ServiceContract]
    public interface INotifyOthers
    {
        [OperationContract]
        void Notify(string identifier);
    }
}
