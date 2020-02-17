using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkCommon
{
    public enum eControllerResultRequestType
    {
        NoResult = 0,
        Unfiltered = 1,
        Filtered = 2
    }

    abstract public class clsControllerBase
    {
        abstract public void LoadConfig(string name , string basedir , VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig);
        abstract public int GetCycle();
        abstract public void CycleDone();
        abstract public void SetResults(System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colReadResults);
        abstract public VRHReaderFrameworkCommon.eControllerResultRequestType GetResultRequestType();
        abstract public void StartController();
        abstract public void StopController();
        abstract public System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction> GetControllerActions();
    }
}
