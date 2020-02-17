using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRHReaderFrameworkCommon;

namespace VRHGateKeeperProcessor
{
    public class clsProcessor : VRHReaderFrameworkCommon.clsProcessorBase
    {
        override public void LoadConfig(string name)
        {

        }

        override public System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction> Process(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data)
            {
                System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now;
                    oAction.iAction = 1;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("READGPI");
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
                    oAction.colActionParameters.Add("WRITEGPO 1 ON");
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
                    oAction.colActionParameters.Add("WRITEGPO 1 OFF");
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
                    oAction.colActionParameters.Add("WRITEGPO 2 ON");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(2);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 2 OFF");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(2);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 3 ON");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(3);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 3 OFF");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(3);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 4 ON");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(4);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 4 OFF");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(4);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 3 ON");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(5);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 3 OFF");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(5);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 2 ON");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(6);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 2 OFF");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(6);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 1 ON");
                    colRet.Add(oAction);
                }

                {
                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                    oAction.dtAction = DateTime.Now;
                    oAction.dtValidAfter = DateTime.Now.AddSeconds(7);
                    oAction.iAction = 2;
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.uidAction = Guid.Empty;
                    oAction.uidProcessor = Guid.Empty;
                    oAction.colActionParameters = new List<string>();
                    oAction.colActionParameters.Add("WRITEGPO 1 OFF");
                    colRet.Add(oAction);
                }

                return colRet;
            }
            else if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Event)
            {

            }
            else if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.ActionResult)
            {

            }
            else if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Failure)
            {

            }
            else if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent)
            {

            }

            return null;
        }
    }
}
