using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkIF2
{
    class clsReadParameter
    {
        public string name;
        public string value;
        public int result;
        public int resultonly;
    }

    class clsAttrib
    {
        public string attrib;
    }

    class clsInitCommand
    {
        public string initcommand;
    }

    class clsReaderConfig : VRHReaderFrameworkCommon.clsReaderConfigBase
    {
        public int port;
        public int readmode;
        public int processtagevtastagread;
        public int discardevtwithrderr;
        public int maxpollresults;
        public string readstartcommand;
        public string readpollcommand;
        public string readstopcommand;
        public System.Collections.Generic.List<clsReadParameter> colReadParameters;
        public System.Collections.Generic.List<clsAttrib> colAttribs;
        public System.Collections.Generic.List<clsInitCommand> colInitCommands;
    }
}
