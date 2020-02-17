using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameWorkSzapportaProcessor
{
    public class clsSzapportaItem
    {
        public string sId = "";
        public string sAnt = "";
        public string sRSSI = "";
        public DateTime dtLeolvasas = DateTime.MinValue;

    }

    public class clsSzapportaPackage
    {
        public clsSzapportaPackage()
        {
        }

        public List<clsSzapportaItem> colItems = new List<clsSzapportaItem>();

        public Guid uidPackage = Guid.NewGuid();

        public bool bMezo1Hit = false;
        public bool bMezo2Hit = false;

        public int iIrany = 0;

        public string sReaderIP = "";
        public Guid uidReader;

        public DateTime dtElsoMessage = DateTime.MinValue;
        public DateTime dtUtolsoMessage = DateTime.MinValue;

        public bool bArchiveLater = false;

        public DateTime dtErrorLogged = DateTime.MaxValue;
    }

    public class clsSzapportaProcessor : VRHReaderFrameworkCommon.clsProcessorBase
    {
        private static object lockThread = new object();
        private static object lockData = new object();
        private static object lockLog = new object();

        private static List<clsSzapportaPackage> colPackages = new List<clsSzapportaPackage>();

        private System.Threading.Thread oThread = new System.Threading.Thread(ProcessorThread);

        public static int iDataOffset = 4;
        public static int iPackageTimeoutSec = 10;
        public static string sRSSIValueName = "rssi";
        public static string sANTValueName = "ant";
        public static string UserName = "";
        public static string Password = "";

        private static bool ArchivePackage(clsSzapportaPackage oPackage)
        {
            bool bRet = false;
            try
            {

                SzapportaWS.RfidReaderServicesClient oClient = new SzapportaWS.RfidReaderServicesClient();
                oClient.ClientCredentials.UserName.UserName = UserName;
                oClient.ClientCredentials.UserName.Password = Password;


                oClient.Open();

                foreach (clsSzapportaItem oItem in oPackage.colItems)
                {
                    byte bIrany = 0;

                    if (oPackage.iIrany == 1)
                    {
                        bIrany = 1;
                    }
                    else if (oPackage.iIrany == 2)
                    {
                        bIrany = 2;
                    }

                    string sTime = oItem.dtLeolvasas.ToString("yyyy.MM.dd HH:mm:ss");
                    oClient.RegisterTagRead( oItem.sId, bIrany, sTime, oPackage.sReaderIP);
                }

                oClient.Close();
                bRet = true;

            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
            }

            return bRet;
        }

        private static void ProcessorThread(object oParam)
        {
            while (true)
            {
                lock (lockData)
                {
                    try
                    {
                        List<clsSzapportaPackage> removePackages = new List<clsSzapportaPackage>();
                        List<clsSzapportaPackage> archivePackages = new List<clsSzapportaPackage>();

                        foreach (clsSzapportaPackage oPackage in colPackages)
                        {
                            if (oPackage.dtUtolsoMessage != DateTime.MinValue)
                            {
                                if ((DateTime.Now - oPackage.dtUtolsoMessage).TotalSeconds > iPackageTimeoutSec)
                                {
                                    if (oPackage.bArchiveLater)
                                    {
                                        archivePackages.Add(oPackage);
                                    }
                                    else
                                    {
                                        removePackages.Add(oPackage);
                                    }
                                }
                            }
                        }

                        foreach (clsSzapportaPackage oPackage in archivePackages)
                        {
                            if (ArchivePackage(oPackage))
                            {
                                colPackages.Remove(oPackage);

                                string sLog = "Mentett csomag törölve! " + "Reader IP: " + oPackage.sReaderIP;
                                VRHReaderFrameworkCommon.clsLogger.Info(sLog);
                            }
                        }

                        foreach (clsSzapportaPackage oPackage in removePackages)
                        {
                            colPackages.Remove(oPackage);

                            string sLog = "Lejárt csomag törölve! " + "Reader IP: " + oPackage.sReaderIP;
                            VRHReaderFrameworkCommon.clsLogger.Info(sLog);
                        }
                    }
                    catch (Exception e)
                    {
                        VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                    }
                }

                //ArchiveLogs();

                System.Threading.Thread.Sleep(1000);
            }
        }


        public override void LoadConfig(string name)
        {
            System.Xml.XmlDocument oXmlDoc = new System.Xml.XmlDocument();

            try
            {
                oXmlDoc.Load(name);
            }
            catch
            {
                using (System.IO.StreamReader oReader = new System.IO.StreamReader(name, System.Text.Encoding.GetEncoding(1250)))
                {
                    oXmlDoc.Load(oReader);
                }
            }

            foreach (System.Xml.XmlNode node in oXmlDoc.DocumentElement.ChildNodes)
            {
                if (string.Compare(node.Name, "UserName", true) == 0)
                {
                    UserName = node.InnerText;
                }
                else if (string.Compare(node.Name, "iDataOffset", true) == 0)
                {
                    iDataOffset = int.Parse(node.InnerText);
                }
                else if (string.Compare(node.Name, "iPackageTimeoutSec", true) == 0)
                {
                    iPackageTimeoutSec = int.Parse(node.InnerText);
                }
                else if (string.Compare(node.Name, "Password", true) == 0)
                {
                    Password = node.InnerText;
                }
                else if (string.Compare(node.Name, "sRSSIValueName", true) == 0)
                {
                    sRSSIValueName = node.InnerText;
                }
                else if (string.Compare(node.Name, "sANTValueName", true) == 0)
                {
                    sANTValueName = node.InnerText;
                }
            }

            oXmlDoc = null;
        }


        public string ConvertHexToAscii(string hexString, int offset)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = offset; i < hexString.Length; i += 2)
                {
                    String hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;

                }

                return ascii;
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
            }

            return string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oReadResult"></param>
        /// <returns>
        /// 0 --> Siker
        /// 1 --> Úgy jött adat, hogy nem érkezett még event a mezőktől
        /// 2 --> Mező esemény rendellenesség
        /// 3 --> Azonosító átalakítási hiba
        /// 4 --> Ismeretlen azonosító
        /// 5 --> Többes targonca azonosító
        /// 6 --> Nincsen targonca azonosító, és nincsen leolvasott azonosító
        /// 7 --> Nincsen leolvasott azonosító
        /// 8 --> Nincsen targonca azonosító
        /// 9 --> Valószínűleg nem ment át a targonca
        /// -1 --> Olvasási csomag mentve
        /// -2 --> Olvasási csomag mentésre jelölve
        /// -3 --> Mezőbe lépés
        /// -4 --> Olvasás vége
        /// </returns>
        public int ProcessItem(VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            int iRet = 0;

            lock (lockData)
            {
                try
                {
                    if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data
                        || oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent
                        || oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Event)
                    {
                        clsSzapportaPackage oPackage = colPackages.FirstOrDefault(x => x.uidReader == oReadResult.uidReader);

                        if (oPackage == null)
                        {
                            oPackage = new clsSzapportaPackage();
                            oPackage.dtElsoMessage = DateTime.Now;
                            oPackage.uidReader = oReadResult.uidReader;
                            oPackage.sReaderIP = oReadResult.oReaderThreadConfig.readerip;
                            colPackages.Add(oPackage);
                        }

                        oPackage.dtUtolsoMessage = DateTime.Now;

                        if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data
                            || oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent)
                        {
                            if (oPackage.bMezo1Hit || oPackage.bMezo2Hit)
                            { // Ha valamelyik mező már felment
                                string sId = oReadResult.sResult.Substring(iDataOffset); //ConvertHexToAscii(oReadResult.sResult, iDataOffset);

                                if (!string.IsNullOrEmpty(sId))
                                {
                                    clsSzapportaItem oItem = oPackage.colItems.FirstOrDefault(x => x.sId == sId);

                                    if (oItem == null)
                                    {
                                        oItem = new clsSzapportaItem();
                                        oItem.sId = sId;
                                        oItem.dtLeolvasas = oReadResult.dtRead;

                                        if (oReadResult.colSubResults != null)
                                        {
                                            foreach (VRHReaderFrameworkCommon.clsReadSubResult oSubResult in oReadResult.colSubResults)
                                            {
                                                if (oSubResult.name == sRSSIValueName)
                                                {
                                                    oItem.sRSSI = oSubResult.value;
                                                }
                                                else if (oSubResult.name == sANTValueName)
                                                {
                                                    oItem.sAnt = oSubResult.value;
                                                }
                                            }
                                        }

                                        oPackage.colItems.Add(oItem);
                                    }
                                }
                                else
                                {
                                    string sLog = "Az azonosító üres! " + "Reader IP: " + oPackage.sReaderIP;
                                    VRHReaderFrameworkCommon.clsLogger.Info(sLog);
                                }
                            }
                            else
                            {
                                string sLog = "Úgy érkezett adat, hogy nincsen aktív érzékelő mező! " + "Reader IP: " + oPackage.sReaderIP;
                                VRHReaderFrameworkCommon.clsLogger.Info(sLog);

                                iRet = 1;
                            }
                        }
                        else if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Event)
                        {
                            System.Diagnostics.Debug.WriteLine("EVENT: " + oReadResult.sOriginalResult + "\n");
                            if (oReadResult.colSubResults != null)
                            {
                                if (oReadResult.colSubResults[1].value == "eIN1")
                                {
                                    if (oReadResult.colSubResults[4].value == "1")
                                    {
                                        if (oPackage.bMezo1Hit == false && oPackage.bMezo2Hit == false)
                                        { //Ez az első mezőbe lépés
                                            oPackage.bMezo1Hit = true;
                                            oPackage.iIrany = 1;

                                            iRet = -3;
                                        }
                                        else if (oPackage.bMezo1Hit == false && oPackage.bMezo2Hit == true)
                                        { //Ha a másik mező már korábban felment
                                            oPackage.bMezo1Hit = true;
                                            iRet = -4;
                                        }
                                        /*else
                                        {
                                            iRet = 2;
                                        }*/
                                    }
                                    else if (oReadResult.colSubResults[4].value == "0")
                                    {
                                        if (oPackage.bMezo1Hit == true && oPackage.bMezo2Hit == true)
                                        { // Olvasás vége
                                            iRet = EndOfReading(ref oPackage);
                                        }
                                    }
                                }
                                else if (oReadResult.colSubResults[1].value == "eIN2")
                                {
                                    if (oReadResult.colSubResults[4].value == "1")
                                    {
                                        if (oPackage.bMezo1Hit == false && oPackage.bMezo2Hit == false)
                                        { //Ez az első mezőbe lépés
                                            oPackage.bMezo2Hit = true;
                                            oPackage.iIrany = 2;

                                            iRet = -3;
                                        }
                                        else if (oPackage.bMezo1Hit == true && oPackage.bMezo2Hit == false)
                                        { //Ha a másik mező már korábban felment
                                            oPackage.bMezo2Hit = true;
                                            iRet = -4;
                                        }
                                        /*else
                                        {
                                            iRet = 2;
                                        }*/
                                    }
                                    else if (oReadResult.colSubResults[4].value == "0")
                                    {
                                        if (oPackage.bMezo1Hit == true && oPackage.bMezo2Hit == true)
                                        { // Olvasás vége
                                            iRet = EndOfReading(ref oPackage);
                                        }
                                    }
                                }
                            }
                        }

                        if (iRet > 0)
                        {
                            if (oPackage != null)
                            {
                                colPackages.Remove(oPackage);

                                string sLogMsg = "";

                                switch (iRet)
                                {
                                    case 1:
                                        sLogMsg = "Úgy érkezett adat, hogy nincsen aktív érzékelő mező!";
                                        break;
                                    case 2:
                                        sLogMsg = "Érzékelő mezőhiba!";
                                        break;
                                    case 5:
                                        sLogMsg = "Többes targonca azonosító!";
                                        break;
                                    case 6:
                                        sLogMsg = "Nincsen leolvasott azonosító és targonca azonosító!";
                                        break;
                                    case 7:
                                        sLogMsg = "Nincsen leolvasott azonosító!";
                                        break;
                                    case 8:
                                        sLogMsg = "Nincsen leolvasott targonca azonosító!";
                                        break;
                                    case 9:
                                        sLogMsg = "A targonca valószínűleg nem haladt át mindkét mezőn!";
                                        break;
                                    default:
                                        break;
                                }

                                string sLog = "Hiba miatt az olvasási csomagot töröltük! (" + sLogMsg + ")" + "Reader IP: " + oPackage.sReaderIP;
                                VRHReaderFrameworkCommon.clsLogger.Info(sLog);
                            }
                        }
                    }
                }
                catch
                {

                }
            }


            return iRet;
        }

        private int EndOfReading(ref clsSzapportaPackage oPackage)
        {
            int iRet = 0;

            if (oPackage.colItems.Count == 0)
            {
                iRet = 7;

                string sLog = "Nincsen leolvasott azonosító! " + "Reader IP: " + oPackage.sReaderIP;
                VRHReaderFrameworkCommon.clsLogger.Info(sLog);
            }
            else
            {
                iRet = 0;
            }

            if (iRet == 0)
            {
                if (ArchivePackage(oPackage))
                {
                    colPackages.Remove(oPackage);

                    string sLog = "Az olvasási csomag elmentve! " + "Reader IP: " + oPackage.sReaderIP;
                    VRHReaderFrameworkCommon.clsLogger.Info(sLog);

                    iRet = -1;
                }
                else
                {
                    oPackage.bArchiveLater = true;
                    oPackage.uidReader = Guid.Empty;

                    string sLog = "Az olvasási csomag mentésre jelölve! " + "Reader IP: " + oPackage.sReaderIP;
                    VRHReaderFrameworkCommon.clsLogger.Info(sLog);

                    iRet = -2;
                }
            }

            return iRet;
        }

        public override List<VRHReaderFrameworkCommon.clsAction> Process(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();

            try
            {
                if (oReadResult != null)
                {
                    if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data
                        || oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent
                        || oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Event)
                    {
                        int iProcessingResult = 0;
                        lock (lockData)
                        {
                            try
                            {
                                iProcessingResult = ProcessItem(oReadResult);
                            }
                            catch (Exception e)
                            {
                                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                            }
                        }

                        /// -1 --> Olvasási csomag mentve
                        /// -2 --> Olvasási csomag mentésre jelölve
                        /// -3 --> Mezőbe lépés

                        if (iProcessingResult == 0)
                        {

                        }
                        else if (iProcessingResult == -1)
                        {
                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 2 1");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now.AddSeconds(10);
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
                                colRet.Add(oAction);
                            }
                        }
                        else if (iProcessingResult == -2)
                        {
                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 2 1");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now.AddSeconds(10);
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
                                colRet.Add(oAction);
                            }


                        }
                        else if (iProcessingResult == -3)
                        {
                            /*{
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN MIStartIn");
                                colRet.Add(oAction);
                            }*/

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 1 1");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 2 1");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now.AddSeconds(1);
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now.AddSeconds(1);
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
                                colRet.Add(oAction);
                            }


                        }
                        else if (iProcessingResult == -4)
                        {
                            /*{
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN MIStopIn");
                                colRet.Add(oAction);
                            }*/
                        }
                        else
                        { // Hiba van
                            /*{
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN MIStopIn");
                                colRet.Add(oAction);
                            }*/

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 2 0");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now;
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 1 1");
                                colRet.Add(oAction);
                            }

                            {
                                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                oAction.dtAction = DateTime.Now;
                                oAction.dtValidAfter = DateTime.Now.AddSeconds(10);
                                oAction.iAction = 2;
                                oAction.uidReader = oReadResult.uidReader;
                                oAction.uidAction = Guid.Empty;
                                oAction.uidProcessor = Guid.Empty;
                                oAction.colActionParameters = new List<string>();
                                oAction.colActionParameters.Add("sMN mDOSetOutput 1 0");
                                colRet.Add(oAction);
                            }
                        }
                    }

                    lock (lockThread)
                    {
                        try
                        {
                            if (oThread.ThreadState == System.Threading.ThreadState.Unstarted)
                            {
                                oThread.Start(null);
                            }
                            else if (oThread.ThreadState == System.Threading.ThreadState.Stopped || oThread.ThreadState == System.Threading.ThreadState.Aborted)
                            {
                                oThread = new System.Threading.Thread(ProcessorThread);
                                oThread.Start(null);
                            }
                        }
                        catch (Exception e)
                        {
                            VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                        }
                    }


                }
                oReadResult.eAppProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.Processed;
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                throw;
            }


            return colRet;
        }
    }
}
