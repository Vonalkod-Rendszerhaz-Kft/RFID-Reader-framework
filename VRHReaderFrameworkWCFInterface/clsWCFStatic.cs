using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkWCFInterface
{
    
    public class clsWCFStatic
    {
        #region "ActionLogic"
        internal class clsReaderCommand
        {
            public string command;
            public int delaysec;
        }

        private static void CreateActionReeaderGenericCommands(string sReaderId, System.Collections.Generic.List<clsReaderCommand> colCommands)
        {
            lock (lockObject)
            {
                try
                {
                    foreach (clsReaderCommand oCommand in colCommands)
                    {
                        VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                        oAction.dtAction = DateTime.Now;
                        oAction.dtValidAfter = DateTime.Now.AddSeconds(oCommand.delaysec);
                        oAction.iAction = 1;
                        oAction.uidReader = Guid.Empty;
                        oAction.uidAction = Guid.Empty;
                        oAction.uidProcessor = Guid.Empty;
                        oAction.sTargetReaderId = sReaderId;
                        oAction.colActionParameters = new List<string>();
                        oAction.colActionParameters.Add(oCommand.command);
                        colActions.Add(oAction);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        private static List<clsReaderCommand> LoadCommands(string sCommandFile , string sCommandSet , ref string sRet)
        {
            List<clsReaderCommand> colRet = new List<clsReaderCommand>();

            try
            {
                System.Xml.XmlDocument oXmlDoc = new System.Xml.XmlDocument();
                try
                {
                    oXmlDoc.Load(_basedir + sCommandFile);
                }
                catch (Exception e)
                {
                    using (System.IO.StreamReader oReader = new System.IO.StreamReader(_basedir + sCommandFile, System.Text.Encoding.GetEncoding(1250)))
                    {
                        oXmlDoc.Load(oReader);
                    }
                }

                foreach (System.Xml.XmlNode node in oXmlDoc.DocumentElement.ChildNodes)
                {
                    if (string.Compare(node.Name, sCommandSet, true) == 0)
                    {
                        clsReaderCommand oCommand = new clsReaderCommand();

                        foreach (System.Xml.XmlNode subnode in node.ChildNodes)
                        {
                            if (string.Compare(subnode.Name, "command", true) == 0)
                            {
                                oCommand.command = subnode.InnerText;
                            }
                            else if (string.Compare(subnode.Name, "delaysec", true) == 0)
                            {
                                oCommand.delaysec = int.Parse(subnode.InnerText);
                            }
                        }

                        if (!string.IsNullOrEmpty(oCommand.command))
                            colRet.Add(oCommand);
                    }
                }
                oXmlDoc = null;

            }
            catch(Exception e)
            {
                sRet = "ERR#CONFIG_FILE";
            }

            return colRet;
        }

        public static string ExecuteReaderCommands(string sReaderId, string sConfigFile, string sCommandSet, int iWaitExecutionSeconds)
        {
            if (IsReaderIdValid(sReaderId) == false)
                return "ERR#READERID";

            string sRet = "";

            lock (lockObject)
            {
                try
                {
                    List<clsReaderCommand> colCommands = LoadCommands(sConfigFile, sCommandSet, ref sRet);

                    if (string.IsNullOrEmpty(sRet))
                    { //Ha a config betöltése sikeres volt
                        CreateActionReeaderGenericCommands(sReaderId, colCommands);
                    }
                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }
            }

            return sRet;

        }

        public static string ExecuteReaderCommand(string sReaderId, string sCommand, int iWaitExecutionSeconds)
        {
            if (IsReaderIdValid(sReaderId) == false)
                return "ERR#READERID";

            string sRet = "";

            lock (lockObject)
            {
                try
                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(0);
                    oAction.iAction = 1;
                    oAction.uidReader = Guid.Empty;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.sTargetReaderId = sReaderId;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add(sCommand);
                    colActions.Add(oAction);
                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }
            }

            return sRet;

        }

        public static string READGPI_IF2(string sReaderId, int iWaitExecutionSeconds , ref bool bInputA , ref bool bInputB , ref bool bInputC , ref bool bInputD)
        {
            bInputA = false;
            bInputB = false;
            bInputC = false;
            bInputD = false;

            if (IsReaderIdValid(sReaderId) == false)
                return "ERR#READERID";

            string sRet = "";

            VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
            oAction.dtAction = DateTime.Now;
            oAction.dtValidAfter = DateTime.Now.AddSeconds(0);
            oAction.iAction = 1;
            oAction.uidReader = Guid.Empty;
            oAction.uidAction = Guid.NewGuid();
            oAction.uidProcessor = Guid.Empty;
            oAction.sTargetReaderId = sReaderId;
            oAction.colActionParameters = new List<string>();
            oAction.colActionParameters.Add("READGPI");

            lock (lockObject)
            {
                try
                {
                    colActions.Add(oAction);

                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }
            }

            if (string.IsNullOrEmpty(sRet))
            {
                VRHReaderFrameworkCommon.clsReadResult oResult = PeekActionResult(oAction.uidAction, iWaitExecutionSeconds);

                if (oResult == null)
                {
                    sRet = "ERR#TIMEOUT";
                }
                else
                {
                    try
                    {
                        int iResult = 0;
                        if (int.TryParse(oResult.sResult,out iResult))
                        {
                            if ((iResult & 1) == 1)
                                bInputA = true;

                            if ((iResult & 2) == 2)
                                bInputB = true;

                            if ((iResult & 4) == 4)
                                bInputC = true;

                            if ((iResult & 8) == 8)
                                bInputD = true;
                        }
                        else
                        {
                            sRet = "ERR#PARSERESULTINT";
                        }
                    }
                    catch (Exception e)
                    {
                        sRet = "ERR#EXCEPTION";
                    }
                }
            }

            return sRet;

        }

        public static string StartTimedoutRead(string sReaderId, int iTimeoutSec, int iWaitExecutionSeconds)
        {
            if (IsReaderIdValid(sReaderId) == false)
                return "ERR#READERID";

            string sRet = "";

            lock (lockObject)
            {
                try
                {
                    if (GetReaderTimeoutMode(sReaderId) == 1)
                    {
                        if (GetReaderAntennaOn(sReaderId) == 1)
                        { //Ha már be van kapcsolva az antenna
                            SetReaderAntennaStateChangeDate(sReaderId, DateTime.Now);
                            SetReaderAntennaTimeoutSec(sReaderId, iTimeoutSec);
                        }
                        else
                        { //Ha nincsen bekapcsolva az antenna
                            CreateActionReaderAntennaOn(sReaderId);
                            SetReaderAntennaTimeoutSec(sReaderId, iTimeoutSec);
                        }
                    }
                    else
                    {
                        sRet = "ERR#SET_TIMEOUT_MODE";
                    }
                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }
            }

            return sRet;
        }

        private static void CreateActionReaderAntennaOn(string sReaderId)
        {
            lock(lockObject)
            {
                try
                {
                    {
                        VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                        oAction.dtAction = DateTime.Now;
                        oAction.dtValidAfter = DateTime.Now;
                        oAction.iAction = 3;
                        oAction.uidReader = Guid.Empty;
                        oAction.uidAction = Guid.Empty;
                        oAction.uidProcessor = Guid.Empty;
                        oAction.colActionParameters = new List<string>();
                        oAction.colActionParameters.Add("");
                        oAction.sTargetReaderId = sReaderId;
                        colActions.Add(oAction);
                    }
                    SetReaderAntennaOn(sReaderId, 1);
                    SetReaderAntennaStateChangeDate(sReaderId, DateTime.Now);
                }
                catch(Exception e)
                {

                }
            }
        }

        private static void CreateActionReaderAntennaOff(string sReaderId)
        {
            lock (lockObject)
            {
                try
                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now;
                    oAction.iAction = 4;
                    oAction.uidReader = Guid.Empty;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("");
                    oAction.sTargetReaderId = sReaderId;
                    colActions.Add(oAction);

                    SetReaderAntennaOn(sReaderId, 0);
                    SetReaderAntennaStateChangeDate(sReaderId, DateTime.Now);
                }
                catch (Exception e)
                {

                }
            }
        }
        #endregion

        #region "ReaderStaus"
        public class clsReaderStatus
        {
            public string sReaderId;
            public int iCycle = -1;
            public int iControllerStartCounter = 0;
            public int iControllerStopCounter = 0;
            public int iTimeoutMode = 1;
            public int iAntennaOn = 1;
            public DateTime dtAntennaStateChange = DateTime.MinValue;
            public int iAntennaTimeoutSec = 10;
        }

        private static List<clsReaderStatus> colReaderStatus = new List<clsReaderStatus>();

        public static int GetReaderAntennaTimeoutSec(string sReaderId)
        {
            int iRet = 1;

            lock (lockObject)
            {
                try
                {
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            iRet = oStatus.iAntennaTimeoutSec;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }

            return iRet;
        }

        public static void SetReaderAntennaTimeoutSec(string sReaderId, int iAntennaTimeoutSec)
        {
            lock (lockObject)
            {
                try
                {
                    bool bDone = false;
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            oStatus.iAntennaTimeoutSec = iAntennaTimeoutSec;
                            bDone = true;
                            break;
                        }
                    }

                    if (bDone == false)
                    {
                        clsReaderStatus oStatus = new clsReaderStatus();
                        oStatus.iAntennaTimeoutSec = iAntennaTimeoutSec;
                        oStatus.sReaderId = sReaderId;
                        colReaderStatus.Add(oStatus);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }


        public static DateTime GetReaderAntennaStateChangeDate(string sReaderId)
        {
            DateTime dtRet = DateTime.MinValue;

            lock (lockObject)
            {
                try
                {
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            dtRet = oStatus.dtAntennaStateChange;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }

            return dtRet;
        }

        public static void SetReaderAntennaStateChangeDate(string sReaderId, DateTime dtChangeDate)
        {
            lock (lockObject)
            {
                try
                {
                    bool bDone = false;
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            oStatus.dtAntennaStateChange = dtChangeDate;
                            bDone = true;
                            break;
                        }
                    }

                    if (bDone == false)
                    {
                        clsReaderStatus oStatus = new clsReaderStatus();
                        oStatus.dtAntennaStateChange = dtChangeDate;
                        oStatus.sReaderId = sReaderId;
                        colReaderStatus.Add(oStatus);
                    }
                }
                catch (Exception e)
                {
                }

            }
        }

        public static int GetReaderAntennaOn(string sReaderId)
        {
            int iRet = 1;

            lock (lockObject)
            {
                try
                {
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            iRet = oStatus.iAntennaOn;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                }

            }

            return iRet;
        }

        public static void SetReaderAntennaOn(string sReaderId, int iAntennaOn)
        {
            lock (lockObject)
            {
                try
                {
                    bool bDone = false;
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            oStatus.iAntennaOn = iAntennaOn;
                            bDone = true;
                            break;
                        }
                    }

                    if (bDone == false)
                    {
                        clsReaderStatus oStatus = new clsReaderStatus();
                        oStatus.iAntennaOn = iAntennaOn;
                        oStatus.sReaderId = sReaderId;
                        colReaderStatus.Add(oStatus);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        public static int GetReaderTimeoutMode(string sReaderId)
        {
            string sRet = "";
            return GetReaderTimeoutMode(sReaderId, ref sRet);
        }

        public static int GetReaderTimeoutMode(string sReaderId , ref string sRet)
        {
            if (IsReaderIdValid(sReaderId) == false)
            {
                sRet = "ERR#READERID";
                return 0;
            }

            sRet = "";

            int iRet = 0;

            lock (lockObject)
            {
                try
                {
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            iRet = oStatus.iTimeoutMode;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }
            }

            return iRet;
        }

        public static void SetReaderTimeoutMode(string sReaderId, int iTimeoutMode , ref string sRet)
        {
            if (IsReaderIdValid(sReaderId) == false)
            {
                sRet = "ERR#READERID";
                return;
            }

            sRet = "";

            lock (lockObject)
            {
                try
                {
                    bool bDone = false;
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            oStatus.iTimeoutMode = iTimeoutMode;
                            bDone = true;
                            break;
                        }
                    }

                    if (bDone == false)
                    {
                        clsReaderStatus oStatus = new clsReaderStatus();
                        oStatus.iTimeoutMode = iTimeoutMode;
                        oStatus.sReaderId = sReaderId;
                        colReaderStatus.Add(oStatus);
                    }
                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }

            }
        }

        public static int GetReaderCycle(string sReaderId)
        {
            string sRet = "";
            return GetReaderCycle(sReaderId, ref sRet);
        }

        public static int GetReaderCycle(string sReaderId , ref string sRet)
        {
            if (IsReaderIdValid(sReaderId) == false)
            {
                sRet = "ERR#READERID";
                return 0;
            }

            sRet = "";

            int iRet = 0;

            lock (lockObject)
            {
                try
                {
                    foreach(clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            iRet = oStatus.iCycle;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }
            }

            return iRet;
        }

        public static void SetReaderCycle(string sReaderId , int iCycle , ref string sRet)
        {
            if (IsReaderIdValid(sReaderId) == false)
            {
                sRet = "ERR#READERID";
                return;
            }

            sRet = "";

            lock (lockObject)
            {
                try
                {
                    bool bDone = false;
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            oStatus.iCycle = iCycle;
                            bDone = true;
                            break;
                        }
                    }

                    if (bDone == false)
                    {
                        clsReaderStatus oStatus = new clsReaderStatus();
                        oStatus.iCycle = iCycle;
                        oStatus.sReaderId = sReaderId;
                        colReaderStatus.Add(oStatus);
                    }
                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }

            }
        }

        public static void ReaderCycleDone(string sReaderId)
        {
            lock (lockObject)
            {
                try
                {
                    bool bDone = false;
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            if (oStatus.iCycle > 0)
                                oStatus.iCycle = oStatus.iCycle - 1;
                            bDone = true;
                            break;
                        }
                    }

                    if (bDone == false)
                    {
                        clsReaderStatus oStatus = new clsReaderStatus();
                        oStatus.iCycle = 0;
                        oStatus.sReaderId = sReaderId;
                        colReaderStatus.Add(oStatus);
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        public static void IncreaseReaderControllerStopCounter(string sReaderId)
        {
            lock (lockObject)
            {
                try
                {
                    bool bDone = false;
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            if (oStatus.iControllerStopCounter < oStatus.iControllerStartCounter)
                                oStatus.iControllerStopCounter = oStatus.iControllerStopCounter + 1;

                            bDone = true;
                            break;
                        }
                    }

                    if (bDone == false)
                    {
                        clsReaderStatus oStatus = new clsReaderStatus();
                        oStatus.sReaderId = sReaderId;
                        colReaderStatus.Add(oStatus);
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        public static void IncreaseReaderControllerStartCounter(string sReaderId)
        {
            lock (lockObject)
            {
                try
                {
                    bool bDone = false;
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        if (oStatus.sReaderId == sReaderId)
                        {
                            oStatus.iControllerStartCounter = oStatus.iControllerStartCounter + 1;
                            bDone = true;
                            break;
                        }
                    }

                    if (bDone == false)
                    {
                        clsReaderStatus oStatus = new clsReaderStatus();
                        oStatus.iControllerStartCounter = 1;
                        oStatus.sReaderId = sReaderId;
                        colReaderStatus.Add(oStatus);
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        #endregion

        #region "ReaderResultRequestType"

        public class clsReaderControllerResultRequestType
        {
            public string sReaderId = "";
            public VRHReaderFrameworkCommon.eControllerResultRequestType eResultRequestType = VRHReaderFrameworkCommon.eControllerResultRequestType.NoResult;
        }

        private static List<clsReaderControllerResultRequestType> colReaderResultRequestTypes = new List<clsReaderControllerResultRequestType>();

        public static VRHReaderFrameworkCommon.eControllerResultRequestType GetReaderResultRequestType(string sReaderId , ref string sRet)
        {
            if (IsReaderIdValid(sReaderId) == false)
            {
                sRet = "ERR#READERID";
                return VRHReaderFrameworkCommon.eControllerResultRequestType.NoResult;
            }

            sRet = "";

            VRHReaderFrameworkCommon.eControllerResultRequestType eRet = VRHReaderFrameworkCommon.eControllerResultRequestType.NoResult;

            lock (lockObject)
            {
                try
                {
                    foreach (clsReaderControllerResultRequestType oResutlRequestType in colReaderResultRequestTypes)
                    {
                        if (oResutlRequestType.sReaderId == sReaderId)
                        {
                            eRet = oResutlRequestType.eResultRequestType;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }
            }

            return eRet;
        }

        public static void SetReaderResultRequestType(string sReaderId , VRHReaderFrameworkCommon.eControllerResultRequestType eResultRequestType , ref string sRet)
        {
            if (IsReaderIdValid(sReaderId) == false)
            {
                sRet = "ERR#READERID";
                return;
            }

            sRet = "";

            lock (lockObject)
            {
                try
                {
                    bool bDone = false;
                    foreach (clsReaderControllerResultRequestType oResultRequestType in colReaderResultRequestTypes)
                    {
                        if (oResultRequestType.sReaderId == sReaderId)
                        {
                            oResultRequestType.eResultRequestType = eResultRequestType;
                            bDone = true;
                            break;
                        }
                    }

                    if (bDone == false)
                    {
                        clsReaderControllerResultRequestType oResultRequestType = new clsReaderControllerResultRequestType();
                        oResultRequestType.sReaderId = sReaderId;
                        oResultRequestType.eResultRequestType = eResultRequestType;
                        colReaderResultRequestTypes.Add(oResultRequestType);
                    }
                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }

            }
        }

        #endregion

        private static Object lockObject = new Object();
        private static List<VRHReaderFrameworkCommon.clsAction> colActions = new List<VRHReaderFrameworkCommon.clsAction>();
        private static List<VRHReaderFrameworkCommon.clsReadResult> colResults = new List<VRHReaderFrameworkCommon.clsReadResult>();
        private static List<VRHReaderFrameworkCommon.clsReadResult> colActionResults = new List<VRHReaderFrameworkCommon.clsReadResult>();
        private static string _basedir;

        public static bool IsReaderIdValid(string sReaderId)
        {
            bool bRet = false;

            List<string> colReaders = GetReaders();

            if (colReaders != null)
            {
                bRet = colReaders.Contains(sReaderId);
            }

            return bRet;
        }

        public static List<string> GetReaders()
        {
            List<string> colRet = new List<string>();

            lock (lockObject)
            {
                try
                {
                    foreach (clsReaderStatus oStatus in colReaderStatus)
                    {
                        colRet.Add(oStatus.sReaderId);
                    }


                }
                catch (Exception e)
                {

                }

            }

            return colRet;
        }

        private static string GetSubResultValue(VRHReaderFrameworkCommon.clsReadResult oReadResult, string item)
        {
            string subresultvalue = "";

            if (oReadResult.colSubResults != null)
            {
                foreach (VRHReaderFrameworkCommon.clsReadSubResult oSubResult in oReadResult.colSubResults)
                {
                    if (string.Compare(oSubResult.name, item, true) == 0)
                    {
                        subresultvalue = oSubResult.value;
                        break;
                    }

                }
            }

            return subresultvalue;
        }

        public static List<VRHReaderFrameworkCommon.clsReadResult_RID_TAGID_COUNT_RSSI> GetResults_RID_TAGID_COUNT_RSSI(string sReaderId, ref string sRet)
        {
            List<VRHReaderFrameworkCommon.clsReadResult> colResults = GetResults(sReaderId, ref sRet);

            List<VRHReaderFrameworkCommon.clsReadResult_RID_TAGID_COUNT_RSSI> colRet = new List<VRHReaderFrameworkCommon.clsReadResult_RID_TAGID_COUNT_RSSI>();

            if (colResults != null)
            {
                foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colResults)
                {
                    if (oResult != null)
                    {
                        VRHReaderFrameworkCommon.clsReadResult_RID_TAGID_COUNT_RSSI oRet = new VRHReaderFrameworkCommon.clsReadResult_RID_TAGID_COUNT_RSSI();
                        oRet.eResultType = oResult.eResultType;
                        oRet.sReaderId = oResult.oReaderThreadConfig.id;
                        oRet.sTagId = oResult.sResult;

                        int readedCount = 0;
                        double rssi = 0;

                        int.TryParse(GetSubResultValue(oResult, "count"), out readedCount);
                        double.TryParse(GetSubResultValue(oResult, "rssi").Replace(",", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator).Replace(".", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator), out rssi);

                        oRet.iCount = readedCount;
                        oRet.dRssi = rssi;

                        colRet.Add(oRet);
                    }
                }
            }

            return colRet;
        }

        public static List<VRHReaderFrameworkCommon.clsReadResult> GetResults(string sReaderId , ref string sRet)
        {
            List<VRHReaderFrameworkCommon.clsReadResult> colRet = new List<VRHReaderFrameworkCommon.clsReadResult>();

            if (sReaderId != "#MIND#")
            {
                if (IsReaderIdValid(sReaderId) == false)
                {
                    sRet = "ERR#READERID";
                    return colRet;
                }
            }

            sRet = "";

            lock (lockObject)
            {
                try
                {
                    int iMaxCount = 32;
                    int iCount = 0;
                    foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colResults)
                    {
                        if (sReaderId == "#MIND#" || oResult.oReaderThreadConfig.id == sReaderId)
                        {
                            colRet.Add(oResult);
                            iCount++;
                            if (iCount >= iMaxCount)
                                break;
                        }
                    }

                    foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colRet)
                    {
                        colResults.Remove(oResult);
                    }

                }
                catch (Exception e)
                {
                    sRet = "ERR#EXCEPTION";
                }

            }

            return colRet;
        }


        public static int GetCycle(VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            int iRet = 0;

            iRet = GetReaderCycle(oRederThreadConfig.id);

            return iRet;
        }

        public static void CycleDone(VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            ReaderCycleDone(oRederThreadConfig.id);

            lock (lockObject)
            {
                try
                {
                    if (GetReaderTimeoutMode(oRederThreadConfig.id) == 1)
                    { //Timeout mód
                        if (GetReaderAntennaOn(oRederThreadConfig.id) == 1)
                        { //Ha az antena be van kapcsolva
                            DateTime dtLastAntennaChangeDate = GetReaderAntennaStateChangeDate(oRederThreadConfig.id);
                            int iAntennaTimeoutSec = GetReaderAntennaTimeoutSec(oRederThreadConfig.id);
                            TimeSpan tsAntennaTimeout = new TimeSpan(0, 0, iAntennaTimeoutSec);

                            if ((DateTime.Now - dtLastAntennaChangeDate) > tsAntennaTimeout)
                            { //És eltelt a timeout, akkor kikapcsoljuk az antennát.
                                CreateActionReaderAntennaOff(oRederThreadConfig.id);
                            }

                        }
                    }
                    else
                    { //Normál mód
                        if (GetReaderAntennaOn(oRederThreadConfig.id) == 0)
                        { //Normál módban ha az antenna ki van kapcsolva, akkor be kell kapcsolni az antennát
                            CreateActionReaderAntennaOn(oRederThreadConfig.id);
                        }
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        public static VRHReaderFrameworkCommon.clsReadResult PeekActionResult(Guid uidAction , int iWaitExecutionSeconds)
        {
            VRHReaderFrameworkCommon.clsReadResult oRet = null;

            DateTime tStart = DateTime.Now;

            while(tStart.AddSeconds(iWaitExecutionSeconds) > DateTime.Now)
            {
                oRet = GetActionResult(uidAction);

                if (oRet != null)
                    break;

                System.Threading.Thread.Sleep(50);
            }

            return oRet;
        }

        public static VRHReaderFrameworkCommon.clsReadResult GetActionResult(Guid uidAction)
        {
            VRHReaderFrameworkCommon.clsReadResult oRet = null;

            lock (lockObject)
            {
                try
                {
                    List<VRHReaderFrameworkCommon.clsReadResult> colRemove = new List<VRHReaderFrameworkCommon.clsReadResult>();

                    foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colActionResults)
                    {
                        if (oResult.dtRead.AddHours(1) < DateTime.Now)
                            colRemove.Add(oResult);

                        if (oResult.uidAction == uidAction)
                        {
                            if (oRet == null)
                            {
                                oRet = oResult;
                            }
                        }
                    }

                    foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colRemove)
                    {
                        colActionResults.Remove(oResult);
                    }
                }
                catch(Exception e)
                {

                }
            }

            return oRet;
        }

        public static void SetResults(List<VRHReaderFrameworkCommon.clsReadResult> colReadResults, VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            lock (lockObject)
            {
                try
                {
                    colResults.AddRange(colReadResults);

                    foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colReadResults)
                    {
                        if (oResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.ActionResult)
                            colActionResults.Add(oResult);
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        public static VRHReaderFrameworkCommon.eControllerResultRequestType GetResultRequestType(VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            VRHReaderFrameworkCommon.eControllerResultRequestType eRet = VRHReaderFrameworkCommon.eControllerResultRequestType.NoResult;
            string sRet = "";

            eRet = GetReaderResultRequestType(oRederThreadConfig.id, ref sRet);

            return eRet;
        }

        public static void StartController(VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            IncreaseReaderControllerStartCounter(oRederThreadConfig.id);
        }

        public static void StopController(VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            IncreaseReaderControllerStopCounter(oRederThreadConfig.id);
        }

        public static List<VRHReaderFrameworkCommon.clsAction> GetControllerActions(VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();
            lock (lockObject)
            {
                try
                {
                    foreach(VRHReaderFrameworkCommon.clsAction oAction in colActions)
                    {
                        if (oAction.sTargetReaderId == oRederThreadConfig.id)
                        {
                            colRet.Add(oAction);
                        }
                    }

                    foreach(VRHReaderFrameworkCommon.clsAction oAction in colRet)
                    {
                        colActions.Remove(oAction);
                    }
                }
                catch (Exception e)
                {

                }

            }

            return colRet;
        }

        public static void LoadConfig(string name, string basedir, VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            lock (lockObject)
            {
                try
                {
                    _basedir = basedir;
                }
                catch (Exception e)
                {

                }

            }
        }
    }
}
