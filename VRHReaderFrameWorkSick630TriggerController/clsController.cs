using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameWorkSick630TriggerController
{
    internal class clsControllerConfig
    {
        public int iPackageTimeoutSec = 60;
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
                oAction.iAction = 2;
                oAction.uidReader = Guid.Empty;
                oAction.uidAction = Guid.Empty;
                oAction.uidProcessor = Guid.Empty;
                oAction.colActionParameters = new List<string>();
                oAction.colActionParameters.Add("sMN MIStartIn");
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
                oAction.iAction = 2;
                oAction.uidReader = Guid.Empty;
                oAction.uidAction = Guid.Empty;
                oAction.uidProcessor = Guid.Empty;
                oAction.colActionParameters = new List<string>();
                oAction.colActionParameters.Add("sMN MIStopIn");
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
                        int.TryParse(node.InnerText, out oControllerConfig.iPackageTimeoutSec);
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
                if (tsElapsed.TotalSeconds > oControllerConfig.iPackageTimeoutSec)
                {
                    ReaderOff();
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
                    if (oReadResult.colSubResults != null)
                    {
                        if (oReadResult.colSubResults[1].value == "eIN1")
                        {
                            if (oReadResult.colSubResults[4].value == "1")
                            {
                                if (bMezo1Hit == false && bMezo2Hit == false)
                                { //Ez az első mezőbe lépés
                                    bMezo1Hit = true;
                                    ReaderOn();
                                }
                                else if (bMezo1Hit == false && bMezo2Hit == true)
                                { //Ha a másik mező már korábban felment
                                    ReaderOff();
                                }
                            }
                            /*else if (oReadResult.colSubResults[4].value == "0")
                            {
                            }*/
                        }
                        else if (oReadResult.colSubResults[1].value == "eIN2")
                        {
                            if (oReadResult.colSubResults[4].value == "1")
                            {
                                if (bMezo1Hit == false && bMezo2Hit == false)
                                { //Ez az első mezőbe lépés
                                    bMezo2Hit = true;
                                    ReaderOn();
                                }
                                else if (bMezo1Hit == true && bMezo2Hit == false)
                                { //Ha a másik mező már korábban felment
                                    ReaderOff();
                                }
                            }
                            /*else if (oReadResult.colSubResults[4].value == "0")
                            {
                            }*/
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
