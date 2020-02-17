using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkMain
{
    public class clsMainWorker
    {
        private static bool started = false;
        private static string basedir;
        private static System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReaderThreadConfig> colReaderThreadConfigs;
        private static System.Collections.Generic.List<clsLoadedAssembly> colAssemblys;
        private static System.Collections.Generic.List<System.Threading.Thread> colReaderThreads;
        private static System.Collections.Generic.List<System.Threading.Thread> colProcessorThreads;
        private static VRHReaderFrameworkCommon.clsProcessorThreadConfig oProcessorThreadConfig;
        private static System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colGlobalReadResults;
        private static object lockObjectGlobalReadResults;
        private static System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction> colGlobalActions;
        private static object lockObjectGlobalActions;
        private static volatile bool bKeepRunning = true;

        //private static System.ServiceModel.ServiceHost svcHost_Controller = null;
        private static System.ServiceModel.ServiceHost svcHost_Application = null;

        private static void ProcessorThread(object oParam)
        {
            Guid uidProcessor = Guid.NewGuid();

            while (bKeepRunning)
            {
                try
                {
                    System.Collections.Generic.List<VRHReaderFrameworkCommon.clsProcessorBase> colProcessorObjects = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsProcessorBase>();
                    foreach (VRHReaderFrameworkCommon.clsProcessorConfig oProcessorConfig in oProcessorThreadConfig.colProcessorConfigs)
                    {
                        System.Reflection.Assembly oAssembly = GetAssemblyLoaded(oProcessorConfig.assembly);
                        VRHReaderFrameworkCommon.clsProcessorBase oProcessor = (VRHReaderFrameworkCommon.clsProcessorBase)Activator.CreateInstance(oAssembly.GetType(oProcessorConfig.type));
                        oProcessor.LoadConfig(basedir + "\\configs\\" + oProcessorConfig.config);
                        colProcessorObjects.Add(oProcessor);
                    }
                    
                    while (bKeepRunning)
                    {
                        VRHReaderFrameworkCommon.clsReadResult oResult = GetUnprocessedReadResultForProcessing(uidProcessor);
                        if (oResult != null)
                        {
                            VRHReaderFrameworkCommon.clsLogger.Debug("PROCESSING: " + oResult.dtRead.ToString() + ": " + oResult.sResult + " " + oResult.eResultType.ToString());

                            foreach (VRHReaderFrameworkCommon.clsProcessorBase oProcessor in colProcessorObjects)
                            {
                                System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction> colActions = null;

                                try
                                {
                                    colActions = oProcessor.Process(ref oResult);
                                }
                                catch
                                {
                                    UnlockReadResultGlobal(ref oResult);
                                    throw;
                                }
                                
                                if (colActions != null)
                                {
                                    foreach(VRHReaderFrameworkCommon.clsAction oAction in colActions)
                                    {
                                        oAction.uidProcessor = uidProcessor;
                                        if (oAction.uidAction == Guid.Empty)
                                            oAction.uidAction = Guid.NewGuid();
                                        AddActionGlobal(oAction);
                                    }
                                }
                            }

                            RemoveReadResultGlobal(ref oResult);
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(50);
                        }
                    }
                }
                catch(Exception e)
                {
                    VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message,e);
                }

                System.Threading.Thread.Sleep(5000);
            }
        }

        private static void RemoveActionGlobal(ref VRHReaderFrameworkCommon.clsAction oAction)
        {
            lock(lockObjectGlobalActions)
            {
                VRHReaderFrameworkCommon.clsLogger.Debug("Deleting action: " + oAction.uidAction.ToString());
                colGlobalActions.Remove(oAction);
            }
        }

        private static VRHReaderFrameworkCommon.clsAction GetUnprocessedActionGlobal(Guid uidReader , string sReaderId)
        {
            VRHReaderFrameworkCommon.clsAction oRet = null;
            lock(lockObjectGlobalActions)
            {
                foreach(VRHReaderFrameworkCommon.clsAction oAction in colGlobalActions)
                {
                    if(oAction.uidReader == uidReader || oAction.sTargetReaderId == sReaderId)
                    {
                        if(oAction.dtValidAfter < DateTime.Now)
                        {
                            VRHReaderFrameworkCommon.clsLogger.Debug("Returning action for processing: " + oAction.uidAction.ToString());
                            oRet = oAction;
                            break;
                        }
                    }
                }
            }
            return oRet;
        }

        private static void AddActionGlobal(VRHReaderFrameworkCommon.clsAction oAction)
        {
            lock(lockObjectGlobalActions)
            {
                VRHReaderFrameworkCommon.clsLogger.Debug("Action added to global store: " + oAction.uidAction.ToString());
                colGlobalActions.Add(oAction);
            }
        }

        private static void RemoveReadResultGlobal(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            lock(lockObjectGlobalReadResults)
            {
                VRHReaderFrameworkCommon.clsLogger.Debug("Deleting read result: " + oReadResult.sResult);
                colGlobalReadResults.Remove(oReadResult);
            }
        }

        private static void UnlockReadResultGlobal(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            lock (lockObjectGlobalReadResults)
            {
                VRHReaderFrameworkCommon.clsLogger.Debug("Unlocking read result: " + oReadResult.sResult);
                oReadResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed;
            }
        }

        private static VRHReaderFrameworkCommon.clsReadResult GetUnprocessedReadResultForProcessing(Guid uidProcessor)
        {
            VRHReaderFrameworkCommon.clsReadResult oRet = null;
            lock(lockObjectGlobalReadResults)
            {
                foreach(VRHReaderFrameworkCommon.clsReadResult oResult in colGlobalReadResults)
                {
                    if (oResult.eResultProcessingStatus == VRHReaderFrameworkCommon.eReadResultProcessingStatus.NotProcessed)
                    {
                        if (oResult.uidProcessor == uidProcessor || oResult.uidProcessor == Guid.Empty)
                        {
                            oResult.eResultProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.Processing;
                            oRet = oResult;
                            VRHReaderFrameworkCommon.clsLogger.Debug("Returning read result for processing: " + oResult.sResult);
                            break;
                        }
                    }
                }

            }
            return oRet;
        }

        private static void AddReadResultGlobal(VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            lock (lockObjectGlobalReadResults)
            {
                colGlobalReadResults.Add(oReadResult);
                VRHReaderFrameworkCommon.clsLogger.Debug("Read result added to global store: " + oReadResult.sResult);
            }
        }

        private static void ReaderThread(object oParam)
        {
            clsReaderThreadParam oThreadParam = (clsReaderThreadParam) oParam;

            Guid uidReader = Guid.NewGuid();

            VRHReaderFrameworkCommon.clsReaderBase oReader = null;
            VRHReaderFrameworkCommon.clsReaderFilterBase oReaderFilter = null;
            VRHReaderFrameworkCommon.clsControllerBase oController = null;
            
            bool bInitialized = false;

            while (bKeepRunning)
            {
                bInitialized = false;

                try
                {
                    VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Loading reader config: " + oThreadParam.oReaderThreadConfig.config);
                    oThreadParam.oReaderConfig = LoadReaderConfigBase(oThreadParam.oReaderThreadConfig.config);

                    if (oThreadParam.oReaderConfig != null)
                    {
                        VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Reader config loaded.");

                        if (oThreadParam.oReaderConfig.valid == 1)
                        { //Van érvényesnek jelölt konfiguráció
                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Reader config valid.");

                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Loading types.");
                            oReader = null;
                            System.Reflection.Assembly oReaderAssembly = GetAssemblyLoaded(oThreadParam.oReaderThreadConfig.assembly);
                            oReader = (VRHReaderFrameworkCommon.clsReaderBase)Activator.CreateInstance(oReaderAssembly.GetType(oThreadParam.oReaderThreadConfig.type));

                            oReaderFilter = null;
                            System.Reflection.Assembly oReaderFilterAssembly = GetAssemblyLoaded(oThreadParam.oReaderThreadConfig.filterassembly);
                            if (oReaderFilterAssembly != null)
                            {
                                oReaderFilter = (VRHReaderFrameworkCommon.clsReaderFilterBase)Activator.CreateInstance(oReaderFilterAssembly.GetType(oThreadParam.oReaderThreadConfig.filtertype));
                                
                                if (oReaderFilter != null)
                                {
                                    oReaderFilter.LoadConfig(basedir + "\\configs\\" + oThreadParam.oReaderThreadConfig.filterconfig);
                                }
                            }

                            oController = null;
                            System.Reflection.Assembly oControllerAssembly = GetAssemblyLoaded(oThreadParam.oReaderThreadConfig.controllerassembly);
                            if (oControllerAssembly != null)
                            {
                                oController = (VRHReaderFrameworkCommon.clsControllerBase)Activator.CreateInstance(oControllerAssembly.GetType(oThreadParam.oReaderThreadConfig.controllertype));

                                if (oController != null)
                                {
                                    oController.LoadConfig(basedir + "\\configs\\" + oThreadParam.oReaderThreadConfig.controllerconfig, basedir + "\\configs\\", oThreadParam.oReaderThreadConfig);
                                    oController.StartController();
                                }
                            }

                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Types loaded.");

                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Starting reader.");
                            oThreadParam.oReaderThreadConfig.readerip = oThreadParam.oReaderConfig.ip;
                            oReader.oReaderThreadConfig = oThreadParam.oReaderThreadConfig;
                            oReader.LoadConfig(basedir + "\\configs\\" + oThreadParam.oReaderThreadConfig.config, oThreadParam.oReaderThreadConfig.id);
                            oReader.OpenReader();

                            oReader.ReadStop();
                            oReader.ReadStart();

                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Reader started.");

                            DateTime dtLastReadTime = DateTime.MinValue;
                            DateTime dtLastPollTime = DateTime.MinValue;
                            DateTime dtLastConfigCheckTime = DateTime.MinValue;
                            DateTime dtLastPingTime = DateTime.MinValue;

                            int iControllerCycle = -1;

                            VRHReaderFrameworkCommon.eControllerResultRequestType iControllerResultRequestType = VRHReaderFrameworkCommon.eControllerResultRequestType.NoResult;

                            bInitialized = true;

                            while (bKeepRunning)
                            {
                                if (oController != null)
                                    iControllerResultRequestType = oController.GetResultRequestType();

                                if (oController != null)
                                {
                                    System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction> colControllerActions = oController.GetControllerActions();
                                    if (colControllerActions != null)
                                    {
                                        if (colControllerActions.Count > 0)
                                        {
                                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Controller sent action(s).");
                                        }

                                        foreach (VRHReaderFrameworkCommon.clsAction oAction in colControllerActions)
                                        {
                                            oAction.uidReader = uidReader;
                                            oAction.uidProcessor = Guid.Empty;

                                            if (oAction.uidAction == Guid.Empty)
                                                oAction.uidAction = Guid.NewGuid();

                                            AddActionGlobal(oAction);
                                        }
                                    }
                                }

                                while (bKeepRunning)
                                {
                                    VRHReaderFrameworkCommon.clsAction oAction = null;
                                    oAction = GetUnprocessedActionGlobal(uidReader , oThreadParam.oReaderThreadConfig.id);
                                    
                                    if (oAction == null)
                                        break;

                                    VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "PROCESSING ACTION: " + oAction.ToXML());

                                    VRHReaderFrameworkCommon.clsReadResult oResult = null;

                                    try
                                    {
                                        oResult = oReader.ProcessAction(oAction);
                                    }
                                    catch
                                    {
                                        VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Action processing failed.");

                                        RemoveActionGlobal(ref oAction);
                                        throw;
                                    }

                                    if (oResult != null)
                                    {
                                        if (oResult.uidProcessor == Guid.Empty)
                                        { //Ha a processzor üres, akkor csak a controllernek szólhat a válasz, egyébként senkinek nem fog kelleni.
                                            if (oController != null)
                                            { //Ha van controllerünk, akkor ez neki szól

                                                VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Sending action result to controller: " + oResult.ToXML());

                                                System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colTempReadResult = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();
                                                colTempReadResult.Add(oResult);
                                                oController.SetResults(colTempReadResult);
                                            }
                                        }
                                        else
                                        { // Ez egy processzornak szól
                                            AddReadResultGlobal(oResult);
                                        }
                                    }

                                    RemoveActionGlobal(ref oAction);
                                }

                                System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colReadResults = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();

                                System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colPollReadResults = null;
                                System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colNormReadResults = null;

                                if (dtLastPollTime.AddMilliseconds(oThreadParam.oReaderConfig.polldelay) <= DateTime.Now)
                                {
                                    VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Executing poll cycle.");

                                    dtLastPollTime = DateTime.Now;

                                    colPollReadResults = oReader.ReadPoll(false);

                                    if (colPollReadResults != null)
                                        foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colPollReadResults)
                                            colReadResults.Add(oResult);
                                }

                                if (oController != null)
                                    iControllerCycle = oController.GetCycle();

                                if (dtLastReadTime.AddMilliseconds(oThreadParam.oReaderConfig.readpolldelay) <= DateTime.Now && iControllerCycle != 0 )
                                {
                                    VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Executing read cycle.");
                                    dtLastReadTime = DateTime.Now;
                                    dtLastPingTime = DateTime.Now;
                                    
                                    colNormReadResults = oReader.ReadPoll(true);

                                    if (colNormReadResults != null)
                                        foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colNormReadResults)
                                            colReadResults.Add(oResult);

                                    if (oController != null)
                                        oController.CycleDone();
                                }

                                if (colReadResults != null)
                                {
                                    foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colReadResults)
                                    {
                                        oResult.uidReader = uidReader;

                                        if (oResult.uid == Guid.Empty)
                                            oResult.uid = Guid.NewGuid();

                                        VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "READ_POLL: " + oResult.ToXML());
                                    }

                                    if (oController != null && iControllerResultRequestType == VRHReaderFrameworkCommon.eControllerResultRequestType.Unfiltered)
                                    {
                                        if (colReadResults.Count > 0)
                                        {
                                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Sending before filter results to controller.");
                                            oController.SetResults(colReadResults);
                                        }
                                    }

                                    if (oReaderFilter != null)
                                    {
                                        if (colReadResults.Count > 0)
                                        {
                                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Filtering.");
                                            colReadResults = oReaderFilter.Filter(colReadResults);
                                        }
                                    }

                                    foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colReadResults)
                                    {
                                        AddReadResultGlobal(oResult);
                                    }

                                    if (oController != null && iControllerResultRequestType == VRHReaderFrameworkCommon.eControllerResultRequestType.Filtered)
                                    {
                                        if (colReadResults.Count > 0)
                                        {
                                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Sending after filter results to controller.");
                                            oController.SetResults(colReadResults);
                                        }
                                    }

                                }

                                if (dtLastPingTime.AddMilliseconds(oThreadParam.oReaderConfig.pingdelay) <= DateTime.Now)
                                {
                                    VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Executing ping cycle.");
                                    dtLastPingTime = DateTime.Now;
                                    oReader.Ping();
                                }

                                if (dtLastConfigCheckTime.AddSeconds(60) <= DateTime.Now)
                                {
                                    VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Executing config change check.");
                                    dtLastConfigCheckTime = DateTime.Now;
                                    VRHReaderFrameworkCommon.clsReaderConfigBase oTempReaderConfig = LoadReaderConfigBase(oThreadParam.oReaderThreadConfig.config);
                                    if (oTempReaderConfig.valid == 1)
                                    {
                                        if (oTempReaderConfig.dtLastWriteTime != oThreadParam.oReaderConfig.dtLastWriteTime)
                                        {
                                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Config changed.");
                                            oReader.ReadStop();
                                            oReader.CloseReader();
                                            throw new Exception("New reader config detected!");
                                        }
                                    }
                                    else
                                    {
                                        VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Reader config invalid.");
                                    }
                                }

                                System.Threading.Thread.Sleep(5);
                            }

                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Stop reader.");
                            oReader.ReadStop();
                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Close reader.");
                            oReader.CloseReader();

                            if (oController != null)
                            {
                                VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Stop controller.");
                                oController.StopController();
                            }
                        }
                    }

                }
                catch(Exception e)
                {
                    VRHReaderFrameworkCommon.clsLogger.Fatal("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + e.Message, e);

                    try
                    {
                        if (oReader != null)
                        {
                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Stop reader.");
                            oReader.ReadStop();
                        }
                    }
                    catch { }

                    try
                    {
                        if (oReader != null)
                        {
                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Close reader.");
                            oReader.CloseReader();
                            oReader = null;
                        }
                    }
                    catch{ }

                    try
                    {
                        if (oReaderFilter != null)
                        {
                            oReaderFilter = null;
                        }

                    }
                    catch{ }

                    try
                    {
                        if (oController != null)
                        {
                            VRHReaderFrameworkCommon.clsLogger.Debug("ReaderID: " + oThreadParam.oReaderThreadConfig.id + " " + "Stop controller.");
                            oController.StopController();
                            oController = null;
                        }
                    }
                    catch { }
                }

                if (bInitialized)
                    System.Threading.Thread.Sleep(5);
                else
                    System.Threading.Thread.Sleep(30000);
            }

        }

        private static System.Reflection.Assembly GetAssemblyLoaded(string name)
        {
            System.Reflection.Assembly oRetAssembly = null;
            foreach (clsLoadedAssembly oAssembly in colAssemblys)
            {
                if (string.Compare(oAssembly.name, name, true) == 0)
                {
                    oRetAssembly = oAssembly.assembly;
                    break;
                }
            }
            return oRetAssembly;
        }

        private static bool IsAssemblyLoaded(string name)
        {
            bool bRet = false;
            foreach (clsLoadedAssembly oAssembly in colAssemblys)
            {
                if (string.Compare(oAssembly.name , name,true) == 0)
                {
                    bRet = true;
                    break;
                }
            }
            return bRet;
        }

        private static VRHReaderFrameworkCommon.clsReaderConfigBase LoadReaderConfigBase(string name)
        {
            VRHReaderFrameworkCommon.clsReaderConfigBase oRetConfig = new VRHReaderFrameworkCommon.clsReaderConfigBase();

            try
            {
                System.IO.FileInfo oFileInfo = new System.IO.FileInfo(basedir + "\\configs\\" + name);
                oRetConfig.dtLastWriteTime = oFileInfo.LastWriteTime;

                System.Xml.XmlDocument oXmlDoc = new System.Xml.XmlDocument();
                try
                {
                    oXmlDoc.Load(basedir + "\\configs\\" + name);
                }
                catch (Exception e)
                {
                    using (System.IO.StreamReader oReader = new System.IO.StreamReader(basedir + "\\configs\\" + name, System.Text.Encoding.GetEncoding(1250)))
                    {
                        oXmlDoc.Load(oReader);
                    }
                }


                foreach (System.Xml.XmlNode node in oXmlDoc.DocumentElement.ChildNodes)
                {
                    if (string.Compare(node.Name, "valid", true) == 0)
                    {
                        oRetConfig.valid = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "readpolldelay", true) == 0)
                    {
                        oRetConfig.readpolldelay = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "polldelay", true) == 0)
                    {
                        oRetConfig.polldelay = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "pingdelay", true) == 0)
                    {
                        oRetConfig.pingdelay = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "ip", true) == 0)
                    {
                        oRetConfig.ip = node.InnerText;
                    }
                }
                oXmlDoc = null;

            }
            catch(Exception e)
            {
                oRetConfig = null;
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message,e);
            }

            return oRetConfig;
        }

        private static bool LoadReadersConfig()
        {
            bool bRet = false;
            try
            {
                System.Xml.XmlDocument oXmlDoc = new System.Xml.XmlDocument();

                try
                {
                    oXmlDoc.Load(basedir + "\\configs\\readers.xml");
                }
                catch(Exception e)
                {
                    using (System.IO.StreamReader oReader = new System.IO.StreamReader(basedir + "\\configs\\readers.xml", System.Text.Encoding.GetEncoding(1250)))
                    {
                        oXmlDoc.Load(oReader);
                    }
                }

                foreach (System.Xml.XmlNode node in oXmlDoc.DocumentElement.ChildNodes)
                {
                    if (string.Compare(node.Name, "reader", true) == 0)
                    {
                        VRHReaderFrameworkCommon.clsReaderThreadConfig oReaderThreadConfig = new VRHReaderFrameworkCommon.clsReaderThreadConfig();
                        foreach (System.Xml.XmlNode readernode in node.ChildNodes)
                        {
                            if (string.Compare(readernode.Name, "name", true) == 0)
                            {
                                oReaderThreadConfig.name = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "id", true) == 0)
                            {
                                oReaderThreadConfig.id = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "location", true) == 0)
                            {
                                oReaderThreadConfig.location = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "description", true) == 0)
                            {
                                oReaderThreadConfig.description = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "config", true) == 0)
                            {
                                oReaderThreadConfig.config = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "assembly", true) == 0)
                            {
                                oReaderThreadConfig.assembly = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "type", true) == 0)
                            {
                                oReaderThreadConfig.type = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "filterconfig", true) == 0)
                            {
                                oReaderThreadConfig.filterconfig = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "filterassembly", true) == 0)
                            {
                                oReaderThreadConfig.filterassembly = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "filtertype", true) == 0)
                            {
                                oReaderThreadConfig.filtertype = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "controllerconfig", true) == 0)
                            {
                                oReaderThreadConfig.controllerconfig = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "controllerassembly", true) == 0)
                            {
                                oReaderThreadConfig.controllerassembly = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "controllertype", true) == 0)
                            {
                                oReaderThreadConfig.controllertype = readernode.InnerText;
                            }
                        }
                        colReaderThreadConfigs.Add(oReaderThreadConfig);
                    }
                }
                oXmlDoc = null;
                bRet = true;
            }
            catch(Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message,e);
                colReaderThreadConfigs = null;
            }

            return bRet;
        }

        private static bool LoadProcessorConfig()
        {
            bool bRet = false;
            try
            {
                System.Xml.XmlDocument oXmlDoc = new System.Xml.XmlDocument();
                try
                {
                    oXmlDoc.Load(basedir + "\\configs\\processor.xml");
                }
                catch (Exception e)
                {
                    using (System.IO.StreamReader oReader = new System.IO.StreamReader(basedir + "\\configs\\processor.xml", System.Text.Encoding.GetEncoding(1250)))
                    {
                        oXmlDoc.Load(oReader);
                    }
                }

                foreach (System.Xml.XmlNode node in oXmlDoc.DocumentElement.ChildNodes)
                {
                    if (string.Compare(node.Name, "threads", true) == 0)
                    {
                        oProcessorThreadConfig.threads = int.Parse(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "processor", true) == 0)
                    {
                        VRHReaderFrameworkCommon.clsProcessorConfig oProcessorConfig = new VRHReaderFrameworkCommon.clsProcessorConfig();
                        foreach (System.Xml.XmlNode readernode in node.ChildNodes)
                        {
                            if (string.Compare(readernode.Name, "config", true) == 0)
                            {
                                oProcessorConfig.config = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "assembly", true) == 0)
                            {
                                oProcessorConfig.assembly = readernode.InnerText;
                            }
                            else if (string.Compare(readernode.Name, "type", true) == 0)
                            {
                                oProcessorConfig.type = readernode.InnerText;
                            }
                        }
                        oProcessorThreadConfig.colProcessorConfigs.Add(oProcessorConfig);
                    }
                }
                oXmlDoc = null;
                bRet = true;
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message,e);
            }

            return bRet;
        }

        public static void Start(string sBaseDir , string sStartMode)
        {
            try
            {
                basedir = sBaseDir;

                VRHReaderFrameworkCommon.clsLogger.LoadConfig(basedir + "\\configs\\logger.xml");

                try
                {
                    VRHReaderFrameworkCommon.clsLogger.cleanup(DateTime.Now.AddDays(-7));
                }
                catch { }

                VRHReaderFrameworkCommon.clsLogger.Info("Starting mode: " + sStartMode);

                VRHReaderFrameworkCommon.clsLogger.Info("Starting.");

                VRHReaderFrameworkCommon.clsLogger.Info("basedir=" + basedir);

                bKeepRunning = true;

                if (!started)
                {
                    lockObjectGlobalReadResults = new object();
                    lockObjectGlobalActions = new object();
                    colReaderThreadConfigs = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReaderThreadConfig>();
                    colAssemblys = new System.Collections.Generic.List<clsLoadedAssembly>();
                    colReaderThreads = new System.Collections.Generic.List<System.Threading.Thread>();
                    oProcessorThreadConfig = new VRHReaderFrameworkCommon.clsProcessorThreadConfig();
                    oProcessorThreadConfig.colProcessorConfigs = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsProcessorConfig>();
                    colProcessorThreads = new System.Collections.Generic.List<System.Threading.Thread>();
                    colGlobalReadResults = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();
                    colGlobalActions = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction>();

                    if (LoadReadersConfig())
                    {
                        bool bStartWcfInterface = false;

                        //Reader assembly -k betöltése
                        foreach (VRHReaderFrameworkCommon.clsReaderThreadConfig oReaderThreadConfig in colReaderThreadConfigs)
                        {
                            if (!string.IsNullOrEmpty(oReaderThreadConfig.assembly))
                            {
                                if (!IsAssemblyLoaded(oReaderThreadConfig.assembly))
                                {
                                    clsLoadedAssembly oAssembly = new clsLoadedAssembly();
                                    oAssembly.name = oReaderThreadConfig.assembly;
                                    oAssembly.assembly = System.Reflection.Assembly.LoadFile(basedir + "\\" + oReaderThreadConfig.assembly);
                                    colAssemblys.Add(oAssembly);
                                }
                            }

                            if (!string.IsNullOrEmpty(oReaderThreadConfig.filterassembly))
                            {
                                if (!IsAssemblyLoaded(oReaderThreadConfig.filterassembly))
                                {
                                    clsLoadedAssembly oAssembly = new clsLoadedAssembly();
                                    oAssembly.name = oReaderThreadConfig.filterassembly;
                                    oAssembly.assembly = System.Reflection.Assembly.LoadFile(basedir + "\\" + oReaderThreadConfig.filterassembly);
                                    colAssemblys.Add(oAssembly);
                                }
                            }

                            if (!string.IsNullOrEmpty(oReaderThreadConfig.controllerassembly))
                            {
                                if (!IsAssemblyLoaded(oReaderThreadConfig.controllerassembly))
                                {
                                    clsLoadedAssembly oAssembly = new clsLoadedAssembly();
                                    oAssembly.name = oReaderThreadConfig.controllerassembly;
                                    oAssembly.assembly = System.Reflection.Assembly.LoadFile(basedir + "\\" + oReaderThreadConfig.controllerassembly);
                                    colAssemblys.Add(oAssembly);
                                    if (string.Compare(oAssembly.name, "VRHReaderFrameWorkWCFController.dll", true) == 0)
                                        bStartWcfInterface = true;
                                }
                            }
                        }
                        //Reader assembly -k betöltve

                        if (LoadProcessorConfig())
                        {
                            //Processor assembly -k betöltése
                            foreach (VRHReaderFrameworkCommon.clsProcessorConfig oProcessorConfig in oProcessorThreadConfig.colProcessorConfigs)
                            {
                                if (!string.IsNullOrEmpty(oProcessorConfig.assembly))
                                {
                                    if (!IsAssemblyLoaded(oProcessorConfig.assembly))
                                    {
                                        clsLoadedAssembly oAssembly = new clsLoadedAssembly();
                                        oAssembly.name = oProcessorConfig.assembly;
                                        oAssembly.assembly = System.Reflection.Assembly.LoadFile(basedir + "\\" + oProcessorConfig.assembly);
                                        colAssemblys.Add(oAssembly);
                                    }
                                }
                            }
                            //Prpcessor assembly -k betöltve
                        }

                        ////////////////
                        // WCFInterfész indítása
                        if (bStartWcfInterface)
                        {
                            //svcHost_Controller = new System.ServiceModel.ServiceHost(typeof(VRHReaderFrameworkWCFInterface.ControllerInterface));
                            svcHost_Application = new System.ServiceModel.ServiceHost(typeof(VRHReaderFrameworkWCFInterface.AppInterface));

                            //svcHost_Controller.Open();
                            svcHost_Application.Open();
                        }
                        ////////////////

                        //Thread -ek indítása
                        VRHReaderFrameworkCommon.clsLogger.Info("Starting processor threads.");
                        for (int iCount = 0; iCount < oProcessorThreadConfig.threads; iCount++)
                        {
                            System.Threading.Thread oThread = new System.Threading.Thread(ProcessorThread);
                            oThread.Start(new object());
                        }

                        VRHReaderFrameworkCommon.clsLogger.Info("Starting reader threads.");
                        foreach (VRHReaderFrameworkCommon.clsReaderThreadConfig oReaderThreadConfig in colReaderThreadConfigs)
                        {
                            System.Threading.Thread oThread = new System.Threading.Thread(ReaderThread);
                            clsReaderThreadParam oThreadParam = new clsReaderThreadParam();
                            oThreadParam.oReaderThreadConfig = oReaderThreadConfig;
                            oThread.Start(oThreadParam);
                        }
                        //Thread -ek elindítva

                        started = true;
                    }
                }
                VRHReaderFrameworkCommon.clsLogger.Info("Started.");
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message,e);
                throw;
            }
        }

        public static void Stop()
        {
            try
            {
                VRHReaderFrameworkCommon.clsLogger.Info("Stopping.");

                bKeepRunning = false;

                if (started)
                {
                    VRHReaderFrameworkCommon.clsLogger.Info("Stopping reader threads.");
                    foreach (System.Threading.Thread oReaderThread in colReaderThreads)
                    {
                        oReaderThread.Abort();
                    }

                    VRHReaderFrameworkCommon.clsLogger.Info("Stopping processor threads.");
                    foreach (System.Threading.Thread oProcessorThread in colProcessorThreads)
                    {
                        oProcessorThread.Abort();
                    }

                    //if (svcHost_Controller != null)
                    //    svcHost_Controller.Close();

                    if (svcHost_Application != null)
                        svcHost_Application.Close();

                    colGlobalActions = null;
                    colGlobalReadResults = null;
                    oProcessorThreadConfig = null;
                    colProcessorThreads = null;
                    colReaderThreads = null;
                    colReaderThreadConfigs = null;
                    colAssemblys = null;
                    lockObjectGlobalReadResults = null;
                    lockObjectGlobalActions = null;
                    started = false;
                }
                VRHReaderFrameworkCommon.clsLogger.Info("Stopped.");
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message,e);
                throw;
            }
        }
    }
}
