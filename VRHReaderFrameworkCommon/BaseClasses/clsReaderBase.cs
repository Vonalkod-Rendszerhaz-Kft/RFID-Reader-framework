using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkCommon
{
    abstract public class clsReaderBase
    {
        public clsReaderThreadConfig oReaderThreadConfig;

        abstract public void OpenReader();
        abstract public void CloseReader();
        abstract public void ReadStart();
        abstract public void ReadStop();
        abstract public System.Collections.Generic.List<clsReadResult> ReadPoll(bool bRead);
        abstract public clsReadResult ProcessAction(clsAction oAction);
        abstract public void LoadConfig(string name, string id);
        abstract public void Ping();

    }
}
