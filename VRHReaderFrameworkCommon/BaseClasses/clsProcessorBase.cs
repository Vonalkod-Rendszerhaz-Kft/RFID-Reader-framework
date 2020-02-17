using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkCommon
{
    abstract public class clsProcessorBase
    {
        abstract public void LoadConfig(string name);
        abstract public System.Collections.Generic.List<clsAction> Process(ref clsReadResult oReadResult);
    }
}
