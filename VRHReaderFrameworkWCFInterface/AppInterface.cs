using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace VRHReaderFrameworkWCFInterface
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AppInterface" in both code and config file together.
    public class AppInterface : IAppInterface
    {
        public ResultBase Read(string sReaderId, int iReadSeconds , int iWaitExecutionSeconds)
        {
            return new ResultBase(clsWCFStatic.StartTimedoutRead(sReaderId, iReadSeconds,iWaitExecutionSeconds));
        }

        public ResultBase ExecuteReaderCommands(string sReaderId, string sConfigFile, string sCommandSet, int iWaitExecutionSeconds)
        {
            return new ResultBase(clsWCFStatic.ExecuteReaderCommands(sReaderId, sConfigFile, sCommandSet, iWaitExecutionSeconds));
        }

        public ResultBase ExecuteReaderCommand(string sReaderId, string sCommand, int iWaitExecutionSeconds)
        {
            return new ResultBase(clsWCFStatic.ExecuteReaderCommand(sReaderId, sCommand, iWaitExecutionSeconds));
        }

        public List<VRHReaderFrameworkCommon.clsReadResult> GetResults(string sReaderId , ref string sError)
        {
            return clsWCFStatic.GetResults(sReaderId , ref sError);
        }

        public List<VRHReaderFrameworkCommon.clsReadResult_RID_TAGID_COUNT_RSSI> GetResults_RID_TAGID_COUNT_RSSI(string sReaderId, ref string sError)
        {
            return clsWCFStatic.GetResults_RID_TAGID_COUNT_RSSI(sReaderId, ref sError);
        }

        public List<string> GetReaders()
        {
            return clsWCFStatic.GetReaders();
        }

        public void SetResultRequestType(string sReaderId, VRHReaderFrameworkCommon.eControllerResultRequestType eResultRequestType , ref string sError)
        {
            clsWCFStatic.SetReaderResultRequestType(sReaderId , eResultRequestType , ref sError);
        }

        public void SetCycle(string sReaderId, int iCycle , ref string sError)
        {
            clsWCFStatic.SetReaderCycle(sReaderId, iCycle , ref sError);
        }

        public VRHReaderFrameworkCommon.eControllerResultRequestType GetResultRequestType(string sReaderId , ref string sError)
        {
            return clsWCFStatic.GetReaderResultRequestType(sReaderId , ref sError);
        }

        public int GetCycle(string sReaderId , ref string sError)
        {
            return clsWCFStatic.GetReaderCycle(sReaderId , ref sError);
        }

        public void SetTimeoutMode(string sReaderId, int iTimeoutMode , ref string sError)
        {
            clsWCFStatic.SetReaderTimeoutMode(sReaderId, iTimeoutMode , ref sError);
        }

        public int GetTimeoutMode(string sReaderId ,  ref string sError)
        {
            return clsWCFStatic.GetReaderTimeoutMode(sReaderId , ref sError);
        }

        public string READGPI_IF2(string sReaderId, int iWaitExecutionSeconds, ref bool bInputA, ref bool bInputB, ref bool bInputC, ref bool bInputD)
        {
            return clsWCFStatic.READGPI_IF2(sReaderId, iWaitExecutionSeconds, ref bInputA, ref bInputB, ref bInputC, ref bInputD);
        }
    }
}
