using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Symbol.RFID3;

namespace VRHReaderFrameworkZebraFX9600Api
{
    public class clsReader : VRHReaderFrameworkCommon.clsReaderBase
    {
        class ResultDictionaryElement
        {
            public VRHReaderFrameworkCommon.clsReadResult oReadReasult = null;
            public int count = 0;
        }

        RFIDReader m_ReaderAPI;
        private clsReaderConfig oReaderConfig;
        private string sReaderID = "";

        private object oStoredReadResultsLock = new object();
        private System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colStoredReadResults = new List<VRHReaderFrameworkCommon.clsReadResult>();
        private System.Collections.Generic.Dictionary<string, ResultDictionaryElement> dictResult = new System.Collections.Generic.Dictionary<string, ResultDictionaryElement>();


        private void LogToResults(string sRes, List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = null)
        {
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

            lock (oStoredReadResultsLock)
            {
                colStoredReadResults.Add(oResult);
            }
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
                colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", "m_ReaderAPI.Actions.Inventory.Perform()"));
                LogToResults("ACTION:READSTART", colSubRes);
            }

            try
            {
                if (oReaderConfig.resultmode == 1 || oReaderConfig.resultmode == 2)
                {
                    lock (oStoredReadResultsLock)
                    {
                        dictResult.Clear();
                    }
                }

                m_ReaderAPI.Actions.Inventory.Perform(null, null, null);
            }
            catch (Exception e)
            {
                { //Logging
                    List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", "m_ReaderAPI.Actions.Inventory.Perform()"));
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("result", e.Message));
                    LogToResults("READER:EXCEPTION", colSubRes);
                }
            }

            return null;
        }

        private VRHReaderFrameworkCommon.clsReadResult ProcessAction_4(VRHReaderFrameworkCommon.clsAction oAction)
        {
            { //Logging
                List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", "m_ReaderAPI.Actions.Inventory.Stop()"));
                LogToResults("ACTION:READSTOP", colSubRes);
            }

            try
            {
                m_ReaderAPI.Actions.Inventory.Stop();
            }
            catch(Exception e)
            {
                { //Logging
                    List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", "m_ReaderAPI.Actions.Inventory.Stop()"));
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("result", e.Message));
                    LogToResults("READER:EXCEPTION", colSubRes);
                }
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
                        try
                        {
                            throw new NotImplementedException("A funkcionalitás nincsen megvalósítva!");
                        }
                        catch (Exception e)
                        {
                            { //Logging
                                List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                                colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", sCommand));
                                colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("result", e.Message));
                                LogToResults("READER:EXCEPTION", colSubRes);
                            }
                        }
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
            if (!m_ReaderAPI.IsConnected)
            {
                LogToResults("DISCONNECTEDREADER");
                throw new Exception("Reader disconnected...");
            }
        }

        override public void ReadStart()
        {
            { //Logging
                LogToResults("READSTART");
            }

            try
            {
                if (oReaderConfig.resultmode == 1 || oReaderConfig.resultmode == 2)
                {
                    lock (oStoredReadResultsLock)
                    {
                        dictResult.Clear();
                    }
                }

                m_ReaderAPI.Actions.Inventory.Perform(null, null, null);
            }
            catch (Exception e)
            {
                { //Logging
                    List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", "m_ReaderAPI.Actions.Inventory.Perform()"));
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("result", e.Message));
                    LogToResults("READER:EXCEPTION", colSubRes);
                }
            }
        }

        override public void ReadStop()
        {
            { //Logging
                LogToResults("READSTOP");
            }

            try
            {
                m_ReaderAPI.Actions.Inventory.Stop();
            }
            catch (Exception e)
            {
                { //Logging
                    List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("command", "m_ReaderAPI.Actions.Inventory.Stop()"));
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("result", e.Message));
                    LogToResults("READER:EXCEPTION", colSubRes);
                }
            }
        }

        private DateTime dtLastSerialNumber = DateTime.MinValue;

        override public System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> ReadPoll(bool bRead)
        {
            System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colReadResult = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

            int iRetData = 0;
            int iRetOther = 0;

            lock (oStoredReadResultsLock)
            {
                if (bRead == true)
                {
                    foreach (VRHReaderFrameworkCommon.clsReadResult oStoredReadResult in colStoredReadResults)
                    {
                        if (oStoredReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data || oStoredReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent)
                        {
                            if (oReaderConfig.resultmode == 1 || oReaderConfig.resultmode == 2)
                            {
                                if (dictResult.ContainsKey(oStoredReadResult.sResult))
                                {
                                    ResultDictionaryElement oResDictElement;
                                    if (dictResult.TryGetValue(oStoredReadResult.sResult, out oResDictElement))
                                    {
                                        if (oStoredReadResult.colSubResults != null)
                                        {
                                            foreach (VRHReaderFrameworkCommon.clsReadSubResult oSubResult in oStoredReadResult.colSubResults)
                                            {
                                                if (oSubResult.name.ToLower() == "count")
                                                {
                                                    oSubResult.value = oResDictElement.count.ToString();
                                                    //System.Diagnostics.Debug.WriteLine(oStoredReadResult.sResult + " count updated to: " + oSubResult.value);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            iRetData++;
                        }
                        else
                        {
                            iRetOther++;
                        }

                        colReadResult.Add(oStoredReadResult);
                    }

                    colStoredReadResults.Clear();

                    if (oReaderConfig.resultmode == 1)
                    {
                        dictResult.Clear();
                    }

                }
                else
                {
                    foreach (VRHReaderFrameworkCommon.clsReadResult oStoredReadResult in colStoredReadResults)
                    {
                        if (oStoredReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data || oStoredReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent)
                        {
                        }
                        else
                        {
                            iRetOther++;
                            colReadResult.Add(oStoredReadResult);
                        }
                    }

                    foreach (VRHReaderFrameworkCommon.clsReadResult oReturningReadResult in colReadResult)
                    {
                        colStoredReadResults.Remove(oReturningReadResult);
                    }
                }
            }

            //System.Diagnostics.Debug.WriteLine("ReadPoll returning Data: "+iRetData.ToString()+"  Other: " + iRetOther.ToString());

            if (colReadResult.Count == 0)
            {
                if (!m_ReaderAPI.IsConnected)
                {
                    LogToResults("DISCONNECTEDREADER");
                    throw new Exception("Reader disconnected...");
                }
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

            try
            {
                if (m_ReaderAPI != null)
                {
                    try
                    {
                        m_ReaderAPI.Events.ReadNotify -= Events_ReadNotify;
                        m_ReaderAPI.Events.StatusNotify -= Events_StatusNotify;
                    }
                    catch { }

                    m_ReaderAPI.Disconnect();
                    m_ReaderAPI = null;
                }
            } catch (Exception e) {}

            m_ReaderAPI = new RFIDReader(oReaderConfig.ip, (uint)oReaderConfig.port, (uint)0);
            m_ReaderAPI.Connect();

            m_ReaderAPI.Events.ReadNotify += Events_ReadNotify;
            m_ReaderAPI.Events.StatusNotify += Events_StatusNotify;

            m_ReaderAPI.Events.NotifyGPIEvent = true;
            m_ReaderAPI.Events.NotifyReaderDisconnectEvent = true;
            m_ReaderAPI.Events.NotifyReaderExceptionEvent = true;
            m_ReaderAPI.Events.NotifyInventoryStartEvent = true;
            m_ReaderAPI.Events.NotifyInventoryStopEvent = true;

            m_ReaderAPI.Events.AttachTagDataWithReadEvent = true;

            ReadStop();

            VRHReaderFrameworkCommon.clsLogger.Info("ReaderID: " + sReaderID + " " + "Initialize reader.");
            RunAttribsAndInit();
        }

        void Events_StatusNotify(object sender, Events.StatusEventArgs e)
        {
            if (e.StatusEventData.StatusEventType == Events.STATUS_EVENT_TYPE.INVENTORY_START_EVENT)
            {
                { //Logging
                    LogToResults("READER:INVENTORY_START_EVENT", null);
                }
            }
            else if (e.StatusEventData.StatusEventType == Events.STATUS_EVENT_TYPE.INVENTORY_STOP_EVENT)
            {
                { //Logging
                    LogToResults("READER:INVENTORY_STOP_EVENT", null);
                }
            }
            else if (e.StatusEventData.StatusEventType == Events.STATUS_EVENT_TYPE.READER_EXCEPTION_EVENT)
            {
                { //Logging
                    List<VRHReaderFrameworkCommon.clsReadSubResult> colSubRes = new List<VRHReaderFrameworkCommon.clsReadSubResult>();
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("event", "READER_EXCEPTION_EVENT"));
                    colSubRes.Add(new VRHReaderFrameworkCommon.clsReadSubResult("result", e.StatusEventData.ReaderExceptionEventData.ReaderExceptionEventInfo));
                    LogToResults("READER:EXCEPTION", colSubRes);
                }
            }
            else if (e.StatusEventData.StatusEventType == Events.STATUS_EVENT_TYPE.DISCONNECTION_EVENT)
            {
                { //Logging
                    LogToResults("READER:DISCONNECTION_EVENT", null);
                }
            }
            else if (e.StatusEventData.StatusEventType == Events.STATUS_EVENT_TYPE.GPI_EVENT)
            {
                VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();
                oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.Event;

                oResult.oReaderThreadConfig = oReaderThreadConfig;
                oResult.sResult = string.Format("<GPIEvent><Port>{0}</Port><State>{1}</State></GPIEvent>", e.StatusEventData.GPIEventData.PortNumber.ToString(), e.StatusEventData.GPIEventData.GPIEvent ? "1" : "0");
                oResult.dtRead = DateTime.Now;
                oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
                oResult.uid = Guid.NewGuid();
                oResult.uidProcessor = Guid.Empty;
                oResult.uidAction = Guid.Empty;
                oResult.uidReader = Guid.Empty;

                oResult.sOriginalResult = oResult.sResult;
                oResult.iViewedState = 0;

                lock (oStoredReadResultsLock)
                {
                    colStoredReadResults.Add(oResult);
                }
            }
            else
            {

            }
        }

        void Events_ReadNotify(object sender, Events.ReadEventArgs e)
        {
            if (e.ReadEventData != null)
            {
                Symbol.RFID3.TagData tagData = e.ReadEventData.TagData;
                if (tagData != null)
                {
                    if (tagData.OpCode == ACCESS_OPERATION_CODE.ACCESS_OPERATION_NONE ||
                        (tagData.OpCode == ACCESS_OPERATION_CODE.ACCESS_OPERATION_READ &&
                        tagData.OpStatus == ACCESS_OPERATION_STATUS.ACCESS_SUCCESS))
                    {
                        VRHReaderFrameworkCommon.clsReadResult oResult = new VRHReaderFrameworkCommon.clsReadResult();
                        oResult.eResultType = VRHReaderFrameworkCommon.eReadResultType.Data;
                        oResult.oReaderThreadConfig = oReaderThreadConfig;

                        oResult.sResult = tagData.TagID;

                        foreach (clsReadParameter oReadParameter in oReaderConfig.colReadParameters)
                        {
                            if (oResult.colSubResults == null)
                                oResult.colSubResults = new List<VRHReaderFrameworkCommon.clsReadSubResult>();

                            if (oReadParameter.result == 1)
                            { //Csak azokkal foglalkozunk, amelyek megjelennek az eredményben
                                if (oReadParameter.name.ToLower() == "rssi")
                                {
                                    VRHReaderFrameworkCommon.clsReadSubResult oSubResult = new VRHReaderFrameworkCommon.clsReadSubResult();
                                    oSubResult.name = oReadParameter.name;
                                    oSubResult.value = tagData.PeakRSSI.ToString();
                                    oResult.colSubResults.Add(oSubResult);
                                }
                                else if (oReadParameter.name.ToLower() == "ant")
                                {
                                    VRHReaderFrameworkCommon.clsReadSubResult oSubResult = new VRHReaderFrameworkCommon.clsReadSubResult();
                                    oSubResult.name = oReadParameter.name;
                                    oSubResult.value = tagData.AntennaID.ToString();
                                    oResult.colSubResults.Add(oSubResult);
                                }
                                else if (oReadParameter.name.ToLower() == "count")
                                {
                                    VRHReaderFrameworkCommon.clsReadSubResult oSubResult = new VRHReaderFrameworkCommon.clsReadSubResult();
                                    oSubResult.name = oReadParameter.name;
                                    oSubResult.value = "1";
                                    oResult.colSubResults.Add(oSubResult);
                                }
                            }
                        }

                        oResult.dtRead = DateTime.Now;
                        oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
                        oResult.uid = Guid.NewGuid();
                        oResult.uidProcessor = Guid.Empty;
                        oResult.uidAction = Guid.Empty;
                        oResult.uidReader = Guid.Empty;

                        oResult.sOriginalResult = tagData.TagID;
                        oResult.iViewedState = 0;

                        lock (oStoredReadResultsLock)
                        {
                            if (oReaderConfig.resultmode == 0)
                            {
                                colStoredReadResults.Add(oResult);
                            }
                            else
                            {
                                if (dictResult.ContainsKey(oResult.sResult))
                                {
                                    ResultDictionaryElement oResDictElement;
                                    if (dictResult.TryGetValue(oResult.sResult,out oResDictElement))
                                    {
                                        oResDictElement.count++;
                                    }
                                }
                                else
                                {
                                    dictResult.Add(oResult.sResult, new ResultDictionaryElement() {count = 1 , oReadReasult = oResult });
                                    colStoredReadResults.Add(oResult);
                                }
                            }
                        }
                    }
                }
            }
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
                //WriteNetworkStream(sCommand);
            }
        }

        //20170516
        override public void CloseReader()
        {
            { //Logging
                LogToResults("CLOSEREADER");
            }

            if (m_ReaderAPI != null)
            {
                m_ReaderAPI.Disconnect();
                m_ReaderAPI = null;
            }
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
                    else if (string.Compare(node.Name, "resultmode", true) == 0)
                    {
                        oReaderConfig.resultmode = int.Parse(node.InnerText);
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
