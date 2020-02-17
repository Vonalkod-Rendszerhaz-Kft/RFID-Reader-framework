using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkKorKapu201904Controller
{
    internal class clsControllerConfig
    {
        public int iReadTimeoutSec = 10;
        public string Port = "";
        public string State = "";
    }

    public class clsController : VRHReaderFrameworkCommon.clsControllerBase
    {
        clsControllerConfig oControllerConfig = null;
        private List<VRHReaderFrameworkCommon.clsAction> colGlobalActions = null;
        private bool bReaderOn = true;
        private DateTime dtReaderStateChange = DateTime.MinValue;

        bool bMezo1Hit = false;
        bool bMezo2Hit = false;


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
                //oAction.colActionParameters = new List<string>();
                //oAction.colActionParameters.Add("sMN MIStartIn");
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
                //oAction.colActionParameters = new List<string>();
                //oAction.colActionParameters.Add("sMN MIStopIn");
                colGlobalActions.Add(oAction);
            }
            bReaderOn = false;
            bMezo1Hit = false;
            bMezo2Hit = false;
            dtReaderStateChange = DateTime.Now;
        }

        /*private TimeSpan ParseTimeSpan(string sTimeSpan)
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
        }*/

        public override void LoadConfig(string name, string basedir, VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
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
                    if (string.Compare(node.Name, "iPackageTimeoutSec", true) == 0)
                    {
                        int.TryParse(node.InnerText, out oControllerConfig.iReadTimeoutSec);
                    }
                    if (string.Compare(node.Name, "Port", true) == 0)
                    {
                        oControllerConfig.Port = node.InnerText;
                    }
                    if (string.Compare(node.Name, "State", true) == 0)
                    {
                        oControllerConfig.State = node.InnerText;
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
            if (bReaderOn == true)
            {
                TimeSpan tsElapsed = DateTime.Now - dtReaderStateChange;
                if (tsElapsed.TotalSeconds > oControllerConfig.iReadTimeoutSec)
                {
                    ReaderOff();
                }
            }

            return;
        }

        private string getParamValue(string containerString , string paramName , string defaultValue)
        {
            string sRet = defaultValue;

            try
            {
                int start = containerString.IndexOf("<" + paramName + ">");
                if (start > -1)
                {
                    start += 2 + paramName.Length;
                    int stop = containerString.IndexOf("</" + paramName + ">", start);
                    if (stop > -1)
                    {
                        int len = stop - start;
                        sRet = containerString.Substring(start, len);
                    }
                }
            } catch { }

            return sRet;
        }

        public override void SetResults(List<VRHReaderFrameworkCommon.clsReadResult> colReadResults)
        {
            foreach (VRHReaderFrameworkCommon.clsReadResult oReadResult in colReadResults)
            {
                if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Event)
                {
                    if (oReadResult.sResult.StartsWith("<GPIEvent>",StringComparison.OrdinalIgnoreCase))
                    {
                        string Port = getParamValue(oReadResult.sResult,"Port","");
                        string State = getParamValue(oReadResult.sResult, "State", "");

                        if (!string.IsNullOrEmpty(Port) && !string.IsNullOrEmpty(State) && !string.IsNullOrEmpty(oControllerConfig.Port) && !string.IsNullOrEmpty(oControllerConfig.State))
                        {
                            if (oControllerConfig.Port == Port && oControllerConfig.State == State)
                            {
                                ReaderOn();
                            }
                        }
                    }
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
