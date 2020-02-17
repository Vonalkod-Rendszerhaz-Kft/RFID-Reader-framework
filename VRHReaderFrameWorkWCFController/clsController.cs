using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameWorkWCFController
{
    class clsController : VRHReaderFrameworkCommon.clsControllerBase
    {
        VRHReaderFrameworkCommon.clsReaderThreadConfig _oRederThreadConfig = null;

        public override void LoadConfig(string name, string basedir, VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            _oRederThreadConfig = oRederThreadConfig;

            VRHReaderFrameworkWCFInterface.clsWCFStatic.LoadConfig(name, basedir, _oRederThreadConfig);
        }

        public override int GetCycle()
        {
            return VRHReaderFrameworkWCFInterface.clsWCFStatic.GetCycle(_oRederThreadConfig);
        }

        public override void CycleDone()
        {
            VRHReaderFrameworkWCFInterface.clsWCFStatic.CycleDone(_oRederThreadConfig);
        }

        public override void SetResults(List<VRHReaderFrameworkCommon.clsReadResult> colReadResults)
        {
            VRHReaderFrameworkWCFInterface.clsWCFStatic.SetResults(colReadResults, _oRederThreadConfig);
        }

        public override VRHReaderFrameworkCommon.eControllerResultRequestType GetResultRequestType()
        {
            return VRHReaderFrameworkWCFInterface.clsWCFStatic.GetResultRequestType(_oRederThreadConfig);
        }

        public override void StartController()
        {
            VRHReaderFrameworkWCFInterface.clsWCFStatic.StartController(_oRederThreadConfig);
        }

        public override void StopController()
        {
            VRHReaderFrameworkWCFInterface.clsWCFStatic.StopController(_oRederThreadConfig);
        }

        public override List<VRHReaderFrameworkCommon.clsAction> GetControllerActions()
        {
            return VRHReaderFrameworkWCFInterface.clsWCFStatic.GetControllerActions(_oRederThreadConfig).ToList();
        }
    }
}
