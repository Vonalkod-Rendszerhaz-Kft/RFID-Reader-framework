using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkIF2TriggerController
{
    internal class clsControllerConfig
    {
        public string triggername = "STARTREAD";
        public TimeSpan tsOnlineStartTime = new TimeSpan(0,0,0);
        public TimeSpan tsOnlineStopTime = new TimeSpan(23,59,59);
        public TimeSpan tsOfflineStopDelay = new TimeSpan(0, 0, 30);
    }

    public class clsController : VRHReaderFrameworkCommon.clsControllerBase
    {
        clsControllerConfig oControllerConfig = null;
        private List<VRHReaderFrameworkCommon.clsAction> colGlobalActions = null;
        private bool bReaderOn = true;
        private DateTime dtReaderStateChange = DateTime.MinValue;
        private bool bOfflineHours = true;


        private void ReaderOn()
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
                colGlobalActions.Add(oAction);
            }
            bReaderOn = true;
            dtReaderStateChange = DateTime.Now;
        }

        private void ReaderOff()
        {
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
                colGlobalActions.Add(oAction);
            }
            bReaderOn = false;
            dtReaderStateChange = DateTime.Now;
        }

        private TimeSpan ParseTimeSpan(string sTimeSpan)
        {
            TimeSpan oRet;

            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            List<string> colParts = sTimeSpan.Split(':').ToList();
            if (colParts != null)
            {
                if (colParts.Count > 0)
                {
                    hours = int.Parse(colParts[0]);
                }
                if (colParts.Count > 1)
                {
                    minutes = int.Parse(colParts[1]);
                }
                if (colParts.Count > 2)
                {
                    seconds = int.Parse(colParts[2]);
                }
            }
            oRet = new TimeSpan(hours, minutes, seconds);

            return oRet;
        }

        public override void LoadConfig(string name , string basedir, VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
            oControllerConfig = new clsControllerConfig();
            
            try
            {
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

                foreach (System.Xml.XmlNode node in oXmlDoc.DocumentElement.ChildNodes)
                {
                    if (string.Compare(node.Name, "triggername", true) == 0)
                    {
                        oControllerConfig.triggername = node.InnerText;
                    }
                    else if (string.Compare(node.Name, "onlinetimestart", true) == 0)
                    {
                        oControllerConfig.tsOnlineStartTime = ParseTimeSpan(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "onlinetimestop", true) == 0)
                    {
                        oControllerConfig.tsOnlineStopTime = ParseTimeSpan(node.InnerText);
                    }
                    else if (string.Compare(node.Name, "offlinestopdelay", true) == 0)
                    {
                        oControllerConfig.tsOfflineStopDelay = ParseTimeSpan(node.InnerText);
                    }
                }
                oXmlDoc = null;

            }
            catch
            {
                oControllerConfig = null;
                throw;
            }
        }

        public override int GetCycle()
        {
            return -1; //Folyamatos olvasás
        }

        public override void CycleDone()
        {
            if (bOfflineHours)
            { //Offline időszak delayed stop kezelés
                if (bReaderOn)
                {
                    if ((DateTime.Now - dtReaderStateChange) > oControllerConfig.tsOfflineStopDelay)
                    {
                        ReaderOff();
                    }
                }
            }

            { //Online időszak ellenőrzése
                if (oControllerConfig.tsOnlineStopTime <= oControllerConfig.tsOnlineStartTime)
                {
                    if (DateTime.Now.TimeOfDay >= oControllerConfig.tsOnlineStartTime || DateTime.Now.TimeOfDay < oControllerConfig.tsOnlineStopTime)
                    {
                        bOfflineHours = false;
                        if (!bReaderOn)
                            ReaderOn();
                    }
                    else
                    {
                        bOfflineHours = true;
                    }
                }
                else if (oControllerConfig.tsOnlineStopTime > oControllerConfig.tsOnlineStartTime)
                {
                    if (DateTime.Now.TimeOfDay >= oControllerConfig.tsOnlineStartTime && DateTime.Now.TimeOfDay < oControllerConfig.tsOnlineStopTime)
                    {
                        bOfflineHours = false;
                        if (!bReaderOn)
                            ReaderOn();
                    }
                    else
                    {
                        bOfflineHours = true;
                    }
                }
            }
            return;
        }

        public override void SetResults(List<VRHReaderFrameworkCommon.clsReadResult> colReadResults)
        {
            foreach (VRHReaderFrameworkCommon.clsReadResult oReadResult in colReadResults)
            {
                if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Event)
                {
                    if (!string.IsNullOrEmpty(oReadResult.sResult))
                    {
                        if (oReadResult.sResult.StartsWith("EVT:TRIGGER",StringComparison.OrdinalIgnoreCase))
                        {
                            List<string> colResultString = oReadResult.sResult.Split(' ').ToList();
                            if (colResultString.Count > 1)
                            {
                                //if (string.Compare(oControllerConfig.triggername,colResultString[1],true) == 0)
                                if (oControllerConfig.triggername.Contains(colResultString[1]) == true)
                                {
                                    if (!bReaderOn)
                                        ReaderOn(); //Ha nincsen bekapcsolva, akkor bekapcsoljuk
                                    else
                                        dtReaderStateChange = DateTime.Now; //Ha be van kapcsolva, akkor csak az időpontot frissíthük, hogy kitoljuk a lekapcsolást.

                                    break;
                                }
                            }
                        }
                    }
                }
                else if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.ActionResult)
                {

                }
            }
        }

        public override VRHReaderFrameworkCommon.eControllerResultRequestType GetResultRequestType()
        {
            return VRHReaderFrameworkCommon.eControllerResultRequestType.Unfiltered;
        }

        public override void StartController()
        {
            colGlobalActions = new List<VRHReaderFrameworkCommon.clsAction>();

            ReaderOff();
        }

        public override void StopController()
        {
            colGlobalActions = null;
        }

        public override List<VRHReaderFrameworkCommon.clsAction> GetControllerActions()
        {
            List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();

            foreach (VRHReaderFrameworkCommon.clsAction oAction in colGlobalActions)
            {
                colRet.Add(oAction);
            }

            colGlobalActions.Clear();

            return colRet;
        }
    }
}
