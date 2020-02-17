using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameWorkSick630
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

            if (sInput.Contains("!TAG!"))
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
            else if (sInput.Contains("sSN"))
            {
                VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();

                oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.Event;

                oResult.oReaderThreadConfig = oReaderThreadConfig;

                System.Collections.Generic.List<string> colInputs = sInput.Split(' ').ToList<string>();

                oResult.sResult = colInputs[0];


                int resultIndex = 1;
                foreach (string sEventDetail in colInputs)
                {
                    if (oResult.colSubResults == null)
                        oResult.colSubResults = new List<VRHReaderFrameworkCommon.clsReadSubResult>();

                    if (!string.IsNullOrEmpty(sEventDetail))
                    {
                        VRHReaderFrameworkCommon.clsReadSubResult oSubResult = new VRHReaderFrameworkCommon.clsReadSubResult();
                        oSubResult.name = resultIndex.ToString();
                        oSubResult.value = sEventDetail;
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
        }

        private void LogToResults(string sRes, List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = null)
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

        //20170516
        bool WriteNetworkStream(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;

            bool bRet = false;
            byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes("\x02" + s + "\x03");
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

        //20170516
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

        //20170516
        private string ReadLineInputNetworkStream()
        {
            string sRet = null;

            StringBuilder sB = new StringBuilder("");


            bool bStarted = false;
            try
            {
                if (oStream != null)
                {
                    while (true)
                    {
                        byte[] b = new byte[2];
                        oStream.Read(b, 0, 1);

                        if (b[0] == '\x02')
                        {
                            bStarted = true;
                        }
                        else if (b[0] == '\x03')
                        {
                            sRet = sB.ToString();
                            break;
                        }
                        else
                        {
                            if (bStarted)
                                sB.Append(System.Text.ASCIIEncoding.ASCII.GetString(b, 0, 1));
                        }
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
                VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + sReaderID + " " + e.Message, e);
                throw;
            }

            return sRet;
        }

        private System.Collections.Generic.List<string> ProcessPollInputNetworkStream()
        {
            System.Collections.Generic.List<string> colRet = new System.Collections.Generic.List<string>();

            int pollresultnum = 0;

            //if (!oClient.Client.Connected)
            //{
            //    VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + sReaderID + " " + "Network stream does not exists.");
            //    throw new Exception("Network stream does not exists.");
            //}

            if (oStream != null)
            {
                while (oStream.DataAvailable)
                {
                    string sInput;
                    sInput = ReadLineInputNetworkStream();

                    if (!string.IsNullOrEmpty(sInput))
                    {
                        if (sInput.Contains("!TAG!") || sInput.Contains("sSN"))
                        { // Ez egy tag, vagy egy esemény
                            //Az eredményt feldolgozzuk. Majd a poll elviszi
                            ParseReadResult(sInput);
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

            string sCommand = "sMN MIStartIn";
            if (!string.IsNullOrEmpty(sCommand))
            {
                WriteNetworkStream(sCommand);
                return null;
            }

            return null;
        }

        private VRHReaderFrameworkCommon.clsReadResult ProcessAction_4(VRHReaderFrameworkCommon.clsAction oAction)
        {
            { //Logging
                List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", oReaderConfig.readstopcommand));
                LogToResults("ACTION:READSTOP", colSubRes);
            }

            string sCommand = "sMN MIStopIn";
            if (!string.IsNullOrEmpty(sCommand))
            {
                WriteNetworkStream(sCommand);
                return null;
            }

            return null;
        }

        /// <summary>
        /// Általános végrehajó függvény
        /// </summary>
        /// <param name="oAction"></param>
        /// <returns></returns>
        private VRHReaderFrameworkCommon.clsReadResult ProcessAction_2(VRHReaderFrameworkCommon.clsAction oAction)
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
                        WriteNetworkStream(sCommand);
                        return null;
                    }
                }
            }

            return null;

        }

        override public VRHReaderFrameworkCommon.clsReadResult ProcessAction(VRHReaderFrameworkCommon.clsAction oAction)
        {

            if (oAction.iAction == 2 || oAction.iAction == 1)
            {
                ProcessAction_2(oAction);
            }
            if (oAction.iAction == 3)
            {
                ProcessAction_3(oAction);
            }
            if (oAction.iAction == 4)
            {
                ProcessAction_4(oAction);
            }

            return null;
        }

        override public void Ping()
        {
            /*if (colStoredReadResults == null)
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
            }*/

        }

        override public void ReadStart()
        {
            { //Logging
                LogToResults("READSTART");
            }

            WriteNetworkStream(oReaderConfig.readstartcommand);

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

        private DateTime dtLastSerialNumber = DateTime.MinValue;

        override public System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> ReadPoll(bool bRead)
        {
            if (colStoredReadResults == null)
                colStoredReadResults = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

            System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colReadResult = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

            if ((DateTime.Now - dtLastSerialNumber).TotalSeconds > 10)
            {
                dtLastSerialNumber = DateTime.Now;
                WriteNetworkStream("sRN SerialNumber");
            }

            if (bRead)
                WriteNetworkStream(oReaderConfig.readpollcommand);

            System.Collections.Generic.List<string> colStringResult = null;

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

        //20170516
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
            //ResetReader();

            VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Initialize reader.");
            RunAttribsAndInit();
        }

        private void RunAttribsAndInit()
        {
            foreach (clsInitCommand oInitCommand in oReaderConfig.colInitCommands)
            {
                { //Logging
                    List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", oInitCommand.initcommand));
                    LogToResults("INITCOMMAND", colSubRes);
                }

                VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Executing init command: " + oInitCommand.initcommand);
                string sCommand = oInitCommand.initcommand;
                WriteNetworkStream(sCommand);
            }
        }

        //20170516
        override public void CloseReader()
        {
            { //Logging
                LogToResults("CLOSEREADER");
            }

            oClient.Close();
            oClient = null;
        }

        //20170516
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
                    oDefaultReadParameter.resultonly = 1;
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
                { // Folyamatos
                    oReaderConfig.readstopcommand = "sMN MIStopIn";
                    oReaderConfig.readpollcommand = "";
                    oReaderConfig.readstartcommand = "sMN MIStartIn";
                }
                else /*if (oReaderConfig.readmode == 1)*/
                { // Kívülről vezérelt
                    oReaderConfig.readstopcommand = "";
                    oReaderConfig.readpollcommand = "";
                    oReaderConfig.readstartcommand = "";
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
