using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkCommon
{
    public enum eReadResultType
    {
        Data = 0,
        Failure = 1,
        Event = 2,
        ActionResult = 3,
        DataEvent = 4,
        Log = 5
    }

    public enum eReadResultProcessingStatus
    {
        NotProcessed = 0,
        Processing = 1,
        Processed = 2
    }

    public class clsReadSubResult
    {
        public string name;
        public string value;

        public clsReadSubResult()
        {

        }

        public clsReadSubResult(string p_name , string p_value)
        {
            name = p_name;
            value = p_value;
        }

        public string ToXML()
        {
            StringBuilder sB = new StringBuilder("");
            sB.Append("<clsReadSubResult>");
            sB.Append("<name>");
            sB.Append(name);
            sB.Append("</name>");
            sB.Append("<value>");
            sB.Append(value);
            sB.Append("</value>");
            sB.Append("</clsReadSubResult>");
            return sB.ToString();
        }

		public override string ToString()
		{
			return this.name + " " + this.value;
		}

	}

    public class clsReadResult
    {
        public string sResult;
        public clsReaderThreadConfig oReaderThreadConfig;
        public eReadResultType eResultType;
        public DateTime dtRead;
        public eReadResultProcessingStatus eResultProcessingStatus;
        public Guid uid;
        public Guid uidReader;
        public Guid uidProcessor;
        public Guid uidAction;
        public System.Collections.Generic.List<clsReadSubResult> colSubResults;
        public eReadResultProcessingStatus eAppProcessingStatus;
        public string sOriginalResult = "";
        public int iViewedState = 0;

        public string ToXML()
        {
            StringBuilder sB = new StringBuilder("");
            sB.Append("<clsReadResult>");

            sB.Append("<sResult>");
            sB.Append(sResult);
            sB.Append("</sResult>");

            sB.Append("<oReaderThreadConfig>");
            sB.Append(oReaderThreadConfig.ToXML());
            sB.Append("</oReaderThreadConfig>");

            sB.Append("<eResultType>");
            sB.Append(eResultType.ToString());
            sB.Append("</eResultType>");
            
            sB.Append("<dtRead>");
            sB.Append(dtRead.ToString());
            sB.Append("</dtRead>");

            sB.Append("<eResultProcessingStatus>");
            sB.Append(eResultProcessingStatus.ToString());
            sB.Append("</eResultProcessingStatus>");

            sB.Append("<eAppProcessingStatus>");
            sB.Append(eAppProcessingStatus.ToString());
            sB.Append("</eAppProcessingStatus>");

            sB.Append("<uid>");
            sB.Append(uid.ToString());
            sB.Append("</uid>");

            sB.Append("<uidReader>");
            sB.Append(uidReader.ToString());
            sB.Append("</uidReader>");

            sB.Append("<uidProcessor>");
            sB.Append(uidProcessor.ToString());
            sB.Append("</uidProcessor>");

            sB.Append("<uidAction>");
            sB.Append(uidAction.ToString());
            sB.Append("</uidAction>");

            if (colSubResults != null)
            {
                if (colSubResults.Count > 0)
                {
                    sB.Append("<colSubResults>");
                    foreach(clsReadSubResult oSubResult in colSubResults)
                    {
                        sB.Append(oSubResult.ToXML());
                    }
                    sB.Append("</colSubResults>");
                }
            }


            sB.Append("</clsReadResult>");
            return sB.ToString();
        }
    }

    public class clsReadResult_RID_TAGID_COUNT_RSSI
    {
        public string sReaderId;
        public string sTagId;
        public int iCount;
        public double dRssi;
        public eReadResultType eResultType;
    }
}
