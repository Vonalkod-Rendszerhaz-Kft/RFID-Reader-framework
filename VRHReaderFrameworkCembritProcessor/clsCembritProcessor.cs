using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkCembritProcessor
{
    public class clsCembritProcessorConfig
    {
        public string sCembritWSUrl = "";
    }

    public class clsCembritProcessor : VRHReaderFrameworkCommon.clsProcessorBase
    {
        clsCembritProcessorConfig oConfig = null;

        public override void LoadConfig(string name)
        {
            oConfig = new clsCembritProcessorConfig();

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
                if (string.Compare(node.Name, "CembritWSUrl", true) == 0)
                {
                    oConfig.sCembritWSUrl = node.InnerText;
                }
            }

            oXmlDoc = null;
        }

        private string GetSubResultValue(ref VRHReaderFrameworkCommon.clsReadResult oReadResult , string item)
        {
            string subresultvalue = "";

            foreach(VRHReaderFrameworkCommon.clsReadSubResult oSubResult in oReadResult.colSubResults)
            {
                if (string.Compare(oSubResult.name,item,true) == 0)
                {
                    subresultvalue = oSubResult.value;
                    break;
                }

            }

            return subresultvalue;
        }

        public override List<VRHReaderFrameworkCommon.clsAction> Process(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();

            if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data || oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent)
            {
                CembritWS.RFIDServiceClient oWSClient = new CembritWS.RFIDServiceClient();

                if (oWSClient != null)
                {
                    oWSClient.Open();

                    System.Collections.Generic.List<CembritWS.ReadedTag> colTags = new List<CembritWS.ReadedTag>();

                    

                    int readedCount = 0;
                    double rssi = 0;

                    int.TryParse(GetSubResultValue(ref oReadResult, "count"), out readedCount);
                    double.TryParse(GetSubResultValue(ref oReadResult, "rssi").Replace(",", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator).Replace(".", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator), out rssi);
                    CembritWS.ReadedTag oTag = new CembritWS.ReadedTag();
                    oTag.readedCount = readedCount;
                    oTag.RSSI = rssi;
                    oTag.TAGID = oReadResult.sResult;
                    colTags.Add(oTag);
                    
                    CembritWS.ResultBase oRes = oWSClient.TagsReaded(colTags.ToArray(), oReadResult.oReaderThreadConfig.id , "");

                    oWSClient.Close();
                }
            }

            oReadResult.eAppProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.Processed;

            return colRet;
        }
    }
}
