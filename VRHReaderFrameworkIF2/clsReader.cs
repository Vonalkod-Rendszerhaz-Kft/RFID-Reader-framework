using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkIF2
{
    public class clsReader : VRHReaderFrameworkCommon.clsReaderBase 
    {
        private System.Net.Sockets.TcpClient oClient;
        private System.Net.Sockets.NetworkStream oStream;
        private clsReaderConfig oReaderConfig;
        private System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colStoredReadResults;
        private int iTimeoutMiliSec = 60000;
        private string sReaderID = "";

        void ParseReadResult(string sInput)
        {
            bool bDataEvent = false;

            string sOriginalInput = sInput;

            if (colStoredReadResults == null)
                colStoredReadResults = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

            if (oReaderConfig.processtagevtastagread == 1)
            {
                if (sInput.Contains("!TAG!") && sInput.StartsWith("EVT:TAG ", StringComparison.OrdinalIgnoreCase))
                {
                    if (oReaderConfig.discardevtwithrderr == 1)
                    {
                        if (sInput.Contains("RDERR"))
                        {
                            return;
                        }
                    }
                    sInput = sInput.Replace("EVT:TAG ", "");
                    bDataEvent = true;
                }
            }

            if (sInput.Contains("!TAG!") && !sInput.StartsWith("EVT:", StringComparison.OrdinalIgnoreCase))
            {
                VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();

                if (bDataEvent)
                    oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.DataEvent;
                else
                    oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.Data;

                oResult.oReaderThreadConfig = oReaderThreadConfig;

                System.Collections.Generic.List<string> colInputs = sInput.Split(' ').ToList<string>();

                oResult.sResult = colInputs[0];

                int resultIndex = 1;
                foreach (clsReadParameter oReadParameter in oReaderConfig.colReadParameters)
                {
                    if (oResult.colSubResults == null)
                        oResult.colSubResults = new List<VRHReaderFrameworkCommon.clsReadSubResult>();

                    if (oReadParameter.result == 1)
                    { //Csak azokkal foglalkozunk, amelyek megjelennek az eredményben
                        VRHReaderFrameworkCommon.clsReadSubResult oSubResult = new VRHReaderFrameworkCommon.clsReadSubResult();
                        oSubResult.name = oReadParameter.name;
                        if (colInputs.Count > resultIndex)
                        {
                            oSubResult.value = colInputs[resultIndex];
                        }
                        resultIndex++;
                        oResult.colSubResults.Add(oSubResult);
                    }
                }

                oResult.dtRead = DateTime.Now;
                oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
                oResult.uid = Guid.NewGuid();
                oResult.uidProcessor = Guid.Empty;
                oResult.uidAction = Guid.Empty;
                oResult.uidReader = Guid.Empty;

                oResult.sOriginalResult = sOriginalInput;
                oResult.iViewedState = 0;

                colStoredReadResults.Add(oResult);
            }
            else if (sInput.StartsWith("EVT:RESET",StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Reader reset detected!");
            }
            else if (sInput.StartsWith("EVT:",StringComparison.OrdinalIgnoreCase))
            {
                VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();
                
                oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.Event;

                oResult.oReaderThreadConfig = oReaderThreadConfig;
                oResult.sResult = sInput;
                oResult.dtRead = DateTime.Now;
                oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
                oResult.uid = Guid.NewGuid();
                oResult.uidProcessor = Guid.Empty;
                oResult.uidAction = Guid.Empty;
                oResult.uidReader = Guid.Empty;

                oResult.sOriginalResult = sOriginalInput;
                oResult.iViewedState = 0;

                colStoredReadResults.Add(oResult);
            }


        }

        private void LogToResults(string sRes , List <VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = null)
        {
            if (colStoredReadResults == null)
                colStoredReadResults = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

            VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();

            oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.Log;

            oResult.oReaderThreadConfig = oReaderThreadConfig;
            oResult.sResult = sRes;
            oResult.dtRead = DateTime.Now;
            oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
            oResult.uid = Guid.NewGuid();
            oResult.uidProcessor = Guid.Empty;
            oResult.uidAction = Guid.Empty;
            oResult.uidReader = Guid.Empty;
            oResult.colSubResults = colSubRes;

            colStoredReadResults.Add(oResult);
        }

        bool WriteNetworkStream(string s)
        {
            bool bRet = false;
            byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(s);
            try
            {
                if (oStream != null)
                {
                    oStream.Write(b, 0, b.Length);
                }
                else
                {
                    VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + sReaderID + " " + "Network stream does not exists.");
                    throw new Exception("Network stream does not exists.");
                }
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + sReaderID + " " + e.Message, e);
                throw;
            }

            return bRet;
        }

        private void RunAttribsAndInit()
        {
            //Leállítjuk az olvasást, ha valamiért futna...
            WriteNetworkStream("READ STOP\r\n");
            System.Collections.Generic.List<string> colRet = null;
            colRet = ProcessInputNetworkStream();
            if (colRet.Count != 0)
            { //Valami hiba van

            }

            foreach (clsAttrib oAttrib in oReaderConfig.colAttribs)
            {
                string sCommand = "ATTRIB " + oAttrib.attrib.Split('=')[0] + "\r\n";
                VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Executing attrib query: " + sCommand);
                WriteNetworkStream(sCommand);
                colRet = ProcessInputNetworkStream();
                if (colRet.Count > 0)
                { // Ha van eredmény
                    string sResult = colRet[0];
                    if (!sResult.Contains("ERR"))
                    {
                        if (sResult != oAttrib.attrib)
                        { // Beállítás szükséges
                            if (oAttrib.attrib.Split('=')[0] == "UTCTIME")
                            {
                                string sUTCTime = ((int)(DateTime.Now - new DateTime(1970, 01, 01, 0, 0, 0)).TotalSeconds).ToString();
                                sCommand = "ATTRIB " + "UTCTIME=" + sUTCTime + "\r\n";
                            }
                            else
                            {
                                sCommand = "ATTRIB " + oAttrib.attrib + "\r\n";
                            }

                            { //Logging
                                List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                                colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", sCommand));
                                LogToResults("CONFIGURATION", colSubRes);
                            }

                            VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Executing attrib set: " + sCommand);
                            WriteNetworkStream(sCommand);
                            colRet = ProcessInputNetworkStream();
                            if (colRet.Count != 0)
                            { //Valami hiba van
                                VRHReaderFrameworkCommon.clsLogger.Error("ReaderID: " + sReaderID + " " + "Reader responds an error: " + colRet[0]);
                            }
                        }
                    }
                    else
                    { // Hiba van
                        VRHReaderFrameworkCommon.clsLogger.Error("ReaderID: " + sReaderID + " " + "Attrib query failed.");
                    }

                }
                else
                { //Ha nincsen eredmény
                    VRHReaderFrameworkCommon.clsLogger.Error("ReaderID: " + sReaderID + " " + "No result for attrib query.");
                }
            }

            foreach (clsInitCommand oInitCommand in oReaderConfig.colInitCommands)
            {
                { //Logging
                    List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", oInitCommand.initcommand));
                    LogToResults("INITCOMMAND", colSubRes);
                }

                VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Executing init command: " + oInitCommand.initcommand);
                string sCommand = oInitCommand.initcommand + "\r\n";
                WriteNetworkStream(sCommand);
                colRet = ProcessInputNetworkStream();
                if (colRet.Count != 0)
                { //Valami hiba van
                    VRHReaderFrameworkCommon.clsLogger.Error("ReaderID: " + sReaderID + " " + "Reader responds an error: " + colRet[0]);
                }
            }

        }

        private void ClearInputNetworkStream()
        {
            if (oStream != null)
            {
                while (oStream.DataAvailable)
                {
                    byte[] b = new byte[2];
                    oStream.Read(b, 0, 1);
                }
            }
            else
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + sReaderID + " " + "Network stream does not exists.");
                throw new Exception("Network stream does not exists.");
            }
        }

        private string ReadLineInputNetworkStream()
        {
            string sRet = null;

            StringBuilder sB = new StringBuilder("");

            try
            {
                if (oStream != null)
                {
                    while (true)
                    {
                        byte[] b = new byte[2];
                        oStream.Read(b, 0, 1);
                        sB.Append(System.Text.ASCIIEncoding.ASCII.GetString(b, 0, 1));
                        sRet = sB.ToString();
                        if (sRet.EndsWith("\r\n"))
                            break;
                    }
                }
                else
                {
                    VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + sReaderID + " " + "Network stream does not exists.");
                    throw new Exception("Network stream does not exists.");
                }
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + sReaderID + " " + e.Message,e);
                throw;
            }

            sRet = sRet.Replace("\r\n", "");
            
            return sRet;
        }

        private System.Collections.Generic.List<string> ProcessInputNetworkStream()
        {
            System.Collections.Generic.List<string> colRet = new System.Collections.Generic.List<string>();

            DateTime dtStart = DateTime.Now;

            while (dtStart.AddMilliseconds(iTimeoutMiliSec) > DateTime.Now)
            {
                string sInput;
                sInput = ReadLineInputNetworkStream();
                if (!string.IsNullOrEmpty(sInput))
                {
                    if (string.Compare(sInput, "OK>", true) == 0)
                    { // A válasz megérkezett. OK> csak parancs után jön
                        break;
                    }
                    else
                    { //Ez még nem a válasz
                        if (sInput.Contains("!TAG!") || sInput.StartsWith("EVT:",StringComparison.OrdinalIgnoreCase))
                        { // Ez egy tag, vagy egy esemény
                            //A korábbi esetlegesen az olvasó által küldött cumó nem érdekel minket
                            colRet = new System.Collections.Generic.List<string>();
                            //Az eredményt feldolgozzuk. Majd a poll elviszi
                            ParseReadResult(sInput);
                        }
                        else if (sInput.StartsWith("NOTAG",StringComparison.OrdinalIgnoreCase))
                        { // Ezzel nem csinálunk semmit... nem érdekel minket.

                        }
                        else
                        { // Hozzáadjuk a visszatérési cucchoz
                            colRet.Add(sInput);
                        }
                    }
                }
            }

            return colRet;
        }

        private void WaitForResetEvent()
        {
            System.Collections.Generic.List<string> colRet = new System.Collections.Generic.List<string>();

            DateTime dtStart = DateTime.Now;

            bool bResetDone = false;

            while (dtStart.AddMilliseconds(120000) > DateTime.Now)
            {
                string sInput;
                sInput = ReadLineInputNetworkStream();
                if (!string.IsNullOrEmpty(sInput))
                {
                    if (string.Compare(sInput, "EVT:RESET", true) == 0)
                    { // A válasz megérkezett.
                        bResetDone = true;
                        break;
                    }
                }
            }

            if (bResetDone)
            {
                while (dtStart.AddMilliseconds(120000) > DateTime.Now)
                {
                    string sInput;
                    sInput = ReadLineInputNetworkStream();
                    if (!string.IsNullOrEmpty(sInput))
                    {
                        if (string.Compare(sInput, "OK>", true) == 0)
                        { // A válasz megérkezett.
                            break;
                        }
                    }
                }
            }

            if (!bResetDone)
                throw new Exception("Failed to reset reader...");
        }


        private System.Collections.Generic.List<string> ProcessPollInputNetworkStream()
        {
            System.Collections.Generic.List<string> colRet = new System.Collections.Generic.List<string>();

            int pollresultnum = 0;

            if (oStream != null)
            {
                while (oStream.DataAvailable)
                {
                    string sInput;
                    sInput = ReadLineInputNetworkStream();

                    if (!string.IsNullOrEmpty(sInput))
                    {
                        if (sInput.Contains("!TAG!") || sInput.StartsWith("EVT:", StringComparison.OrdinalIgnoreCase))
                        { // Ez egy tag, vagy egy esemény
                            //Az eredményt feldolgozzuk. Majd a poll elviszi
                            ParseReadResult(sInput);
                        }
                        else if (sInput.StartsWith("NOTAG", StringComparison.OrdinalIgnoreCase))
                        { // Ezzel nem csinálunk semmit... nem érdekel minket.

                        }
                        else
                        { // Hozzáadjuk a visszatérési cucchoz
                            colRet.Add(sInput);
                        }
                    }

                    pollresultnum++;
                    if (pollresultnum >= oReaderConfig.maxpollresults)
                        break;
                }
            }
            else
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + sReaderID + " " + "Network stream does not exists.");
                throw new Exception("Network stream does not exists.");
            }

            return colRet;
        }


        /// <summary>
        /// Általános végrehajó függvény
        /// </summary>
        /// <param name="oAction"></param>
        /// <returns></returns>
        private VRHReaderFrameworkCommon.clsReadResult ProcessAction_1(VRHReaderFrameworkCommon.clsAction oAction)
        {
            if (oAction.colActionParameters != null)
            {
                if (oAction.colActionParameters.Count > 0)
                {
                    { //Logging
                        List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                        colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", oAction.colActionParameters[0]));
                        LogToResults("ACTION:GENERIC", colSubRes);
                    }

                    string sCommand = oAction.colActionParameters[0];
                    if (!string.IsNullOrEmpty(sCommand))
                    {
                        if (!sCommand.EndsWith("\r\n"))
                            sCommand = sCommand + "\r\n";

                        VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();
                        oResult.dtRead = DateTime.Now;
                        oResult.uid = Guid.NewGuid();
                        oResult.uidAction = oAction.uidAction;
                        oResult.uidProcessor = oAction.uidProcessor;
                        oResult.uidReader = oAction.uidReader;
                        oResult.oReaderThreadConfig = oReaderThreadConfig;
                        oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
                        oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.ActionResult;

                        WriteNetworkStream(sCommand);

                        oResult.sResult = "";
                        System.Collections.Generic.List<string> colStringResult = ProcessInputNetworkStream();

                        foreach (string s in colStringResult)
                            oResult.sResult += s + "\n";

                        oResult.sOriginalResult = oResult.sResult;
                        oResult.iViewedState = 0;

                        return oResult;
                    }
                }
            }

            return null;

        }

        /// <summary>
        /// Continious mód bekapcsolása
        /// </summary>
        /// <param name="oAction"></param>
        /// <returns></returns>
        private VRHReaderFrameworkCommon.clsReadResult ProcessAction_3(VRHReaderFrameworkCommon.clsAction oAction)
        {
            { //Logging
                List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", oReaderConfig.readstartcommand));
                LogToResults("ACTION:READSTART", colSubRes);
            }

            VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();
            oResult.dtRead = DateTime.Now;
            oResult.uid = Guid.NewGuid();
            oResult.uidAction = oAction.uidAction;
            oResult.uidProcessor = oAction.uidProcessor;
            oResult.uidReader = oAction.uidReader;
            oResult.oReaderThreadConfig = oReaderThreadConfig;
            oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
            oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.ActionResult;

            WriteNetworkStream(oReaderConfig.readstartcommand);

            oResult.sResult = "";
            System.Collections.Generic.List<string> colStringResult = ProcessInputNetworkStream();

            foreach (string s in colStringResult)
                oResult.sResult += s + "\n";

            oResult.sOriginalResult = oResult.sResult;
            oResult.iViewedState = 0;

            return oResult;
        }

        private VRHReaderFrameworkCommon.clsReadResult ProcessAction_4(VRHReaderFrameworkCommon.clsAction oAction)
        {
            { //Logging
                List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", oReaderConfig.readstopcommand));
                LogToResults("ACTION:READSTOP", colSubRes);
            }
            VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();
            oResult.dtRead = DateTime.Now;
            oResult.uid = Guid.NewGuid();
            oResult.uidAction = oAction.uidAction;
            oResult.uidProcessor = oAction.uidProcessor;
            oResult.uidReader = oAction.uidReader;
            oResult.oReaderThreadConfig = oReaderThreadConfig;
            oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
            oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.ActionResult;

            WriteNetworkStream(oReaderConfig.readstopcommand);

            oResult.sResult = "";
            System.Collections.Generic.List<string> colStringResult = ProcessInputNetworkStream();

            foreach (string s in colStringResult)
                oResult.sResult += s + "\n";

            oResult.sOriginalResult = oResult.sResult;
            oResult.iViewedState = 0;

            return oResult;
        }

        private VRHReaderFrameworkCommon.clsReadResult ProcessAction_5(VRHReaderFrameworkCommon.clsAction oAction)
        {
            { //Logging
                LogToResults("ACTION:RESET");
            }

            VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();
            oResult.dtRead = DateTime.Now;
            oResult.uid = Guid.NewGuid();
            oResult.uidAction = oAction.uidAction;
            oResult.uidProcessor = oAction.uidProcessor;
            oResult.uidReader = oAction.uidReader;
            oResult.oReaderThreadConfig = oReaderThreadConfig;
            oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
            oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.ActionResult;

            WriteNetworkStream("RESET\r\n");

            oResult.sResult = "";
            System.Collections.Generic.List<string> colStringResult = ProcessInputNetworkStream();

            foreach (string s in colStringResult)
                oResult.sResult += s + "\n";

            oResult.sOriginalResult = oResult.sResult;
            oResult.iViewedState = 0;

            return oResult;
        }

        override public VRHReaderFrameworkCommon.clsReadResult ProcessAction(VRHReaderFrameworkCommon.clsAction oAction)
        {
            if (oAction.iAction == 1)
            {
                return ProcessAction_1(oAction);
            }
            else if (oAction.iAction == 2)
            {
                ProcessAction_1(oAction);
                return null;
            }
            else if (oAction.iAction == 3)
            {
                return ProcessAction_3(oAction);
            }
            else if (oAction.iAction == 4)
            {
                return ProcessAction_4(oAction);
            }
            else if (oAction.iAction == 5)
            {
                return ProcessAction_5(oAction);
            }

            return null;
        }

        override public void Ping()
        {
            if (colStoredReadResults == null)
                colStoredReadResults = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

            System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colReadResult = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

            WriteNetworkStream("PING\r\n");

            System.Collections.Generic.List<string> colStringResult = null;

            colStringResult = ProcessInputNetworkStream();
 
            foreach (string line in colStringResult)
            {
                VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();
                oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.Failure;
                oResult.oReaderThreadConfig = oReaderThreadConfig;
                oResult.sResult = line;
                oResult.sOriginalResult = oResult.sResult;
                oResult.iViewedState = 0;
                oResult.dtRead = DateTime.Now;
                oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
                oResult.uid = Guid.NewGuid();
                oResult.uidProcessor = Guid.Empty;
                oResult.uidAction = Guid.Empty;
                oResult.uidReader = Guid.Empty;
                colReadResult.Add(oResult);
            }

        }

        override public void ReadStart()
        {
            { //Logging
                LogToResults("READSTART");
            }

            if (!string.IsNullOrEmpty(oReaderConfig.readstartcommand))
            {
                WriteNetworkStream(oReaderConfig.readstartcommand);

                System.Collections.Generic.List<string> colStringResult = ProcessInputNetworkStream();
            }
        }

        override public void ReadStop()
        {
            { //Logging
                LogToResults("READSTOP");
            }

            WriteNetworkStream(oReaderConfig.readstopcommand);
            System.Threading.Thread.Sleep(1000);
            ClearInputNetworkStream();
        }

        override public System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> ReadPoll(bool bRead)
        {
            if (colStoredReadResults == null)
                colStoredReadResults = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

            System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colReadResult = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

            if (bRead)
                WriteNetworkStream(oReaderConfig.readpollcommand);

            System.Collections.Generic.List<string> colStringResult = null;
            
            if (bRead)
                colStringResult = ProcessInputNetworkStream();
            else
                colStringResult = ProcessPollInputNetworkStream();

            foreach (VRHReaderFrameworkCommon.clsReadResult oStoredReadResult in colStoredReadResults)
            {
                colReadResult.Add(oStoredReadResult);
            }

            colStoredReadResults.Clear();

            foreach (string line in colStringResult)
            {
                VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();
                oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.Failure;
                oResult.oReaderThreadConfig = oReaderThreadConfig;
                oResult.sResult = line;
                oResult.sOriginalResult = oResult.sResult;
                oResult.iViewedState = 0;
                oResult.dtRead = DateTime.Now;
                oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
                oResult.uid = Guid.NewGuid();
                oResult.uidProcessor = Guid.Empty;
                oResult.uidAction = Guid.Empty;
                oResult.uidReader = Guid.Empty;
                colReadResult.Add(oResult);
            }

            return colReadResult;
        }

        private void ResetReader()
        {
            WriteNetworkStream("RESET\r\n");
            WaitForResetEvent();
        }

        override public void OpenReader()
        {
            VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Opening reader.");

            { //Logging
                LogToResults("OPENREADER");
            }

            oClient = new System.Net.Sockets.TcpClient();
            oClient.Connect(oReaderConfig.ip, oReaderConfig.port);
            oStream = oClient.GetStream();
            oStream.WriteTimeout = iTimeoutMiliSec;
            oStream.ReadTimeout = iTimeoutMiliSec;

            { //Logging
                LogToResults("RESETREADER");
            }

            VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Reset reader.");
            ResetReader();

            VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Initialize reader.");
            RunAttribsAndInit();
        }

        override public void CloseReader()
        {
            { //Logging
                LogToResults("CLOSEREADER");
            }

            oClient.Close();
            oClient = null;
        }

        override public void LoadConfig(string name, string id)
        {
            sReaderID = id;

            VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Loading config: " + name);

            oReaderConfig = new clsReaderConfig();

            try
            {
                System.IO.FileInfo oFileInfo = new System.IO.FileInfo(name);
                oReaderConfig.dtLastWriteTime = oFileInfo.LastWriteTime;

                System.Xml.XmlDocument oXmlDoc = new System.Xml.XmlDocument();

                try
                {
                    oXmlDoc.Load(name);
                }
                catch (Exception e)
                {
                    using (System.IO.StreamReader oReader = new System.IO.StreamReader(name, System.Text.Encoding.GetEncoding(1250)))
                    {
                        oXmlDoc.Load(oReader);
                    }
                }

                if (oReaderConfig.colReadParameters == null)
                {
                    oReaderConfig.colReadParameters = new System.Collections.Generic.List<clsReadParameter>();
                    clsReadParameter oDefaultReadParameter = new clsReadParameter();
                    oDefaultReadParameter.name = "default";
                    oDefaultReadParameter.value = "\"!TAG!\"";
                    oDefaultReadParameter.result = 1;
                    oReaderConfig.colReadParameters.Add(oDefaultReadParameter);
                }

                if (oReaderConfig.colAttribs == null)
                {
                    oReaderConfig.colAttribs = new System.Collections.Generic.List<clsAttrib>();
                }

                if (oReaderConfig.colInitCommands == null)
                {
                    oReaderConfig.colInitCommands = new System.Collections.Generic.List<clsInitCommand>();
                }

                foreach (System.Xml.XmlNode node in oXmlDoc.DocumentElement.ChildNodes)
                {
                    if (string.Compare(node.Name, "valid", true) == 0)
                    {
                        oReaderConfig.valid = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "readpolldelay", true) == 0)
                    {
                        oReaderConfig.readpolldelay = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "polldelay", true) == 0)
                    {
                        oReaderConfig.polldelay = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "pingdelay", true) == 0)
                    {
                        oReaderConfig.pingdelay = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "ip", true) == 0)
                    {
                        oReaderConfig.ip = node.InnerText;
                    }
                    else if (string.Compare(node.Name, "port", true) == 0)
                    {
                        oReaderConfig.port = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "readmode", true) == 0)
                    {
                        oReaderConfig.readmode = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "processtagevtastagread", true) == 0)
                    {
                        oReaderConfig.processtagevtastagread = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "discardevtwithrderr", true) == 0)
                    {
                        oReaderConfig.discardevtwithrderr = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "maxpollresults", true) == 0)
                    {
                        oReaderConfig.maxpollresults = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "readparameter", true) == 0)
                    {
                        clsReadParameter oReadParameter = new clsReadParameter();

                        foreach (System.Xml.XmlNode childnode in node.ChildNodes)
                        {
                            if (string.Compare(childnode.Name, "name", true) == 0)
                            {
                                oReadParameter.name = childnode.InnerText;
                            }
                            else if (string.Compare(childnode.Name, "value", true) == 0)
                            {
                                oReadParameter.value = childnode.InnerText;
                            }
                            else if (string.Compare(childnode.Name, "result", true) == 0)
                            {
                                oReadParameter.result = int.Parse(childnode.InnerText);
                            }
                            else if (string.Compare(childnode.Name, "resultonly", true) == 0)
                            {
                                oReadParameter.resultonly = int.Parse(childnode.InnerText);
                            }
                        }

                        oReaderConfig.colReadParameters.Add(oReadParameter);
                    }
                    else if (string.Compare(node.Name, "attrib", true) == 0)
                    {
                        clsAttrib oAttrib = new clsAttrib();
                        oAttrib.attrib = node.InnerText.ToUpper();
                        if (oAttrib.attrib.StartsWith("FIELDSEP"))
                        {
                            oAttrib.attrib = "FIELDSEP=\" \"";
                        }
                        else if (oAttrib.attrib.StartsWith("IDREPORT"))
                        {
                            oAttrib.attrib = "IDREPORT=ON";
                        }
                        else if (oAttrib.attrib.StartsWith("TTY"))
                        {
                            oAttrib.attrib = "TTY=OFF";
                        }
                        else if (oAttrib.attrib.StartsWith("ECHO"))
                        {
                            oAttrib.attrib = "ECHO=OFF";
                        }
                        oReaderConfig.colAttribs.Add(oAttrib);
                    }
                    else if (string.Compare(node.Name, "initcommand", true) == 0)
                    {
                        clsInitCommand oInitCommand = new clsInitCommand();
                        oInitCommand.initcommand = node.InnerText;
                        oReaderConfig.colInitCommands.Add(oInitCommand);
                    }
                }
                oXmlDoc = null;

                if (oReaderConfig.readmode == 0)
                { //Single shot
                    oReaderConfig.readstartcommand = "";
                    oReaderConfig.readstopcommand = "READ STOP\r\n";
                    oReaderConfig.readpollcommand = "READ";
                    foreach (clsReadParameter oReadParameter in oReaderConfig.colReadParameters)
                    {
                        if (oReadParameter.resultonly == 0)
                            oReaderConfig.readpollcommand += " " + oReadParameter.value;
                    }
                    oReaderConfig.readpollcommand += "\r\n";
                }
                else if (oReaderConfig.readmode == 1)
                { //Cont single event
                    oReaderConfig.readstopcommand = "READ STOP\r\n";
                    oReaderConfig.readpollcommand = "READ POLL\r\n";
                    oReaderConfig.readstartcommand = "READ";
                    foreach (clsReadParameter oReadParameter in oReaderConfig.colReadParameters)
                    {
                        if (oReadParameter.resultonly == 0)
                            oReaderConfig.readstartcommand += " " + oReadParameter.value;
                    }
                    oReaderConfig.readstartcommand += " " + "REPORT=EVENT";
                    oReaderConfig.readstartcommand += "\r\n";
                }
                else if (oReaderConfig.readmode == 2)
                { //Cont no event
                    oReaderConfig.readstopcommand = "READ STOP\r\n";
                    oReaderConfig.readpollcommand = "READ POLL\r\n";
                    oReaderConfig.readstartcommand = "READ";
                    foreach (clsReadParameter oReadParameter in oReaderConfig.colReadParameters)
                    {
                        if (oReadParameter.resultonly == 0)
                            oReaderConfig.readstartcommand += " " + oReadParameter.value;
                    }
                    oReaderConfig.readstartcommand += " " + "REPORT=NO";
                    oReaderConfig.readstartcommand += "\r\n";
                }
                else if (oReaderConfig.readmode == 3)
                { //Cont lot of events
                    oReaderConfig.readstopcommand = "READ STOP\r\n";
                    oReaderConfig.readpollcommand = "READ POLL\r\n";
                    oReaderConfig.readstartcommand = "READ";
                    foreach (clsReadParameter oReadParameter in oReaderConfig.colReadParameters)
                    {
                        if (oReadParameter.resultonly == 0)
                            oReaderConfig.readstartcommand += " " + oReadParameter.value;
                    }
                    oReaderConfig.readstartcommand += " " + "REPORT=EVENTALL";
                    oReaderConfig.readstartcommand += "\r\n";
                }

                VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Read start command: " + oReaderConfig.readstartcommand);
                VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Read poll command: " + oReaderConfig.readpollcommand);
                VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Read stop command: " + oReaderConfig.readstopcommand);
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + sReaderID + " " + e.Message, e);
                oReaderConfig = null;
                throw;
            }
        }
    }
}
