using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkKorKapu201904Processor
{
    public class clsProcessor : VRHReaderFrameworkCommon.clsProcessorBase
    {

        public class KapuPi
        {
            public KapuPi()
            {

            }

            ~KapuPi()
            {
                try
                {
                    if (oPlayerThread.ThreadState == System.Threading.ThreadState.Running)
                    {
                        oPlayerThread.Abort();
                    }
                }
                catch { }
                
            }

            public string kapu = "";
            public string pi = "";
            public object lockObject = new object();
            List<string> playList = new List<string>();

            static void playerThread(object _caller)
            {
                KapuPi oBase = (KapuPi)_caller;

                bool bGo = true;
                while(bGo)
                {
                    try
                    {
                        string currentPlay = "";
                        lock(oBase.lockObject)
                        {
                            if (oBase.playList.Count > 0)
                            {
                                currentPlay = oBase.playList[0];
                                oBase.playList.RemoveAt(0);
                            }
                        }

                        if (!string.IsNullOrEmpty(currentPlay))
                        {
                            oBase.PlayPiSequence(currentPlay);
                        }

                        System.Threading.Thread.Sleep(100);
                    }
                    catch(Exception e)
                    {
                        VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                        System.Threading.Thread.Sleep(10000);
                    }
                }

            }

            System.Threading.Thread oPlayerThread = new System.Threading.Thread(playerThread);

            public void StartPlayerThread()
            {
                if (oPlayerThread.ThreadState != System.Threading.ThreadState.Running)
                {
                    oPlayerThread.Start((object)this);
                }
            }

            public void AddToPlayList(string sequence)
            {
                try
                {
                    lock(lockObject)
                    {
                        playList.Add(sequence);
                    }
                }
                catch(Exception e)
                {
                    VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                    throw;
                }
            }


            public void PlayPiSequence(string sequence)
            {
                try
                {
                    if (string.IsNullOrEmpty(sequence) || string.IsNullOrEmpty(kapu) || string.IsNullOrEmpty(pi))
                        return;

                    VRHReaderFrameworkCommon.clsLogger.Info("Playing pi sequnce on: " + pi + " [" + sequence + "]");

                    string[] mainparts = sequence.Split(';');

                    foreach (string mainpart in mainparts)
                    {
                        string[] parts = mainpart.Split(':');

                        if (parts.Length == 1)
                        {
                            int sleeptime = 0;
                            if (int.TryParse(parts[0], out sleeptime))
                            {
                                if (sleeptime < 0)
                                    sleeptime = 0;

                                if (sleeptime > 60000)
                                    sleeptime = 60000;

                                System.Threading.Thread.Sleep(sleeptime);
                            }
                        }
                        else if (parts.Length == 2)
                        {
                            uint pin = uint.MaxValue;
                            uint value = uint.MaxValue;

                            if (uint.TryParse(parts[0], out pin) && uint.TryParse(parts[1], out value))
                            {
                                if (pin != uint.MaxValue && value != uint.MaxValue)
                                {
                                    uint read_value = WebIOPi.GetValue(pi, pin);
                                    if (read_value != uint.MaxValue && read_value != value)
                                    {
                                        read_value = WebIOPi.SetValue(pi, pin, value);
                                    }
                                }
                            }
                        }
                        else { }

                    }

                }
                catch (Exception e)
                {
                    VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                }
            }
        
        }


        public class VRHReaderFrameworkKorKapu201904ProcessorConfig
        {
            public string InfoRendKapuFuncUrl = "";
            public string InfoRendKapuFuncData = "";
            public string InfoRendUsername = "";
            public string InfoRendPassword = "";
            public bool decodegs1 = false;
            public string portinit = "";
            public string success = "";
            public string fail = "";
            public string error = "";
            public string InfoRendLoginFuncUrl = "";
            public int WSTimeout = 100000;
            public int WSReadWriteTimeout = 300000;

            public List<KapuPi> PiK = new List<KapuPi>();
        }

        public VRHReaderFrameworkKorKapu201904ProcessorConfig oConfig = new VRHReaderFrameworkKorKapu201904ProcessorConfig();


        private void InitPiPorts()
        {
            try
            {
                if (!string.IsNullOrEmpty(oConfig.portinit))
                {
                    foreach (KapuPi oPi in oConfig.PiK)
                    {
                        oPi.PlayPiSequence(oConfig.portinit);
                        oPi.StartPlayerThread();
                    }
                }
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                throw;
            }
        }

        public override void LoadConfig(string name)
        {
            oConfig = new VRHReaderFrameworkKorKapu201904ProcessorConfig();

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
                if (string.Compare(node.Name, "InfoRendKapuFuncUrl", true) == 0)
                {
                    oConfig.InfoRendKapuFuncUrl = node.InnerText;
                }
                else if (string.Compare(node.Name, "InfoRendKapuFuncData", true) == 0)
                {
                    oConfig.InfoRendKapuFuncData = node.InnerText;
                }
                else if (string.Compare(node.Name, "InfoRendUsername", true) == 0)
                {
                    oConfig.InfoRendUsername = node.InnerText;
                }
                else if (string.Compare(node.Name, "InfoRendPassword", true) == 0)
                {
                    oConfig.InfoRendPassword = node.InnerText;
                }
                else if (string.Compare(node.Name, "decodegs1", true) == 0)
                {
                    if (!string.IsNullOrEmpty(node.InnerText))
                    {
                        if (node.InnerText == "1" || node.InnerText.ToLower() == "yes" || node.InnerText.ToLower() == "true")
                        {
                            oConfig.decodegs1 = true;
                        }
                    }
                }
                else if (string.Compare(node.Name, "portinit", true) == 0)
                {
                    oConfig.portinit = node.InnerText;
                }
                else if (string.Compare(node.Name, "success", true) == 0)
                {
                    oConfig.success = node.InnerText;
                }
                else if (string.Compare(node.Name, "fail", true) == 0)
                {
                    oConfig.fail = node.InnerText;
                }
                else if (string.Compare(node.Name, "error", true) == 0)
                {
                    oConfig.error = node.InnerText;
                }
                else if (string.Compare(node.Name, "InfoRendLoginFuncUrl", true) == 0)
                {
                    oConfig.InfoRendLoginFuncUrl = node.InnerText;
                }
                else if (string.Compare(node.Name, "WSTimeout", true) == 0)
                {
                    if (!int.TryParse(node.InnerText,out oConfig.WSTimeout))
                    {
                        oConfig.WSTimeout = 100000;
                    }
                }
                else if (string.Compare(node.Name, "WSReadWriteTimeout", true) == 0)
                {
                    if (!int.TryParse(node.InnerText, out oConfig.WSReadWriteTimeout))
                    {
                        oConfig.WSReadWriteTimeout = 100000;
                    }
                }
                else if (string.Compare(node.Name, "kapu_pi", true) == 0)
                {
                    KapuPi oPi = new KapuPi();

                    foreach (System.Xml.XmlNode inner_node in node.ChildNodes)
                    {
                        if (string.Compare(inner_node.Name, "kapu", true) == 0)
                        {
                            oPi.kapu = inner_node.InnerText;
                        }
                        else if (string.Compare(inner_node.Name, "pi", true) == 0)
                        {
                            oPi.pi = inner_node.InnerText;
                        }
                    }

                    oConfig.PiK.Add(oPi);
                }

            }

            InitPiPorts();

            oXmlDoc = null;
        }

        private void AddToKapuPlayList(string kapu , string sequence)
        {
            foreach(KapuPi oKapu in oConfig.PiK)
            {
                if (oKapu.kapu.ToLower() == kapu.ToLower())
                {
                    if (!string.IsNullOrEmpty(sequence))
                    {
                        oKapu.AddToPlayList(sequence);
                    }
                }
            }
        }

        public override List<VRHReaderFrameworkCommon.clsAction> Process(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {

            List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();

            oReadResult.eAppProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.Processed;


            if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data)
            {
                string sResult = oReadResult.sResult;

                if (oConfig.decodegs1)
                {
                    sResult = GSRN96.gs1decode(oReadResult.sResult);
                }

                bool bJogosult = true;

                try
                {
                    //System.Diagnostics.Debug.WriteLine("Calling InfoRend Api " + sResult);

                    bool needlogin = false;
                    bJogosult = InfoRendFunctions.BetegKapu(oConfig.InfoRendKapuFuncUrl, oConfig.InfoRendKapuFuncData, oConfig.InfoRendUsername, oConfig.InfoRendPassword, sResult, oReadResult.oReaderThreadConfig.name,oConfig.WSTimeout,oConfig.WSReadWriteTimeout,out needlogin);

                    if (needlogin)
                    {
                        if (InfoRendFunctions.Login(oConfig.InfoRendLoginFuncUrl,oConfig.InfoRendUsername,oConfig.InfoRendPassword,oConfig.WSTimeout,oConfig.WSReadWriteTimeout))
                        {
                            bJogosult = InfoRendFunctions.BetegKapu(oConfig.InfoRendKapuFuncUrl, oConfig.InfoRendKapuFuncData, oConfig.InfoRendUsername, oConfig.InfoRendPassword, sResult, oReadResult.oReaderThreadConfig.name, oConfig.WSTimeout, oConfig.WSReadWriteTimeout, out needlogin);
                        }
                    }

                    if (bJogosult)
                    {
                        AddToKapuPlayList(oReadResult.oReaderThreadConfig.name, oConfig.success);
                    }
                    else
                    {
                        AddToKapuPlayList(oReadResult.oReaderThreadConfig.name, oConfig.fail);
                    }
                }
                catch 
                {
                    AddToKapuPlayList(oReadResult.oReaderThreadConfig.name, oConfig.error);
                }

            }

            return colRet;
        }
    }

    class WebIOPi
    {
        public static uint GetValue(string baseaddress, uint pin, string username = "", string password = "")
        {
            uint uiRet = uint.MaxValue;
            try
            {
                string url = "" + baseaddress + "/GPIO/" + pin.ToString() + "/value";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    string AuthString = "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(username + ":" + password));
                    request.Headers.Add("Authorization", AuthString);
                }

                request.Method = "GET";

                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                using (StreamReader responseReader = new StreamReader(webStream))
                {
                    string response = responseReader.ReadToEnd();
                    uint.TryParse(response, out uiRet);
                }
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                throw;
            }

            return uiRet;
        }

        public static uint SetValue(string baseaddress, uint pin, uint value, string username = "", string password = "")
        {
            uint uiRet = uint.MaxValue;
            try
            {
                string url = "" + baseaddress + "/GPIO/" + pin.ToString() + "/value/" + value.ToString();
                string data = @"";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    string AuthString = "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(username + ":" + password));
                    request.Headers.Add("Authorization", AuthString);
                }

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;
                using (Stream webStream = request.GetRequestStream())
                using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
                {
                    requestWriter.Write(data);
                }

                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                using (StreamReader responseReader = new StreamReader(webStream))
                {
                    string response = responseReader.ReadToEnd();
                    uint.TryParse(response, out uiRet);
                }
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                throw;
            }

            return uiRet;
        }
    }

    class InfoRendFunctions
    {
        public static bool Login(string url, string username, string password, int timeout, int readwritetimeout)
        {
            string sFuncInfoForLog = "InfoRendFunctions::Login() --> " + "Url:" + url + " --> ";

            bool bRet = false;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    string AuthString = "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(username + ":" + password));
                    request.Headers.Add("Authorization", AuthString);
                }

                request.Method = "GET";
                request.ContentType = "application/json";
                request.Timeout = timeout;
                request.ReadWriteTimeout = readwritetimeout;

                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                using (StreamReader responseReader = new StreamReader(webStream))
                {
                    string response = responseReader.ReadToEnd();

                    if (!string.IsNullOrEmpty(response))
                    {
                        bRet = true;
                    }

                    VRHReaderFrameworkCommon.clsLogger.Info(sFuncInfoForLog + response);
                }
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(sFuncInfoForLog + e.Message, e);

                throw;
            }

            return bRet;
        }

        public static bool BetegKapu(string url, string data, string username, string password, string readerresult, string readername, int timeout, int readwritetimeout, out bool needlogin)
        {
            string sFuncInfoForLog = "InfoRendFunctions::BetegKapu() --> " + "Url:" + url + " Data:" + data + " Reader:" + readername + " Readerresult:" + readerresult + " --> ";

            bool bRet = false;

            needlogin = false;

            try
            {
                data = data.Replace("@oReadResult.sResult@", readerresult);
                data = data.Replace("@oReadResult.oReaderThreadConfig.name@", readername);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    string AuthString = "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(username + ":" + password));
                    request.Headers.Add("Authorization", AuthString);
                }

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;
                request.Timeout = timeout;
                request.ReadWriteTimeout = readwritetimeout;

                using (Stream webStream = request.GetRequestStream())
                using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
                {
                    requestWriter.Write(data);
                }

                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                using (StreamReader responseReader = new StreamReader(webStream))
                {
                    string response = responseReader.ReadToEnd();

                    if (response.ToLower().Contains("{\"success\":true}"))
                    {
                        bRet = true;
                    }

                    VRHReaderFrameworkCommon.clsLogger.Info(sFuncInfoForLog + response);
                }
            }
            catch (System.Net.WebException we)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(sFuncInfoForLog + we.Message, we);

                if (we.Response != null)
                {
                    System.Net.HttpWebResponse oErrorResponse = (System.Net.HttpWebResponse)we.Response;
                    needlogin = true;

                    if (oErrorResponse != null)
                    {
                        if (oErrorResponse.StatusCode == HttpStatusCode.Forbidden || 
                            oErrorResponse.StatusCode == HttpStatusCode.Unauthorized || 
                            oErrorResponse.StatusCode == HttpStatusCode.BadRequest ||
                            oErrorResponse.StatusCode == HttpStatusCode.MethodNotAllowed ||
                            oErrorResponse.StatusCode == HttpStatusCode.NotAcceptable)
                        {
                            needlogin = true;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(sFuncInfoForLog + e.Message, e);
                throw;
            }

            return bRet;
        }
    }

    class GSRN96
    {
        private string _BinaryString = "";

        public GSRN96(string BinaryString)
        {
            _BinaryString = BinaryString;
        }

        private int ServiceReferenceDigits
        {
            get
            {
                return (17 - GS1CompanyPrefixDigits);
            }
        }

        private int ServiceReferenceLen
        {
            get
            {
                return (58 - GS1CompanyPrefixLen);
            }
        }

        private int GS1CompanyPrefixDigits
        {
            get
            {
                int iRet = 12;
                switch (Partition)
                {
                    case 1:
                        iRet = 11;
                        break;
                    case 2:
                        iRet = 10;
                        break;
                    case 3:
                        iRet = 9;
                        break;
                    case 4:
                        iRet = 8;
                        break;
                    case 5:
                        iRet = 7;
                        break;
                    case 6:
                        iRet = 6;
                        break;
                    default:
                        break;
                }
                return iRet;
            }
        }

        private int GS1CompanyPrefixLen
        {
            get
            {
                int iRet = 40;
                switch (Partition)
                {
                    case 1:
                        iRet = 37;
                        break;
                    case 2:
                        iRet = 34;
                        break;
                    case 3:
                        iRet = 30;
                        break;
                    case 4:
                        iRet = 27;
                        break;
                    case 5:
                        iRet = 24;
                        break;
                    case 6:
                        iRet = 20;
                        break;
                    default:
                        break;
                }
                return iRet;
            }
        }

        public int Header
        {
            get
            {
                try
                {
                    string sRet = _BinaryString.Substring(0, 8);
                    return Convert.ToInt32(sRet, 2);
                }
                catch { }
                return 0;
            }
        }

        public int Filter
        {
            get
            {
                try
                {
                    string sRet = _BinaryString.Substring(8, 3);
                    return Convert.ToInt32(sRet, 2);
                }
                catch { }
                return 0;
            }
        }

        public int Partition
        {
            get
            {
                try
                {
                    string sRet = _BinaryString.Substring(11, 3);
                    return Convert.ToInt32(sRet, 2);
                }
                catch { }
                return 0;
            }
        }

        public int GS1CompanyPrefix
        {
            get
            {
                try
                {
                    string sRet = _BinaryString.Substring(14, GS1CompanyPrefixLen);
                    return Convert.ToInt32(sRet, 2);
                }
                catch { }
                return 0;
            }
        }

        public string sGS1CompanyPrefix
        {
            get
            {
                string sRet = "";
                try
                {
                    sRet = GS1CompanyPrefix.ToString();
                    while (sRet.Length < GS1CompanyPrefixDigits)
                        sRet = "0" + sRet;
                }
                catch { }
                return sRet;
            }
        }

        public int ServiceReference
        {
            get
            {
                try
                {
                    string sRet = _BinaryString.Substring(14 + GS1CompanyPrefixLen, ServiceReferenceLen);
                    return Convert.ToInt32(sRet, 2);
                }
                catch { }
                return 0;
            }
        }

        public string sServiceReference
        {
            get
            {
                string sRet = "";
                try
                {
                    sRet = ServiceReference.ToString();
                    while (sRet.Length < ServiceReferenceDigits)
                        sRet = "0" + sRet;
                }
                catch { }
                return sRet;
            }
        }

        public string sDecodedValue
        {
            get
            {
                return sGS1CompanyPrefix + sServiceReference;
            }
        }

        public bool Valid
        {
            get
            {
                bool bRet = false;
                if (_BinaryString.Length == 12 * 8)
                {
                    if (_BinaryString.StartsWith("00101101"))
                    {
                        if (Filter == 0)
                        {
                            if (Partition >= 0 && Partition <= 6)
                            {
                                if (sDecodedValue.Length == 17)
                                {
                                    bRet = true;
                                }
                            }
                        }
                    }
                }
                return bRet;
            }
        }

        public static string gs1decode(string encodedString)
        {
            string decodedString = encodedString;

            if (!string.IsNullOrEmpty(encodedString))
            {
                if (encodedString.Length == 24)
                {
                    if (encodedString.Length % 2 == 0)
                    {
                        if (encodedString.StartsWith("2D", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                string BinaryString = "";
                                for (int iHexCounter = 0; iHexCounter < encodedString.Length; iHexCounter += 2)
                                {
                                    string sHexPart = encodedString.Substring(iHexCounter, 2);
                                    string sBinary = Convert.ToString(Convert.ToByte(sHexPart, 16), 2);
                                    while (sBinary.Length < 8)
                                        sBinary = "0" + sBinary;
                                    BinaryString += sBinary;
                                }

                                GSRN96 oCode = new GSRN96(BinaryString);

                                if (oCode.Valid)
                                    decodedString = oCode.sDecodedValue;

                            }
                            catch { }
                        }
                    }
                }
            }
            return decodedString;
        }
    }

}
