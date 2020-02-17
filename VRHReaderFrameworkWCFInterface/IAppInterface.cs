using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace VRHReaderFrameworkWCFInterface
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IAppInterface" in both code and config file together.
    [ServiceContract]
    public interface IAppInterface
    {
        [OperationContract]
        ResultBase Read(string sReaderId, int iReadSeconds, int iWaitExecutionSeconds);

        [OperationContract]
        ResultBase ExecuteReaderCommands(string sReaderId, string sConfigFile, string sCommandSet, int iWaitExecutionSeconds);

        [OperationContract]
        ResultBase ExecuteReaderCommand(string sReaderId, string sCommand, int iWaitExecutionSeconds);

        [OperationContract]
        List<VRHReaderFrameworkCommon.clsReadResult> GetResults(string sReaderId , ref string sError);

        [OperationContract]
        List<VRHReaderFrameworkCommon.clsReadResult_RID_TAGID_COUNT_RSSI> GetResults_RID_TAGID_COUNT_RSSI(string sReaderId, ref string sError);

        [OperationContract]
        List<string> GetReaders();

        [OperationContract]
        void SetResultRequestType(string sReaderId, VRHReaderFrameworkCommon.eControllerResultRequestType eResultRequestType, ref string sError);

        [OperationContract]
        VRHReaderFrameworkCommon.eControllerResultRequestType GetResultRequestType(string sReaderId, ref string sError);

        [OperationContract]
        void SetCycle(string sReaderId, int iCycle, ref string sError);

        [OperationContract]
        int GetCycle(string sReaderId, ref string sError);

        [OperationContract]
        void SetTimeoutMode(string sReaderId, int iTimeoutMode, ref string sError);

        [OperationContract]
        int GetTimeoutMode(string sReaderId, ref string sError);

        [OperationContract]
        string READGPI_IF2(string sReaderId, int iWaitExecutionSeconds, ref bool bInputA, ref bool bInputB, ref bool bInputC, ref bool bInputD);

    }

    [DataContract]
    public class ResultBase
    {
        /// <summary>
        /// ide kerül be ha van valami hiba
        /// </summary>
        [DataMember]
        public string ErrorMessage { get; set; }

        public ResultBase()
            : this(string.Empty)
        {
        }

        public ResultBase(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

}
