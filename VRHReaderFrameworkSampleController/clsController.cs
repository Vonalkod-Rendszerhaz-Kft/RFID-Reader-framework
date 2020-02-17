using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace VRHReaderFrameworkSampleController
{
    public class clsController : VRHReaderFrameworkCommon.clsControllerBase
    {
        #region "NET"

        private volatile bool bStopNET = false;
        Socket oListenSocket = null;

        private void ListenerThread()
        {
            try
            {
                //IPAddress ipAddress = new IPAddress(0);
                //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 6666);
                IPAddress ipAddress = IPAddress.Parse("192.168.0.200");
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 2112);

                oListenSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    oListenSocket.Bind(localEndPoint);
                    oListenSocket.Listen(10);

                    while (!bStopNET)
                    {
                        Socket oSocket = oListenSocket.Accept();
                        System.Threading.Thread thrHandler = new System.Threading.Thread(HandlerThread);
                        thrHandler.Start(oSocket);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                try
                {
                    oListenSocket.Close();
                    oListenSocket = null;
                }
                catch { }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void HandlerThread(object oParam)
        {
            Socket oSocket = (Socket)oParam;
            string command = "";
            try
            {
                oSocket.ReceiveTimeout = 100;
                DateTime dtLastReceive = DateTime.Now;
                while (!bStopNET)
                {
                    byte[] bRec = new byte[1];
                    try
                    {
                        if (oSocket.Receive(bRec) == 1)
                        {
                            dtLastReceive = DateTime.Now;
                            command += System.Text.Encoding.ASCII.GetString(bRec);
                        }
                    }
                    catch (System.Net.Sockets.SocketException sockEx)
                    {
                        if (sockEx.SocketErrorCode == SocketError.TimedOut)
                        {
                            if ((DateTime.Now - dtLastReceive).TotalSeconds > 600)
                                throw;

                            string sSend = "";
                            lock (oResultLockObject)
                            {
                                foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colGlobalReadResults)
                                {
                                    sSend += oResult.eResultType.ToString() + " : " + oResult.sResult + "\r\n";
                                }
                                colGlobalReadResults.Clear();
                            }
                            if (!string.IsNullOrEmpty(sSend))
                            {
                                byte[] bSend = System.Text.Encoding.ASCII.GetBytes(sSend);
                                oSocket.Send(bSend);
                            }

                        }
                        else
                        {
                            throw;
                        }
                    }

                    if (command.EndsWith("\r"))
                    {
                        command = command.Replace("\r", "").Replace("\n", "");

                        if (string.Compare(command,"hold",true) == 0)
                        {
                            lock (oCycleLockObject)
                            {
                                iCycle = 0;
                            }
                        }
                        else if (string.Compare(command,"poll",true) == 0)
                        {
                            lock(oCycleLockObject)
                            {
                                iCycle = -1;
                            }
                        }
                        else if (string.Compare(command,"read",true) == 0)
                        {
                            lock (oCycleLockObject)
                            {
                                iCycle = 1;
                            }
                        }
                        else if (string.Compare(command, "readgpi", true) == 0)
                        {
                            lock (oActionLockObject)
                            {
                                {
                                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                    oAction.dtAction = DateTime.Now;
                                    oAction.dtValidAfter = DateTime.Now;
                                    oAction.iAction = 1;
                                    oAction.uidReader = Guid.Empty;
                                    oAction.uidAction = Guid.Empty;
                                    oAction.uidProcessor = Guid.Empty;
                                    oAction.colActionParameters = new List<string>();
                                    oAction.colActionParameters.Add("READGPI");
                                    colGlobalActions.Add(oAction);
                                }
                            }
                        }
                        else if (string.Compare(command, "off", true) == 0)
                        {
                            lock (oActionLockObject)
                            {
                                {
                                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                    oAction.dtAction = DateTime.Now;
                                    oAction.dtValidAfter = DateTime.Now;
                                    oAction.iAction = 4;
                                    oAction.uidReader = Guid.Empty;
                                    oAction.uidAction = Guid.Empty;
                                    oAction.uidProcessor = Guid.Empty;
                                    oAction.colActionParameters = new List<string>();
                                    oAction.colActionParameters.Add("");
                                    colGlobalActions.Add(oAction);
                                }
                            }
                        }
                        else if (string.Compare(command, "on", true) == 0)
                        {
                            lock (oActionLockObject)
                            {
                                {
                                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                    oAction.dtAction = DateTime.Now;
                                    oAction.dtValidAfter = DateTime.Now;
                                    oAction.iAction = 3;
                                    oAction.uidReader = Guid.Empty;
                                    oAction.uidAction = Guid.Empty;
                                    oAction.uidProcessor = Guid.Empty;
                                    oAction.colActionParameters = new List<string>();
                                    oAction.colActionParameters.Add("");
                                    colGlobalActions.Add(oAction);
                                }
                            }
                        }
                        else if (string.Compare(command, "reset", true) == 0)
                        {
                            lock (oActionLockObject)
                            {
                                {
                                    VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                                    oAction.dtAction = DateTime.Now;
                                    oAction.dtValidAfter = DateTime.Now;
                                    oAction.iAction = 5;
                                    oAction.uidReader = Guid.Empty;
                                    oAction.uidAction = Guid.Empty;
                                    oAction.uidProcessor = Guid.Empty;
                                    oAction.colActionParameters = new List<string>();
                                    oAction.colActionParameters.Add("");
                                    colGlobalActions.Add(oAction);
                                }
                            }
                        }
                        else if (string.Compare(command, "help", true) == 0)
                        {
                            string sSend = "";
                            sSend += "hold - disable read poll cycles\r\n";
                            sSend += "poll - continious read poll cycles\r\n";
                            sSend += "read - make a sinle read poll cycle\r\n";
                            sSend += "readgpi - read general input pins\r\n";
                            sSend += "on - executes readstart command on the reader\r\n";
                            sSend += "off - executes readstop command on the reader\r\n";
                            sSend += "reset - executes reset command on the reader\r\n        you should reconnect after about few minutes\r\n";
                            if (!string.IsNullOrEmpty(sSend))
                            {
                                byte[] bSend = System.Text.Encoding.ASCII.GetBytes(sSend);
                                oSocket.Send(bSend);
                            }

                        }

                        command = "";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            try
            {
                oSocket.Close();
                oSocket = null;
            }
            catch{ }
        }

        #endregion

        private int iCycle;
        private object oCycleLockObject;
        private System.Threading.Thread thrListener;
        private System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult> colGlobalReadResults;
        private System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction> colGlobalActions;
        private object oResultLockObject;
        private object oActionLockObject;

        public override System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction> GetControllerActions()
        {
            System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();

            lock(oActionLockObject)
            {
                foreach(VRHReaderFrameworkCommon.clsAction oAction in colGlobalActions)
                {
                    colRet.Add(oAction);
                }
                colGlobalActions.Clear();
            }

            return colRet;
        }

        public override void StartController()
        {
            oActionLockObject = new object();
            oCycleLockObject = new object();
            iCycle = 0;

            oResultLockObject = new object();

            colGlobalReadResults = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsReadResult>();
            colGlobalActions = new System.Collections.Generic.List<VRHReaderFrameworkCommon.clsAction>();

            thrListener = new System.Threading.Thread(ListenerThread);
            thrListener.Start();

        }

        public override void StopController()
        {
            bStopNET = true;
            try
            {
                if (oListenSocket != null)
                    oListenSocket.Close();
            }
            catch { }

            System.Threading.Thread.Sleep(1000);

            thrListener.Abort();
            thrListener = null;

            oActionLockObject = null;
            oResultLockObject = null;
            oCycleLockObject = null;
        }

        public override void LoadConfig(string name, string basedir, VRHReaderFrameworkCommon.clsReaderThreadConfig oRederThreadConfig)
        {
        }

        public override int GetCycle()
        {
            int iRet = 0;

            lock (oCycleLockObject)
            {
                iRet = iCycle;
            }

            return iRet;
        }

        public override void CycleDone()
        {
            lock (oCycleLockObject)
            {
                if (iCycle > 0)
                    iCycle--;
            }
        }

        public override void SetResults(List<VRHReaderFrameworkCommon.clsReadResult> colReadResults)
        {
            if (colReadResults != null)
            {
                if (colReadResults.Count > 0)
                {
                    lock(oResultLockObject)
                    {
                        foreach (VRHReaderFrameworkCommon.clsReadResult oResult in colReadResults)
                        {
                            colGlobalReadResults.Add(oResult);
                        }
                    }
                }
            }
        }

        public override VRHReaderFrameworkCommon.eControllerResultRequestType GetResultRequestType()
        {
            return VRHReaderFrameworkCommon.eControllerResultRequestType.Unfiltered;
        }
    }
}
