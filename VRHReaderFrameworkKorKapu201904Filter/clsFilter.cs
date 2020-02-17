using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkKorKapu201904Filter
{
    class clsFilterConfig
    {
        public int tagmaxagesec = int.MaxValue;
        public int tagtimeoutsec = 0;
        public int decodegs1 = 0;
        public string start = "";
    }

    class clsFilter : VRHReaderFrameworkCommon.clsReaderFilterBase
    {
        private clsFilterConfig oFilterConfig;
        private System.Collections.Generic.Dictionary<string, VRHReaderFrameworkCommon.clsReadResult> dictResult;
        private object objLock;

        override public void LoadConfig(string name)
        {
            objLock = new Object();
            dictResult = new System.Collections.Generic.Dictionary<string, VRHReaderFrameworkCommon.clsReadResult>();

            oFilterConfig = new clsFilterConfig();

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
                    if (string.Compare(node.Name, "tagtimeoutsec", true) == 0)
                    {
                        oFilterConfig.tagtimeoutsec = int.Parse(node.InnerText);
                    }
                    if (string.Compare(node.Name, "decodegs1", true) == 0)
                    {
                        oFilterConfig.decodegs1 = int.Parse(node.InnerText);
                    }
                    if (string.Compare(node.Name, "start", true) == 0)
                    {
                        oFilterConfig.start = node.InnerText;
                    }
                    if (string.Compare(node.Name, "tagmaxagesec", true) == 0)
                    {
                        oFilterConfig.tagmaxagesec = int.Parse(node.InnerText);
                    }
                    
                }
                oXmlDoc = null;

            }
            catch
            {
                oFilterConfig = null;
                throw;
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
                    } catch { }
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
                    } catch { }
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
                    if (_BinaryString.Length == 12*8)
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


        private bool Exists(VRHReaderFrameworkCommon.clsReadResult oResult)
        {
            bool bRet = false;

            lock (objLock)
            {
                if (dictResult.ContainsKey(oResult.sResult))
                {
                    VRHReaderFrameworkCommon.clsReadResult oStoredResult;
                    if (dictResult.TryGetValue(oResult.sResult, out oStoredResult))
                    {
                        if ((DateTime.Now - oStoredResult.dtRead).TotalSeconds < oFilterConfig.tagtimeoutsec)
                        {
                            bRet = true;
                        }
                        else
                        {
                            dictResult.Remove(oResult.sResult);
                        }
                    }
                }

                if (bRet == false)
                {
                    dictResult.Add(oResult.sResult, oResult);
                }
            }

            return bRet;
        }

        override public System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> Filter(System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colResults)
        {
            System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colRet = new List<VRHReaderFrameworkCommon.clsReadResult>();

            foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colResults)
            {
                if (oResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data || oResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent)
                {
                    if ((DateTime.Now - oResult.dtRead).TotalSeconds <= oFilterConfig.tagmaxagesec)
                    {

                        if (!string.IsNullOrEmpty(oFilterConfig.start))
                        {
                            string tempData = oResult.sResult;
                            if (oFilterConfig.decodegs1 == 1)
                            {
                                tempData = GSRN96.gs1decode(tempData);
                            }

                            if (tempData.StartsWith(oFilterConfig.start, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!Exists(oResult))
                                {
                                    colRet.Add(oResult);
                                }
                                else
                                {
                                    VRHReaderFrameworkCommon.clsLogger.Debug("Filtered out on time base: " + oResult.sResult);
                                }
                            }
                            else
                            {
                                VRHReaderFrameworkCommon.clsLogger.Debug("Filtered out on config base: " + oResult.sResult);
                            }
                        }
                        else
                        {
                            if (!Exists(oResult))
                            {
                                colRet.Add(oResult);
                            }
                            else
                            {
                                VRHReaderFrameworkCommon.clsLogger.Debug("Filtered out on time base: " + oResult.sResult);
                            }
                        }
                    }
                    else
                    {
                        VRHReaderFrameworkCommon.clsLogger.Debug("Filtered out on time base - too old: " + oResult.sResult);
                    }
                }
                else
                {
                    colRet.Add(oResult);
                }
            }

            return colRet;
        }
    }
}
