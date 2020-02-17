using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkCommon
{
    abstract public class clsReaderFilterBase
    {
        abstract public void LoadConfig(string name);
        abstract public System.Collections.Generic.List<clsReadResult> Filter(System.Collections.Generic.List<clsReadResult> colResults);
    }
}
