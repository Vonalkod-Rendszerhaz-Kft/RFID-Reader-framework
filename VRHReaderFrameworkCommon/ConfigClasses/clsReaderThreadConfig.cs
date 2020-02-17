using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkCommon
{
    public class clsReaderThreadConfig
    {
        public string name;
        public string id;
        public string location;
        public string description;
        public string config;
        public string assembly;
        public string type;
        public string filterconfig;
        public string filterassembly;
        public string filtertype;
        public string controllerconfig;
        public string controllerassembly;
        public string controllertype;
        public string readerip;

        public string ToXML()
        {
            StringBuilder sB = new StringBuilder("");
            sB.Append("<clsReaderThreadConfig>");

            sB.Append("<name>");
            sB.Append(name);
            sB.Append("</name>");

            sB.Append("<id>");
            sB.Append(id);
            sB.Append("</id>");

            sB.Append("<location>");
            sB.Append(location);
            sB.Append("</location>");

            sB.Append("<description>");
            sB.Append(description);
            sB.Append("</description>");

            sB.Append("<config>");
            sB.Append(config);
            sB.Append("</config>");

            sB.Append("<assembly>");
            sB.Append(assembly);
            sB.Append("</assembly>");

            sB.Append("<type>");
            sB.Append(type);
            sB.Append("</type>");

            sB.Append("<filterconfig>");
            sB.Append(filterconfig);
            sB.Append("</filterconfig>");

            sB.Append("<filterassembly>");
            sB.Append(filterassembly);
            sB.Append("</filterassembly>");

            sB.Append("<filtertype>");
            sB.Append(filtertype);
            sB.Append("</filtertype>");

            sB.Append("<controllerconfig>");
            sB.Append(controllerconfig);
            sB.Append("</controllerconfig>");

            sB.Append("<controllerassembly>");
            sB.Append(controllerassembly);
            sB.Append("</controllerassembly>");

            sB.Append("<controllertype>");
            sB.Append(controllertype);
            sB.Append("</controllertype>");

            sB.Append("<readerip>");
            sB.Append(readerip);
            sB.Append("</readerip>");
            
            sB.Append("</clsReaderThreadConfig>");
            return sB.ToString();
        }

    }
}
