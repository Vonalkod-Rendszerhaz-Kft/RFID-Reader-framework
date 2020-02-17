using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkCommon
{
    public class clsAction
    {
        public int iAction;
        public Guid uidReader;
        public Guid uidAction;
        public Guid uidProcessor;
        public System.Collections.Generic.List<string> colActionParameters;
        public DateTime dtAction;
        public DateTime dtValidAfter;
        public string sTargetReaderId;
        
        public string ToXML()
        {
            StringBuilder sB = new StringBuilder("");

            sB.Append("<clsAction>");
            sB.Append("<iAction>" + iAction.ToString() + "</iAction>");
            sB.Append("<uidReader>" + uidReader.ToString() + "</uidReader>");
            sB.Append("<sTargetReaderId>" + sTargetReaderId + "</sTargetReaderId>");
            sB.Append("<uidAction>" + uidAction.ToString() + "</uidAction>");
            sB.Append("<uidProcessor>" + uidProcessor.ToString() + "</uidProcessor>");
            if (colActionParameters != null)
            {
                sB.Append("<colActionParameters>");

                foreach (string s in colActionParameters)
                    sB.Append("<sActionParameter>" + s + "</sActionParameter>");

                sB.Append("</colActionParameters>");
            }
            sB.Append("<dtAction>" + dtAction.ToString() + "</dtAction>");
            sB.Append("<dtValidAfter>" + dtValidAfter.ToString() + "</dtValidAfter>");

            sB.Append("</clsAction>");

            return sB.ToString();
        }
    }
}
