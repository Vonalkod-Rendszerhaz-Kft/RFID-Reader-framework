using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkTimeBasedFilter
{
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
                }
                oXmlDoc = null;

            }
            catch
            {
                oFilterConfig = null;
                throw;
            }
        }
        
        private bool Exists(VRHReaderFrameworkCommon.clsReadResult oResult)
        {
            bool bRet = false;

            lock(objLock)
            {
                if(dictResult.ContainsKey(oResult.sResult))
                {
                    VRHReaderFrameworkCommon.clsReadResult oStoredResult;
                    if (dictResult.TryGetValue(oResult.sResult, out oStoredResult))
                    {
                        if((DateTime.Now - oStoredResult.dtRead).TotalSeconds < oFilterConfig.tagtimeoutsec)
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
                    if (!Exists(oResult))
                    {
                        colRet.Add(oResult);
                    }
                    else
                    {
                        VRHReaderFrameworkCommon.clsLogger.Debug("Filtered out: " + oResult.sResult);
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
