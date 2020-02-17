using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WCFTestApp
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lstReaders.Items.Clear();
            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();
            foreach (string s in oClient.GetReaders().ToList())
            {
                lstReaders.Items.Add(s);
            }
            oClient.Close();
        }

        private void btnSetResultRequestType_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            if (lstResultRequestType.Text == "Szűrt")
            {
                VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
                oClient.Open();
                
                string sError = "";
                
                oClient.SetResultRequestType(lstReaders.Text, VRHWCFAppInterface.eControllerResultRequestType.Filtered,ref sError);
                
                if (!string.IsNullOrEmpty(sError))
                    MessageBox.Show(sError);

                oClient.Close();
            }
            else if (lstResultRequestType.Text == "Mind")
            {
                VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
                oClient.Open();
                string sError = "";
                oClient.SetResultRequestType(lstReaders.Text, VRHWCFAppInterface.eControllerResultRequestType.Unfiltered,ref sError);

                if (!string.IsNullOrEmpty(sError))
                    MessageBox.Show(sError);

                oClient.Close();
            }
            else if (lstResultRequestType.Text == "Semmi")
            {
                VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
                oClient.Open();
                string sError = "";
                oClient.SetResultRequestType(lstReaders.Text, VRHWCFAppInterface.eControllerResultRequestType.NoResult, ref sError);

                if (!string.IsNullOrEmpty(sError))
                    MessageBox.Show(sError);

                oClient.Close();
            }
        }

        private void btnGetResults_Click(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(lstReaders.Text))
            //    return;

            List<VRHWCFAppInterface.clsReadResult> colReadResults;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();
            string sError = "";
            colReadResults = oClient.GetResults("#MIND#",ref sError).ToList(); //(lstReaders.Text)

            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);
            
            oClient.Close();

            if (colReadResults != null)
            {
                foreach (VRHWCFAppInterface.clsReadResult oResult in colReadResults)
                {
                    lstResults.Items.Add(oResult.oReaderThreadConfig.id + ": "+ oResult.sResult +" : " + oResult.sOriginalResult);
                }

                if (lstResults.Items.Count > 0)
                    lstResults.SelectedIndex = lstResults.Items.Count - 1;
            }

        }

        private void lstReaders_MouseClick(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();

            string sError = "";

            VRHWCFAppInterface.eControllerResultRequestType eResultRequestType = oClient.GetResultRequestType(sReaderId , ref sError);

            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            switch (eResultRequestType)
            {
                case VRHWCFAppInterface.eControllerResultRequestType.Filtered:
                    lstResultRequestType.Text = "Szűrt";
                    break;
                case VRHWCFAppInterface.eControllerResultRequestType.Unfiltered:
                    lstResultRequestType.Text = "Mind";
                    break;
                default:
                    lstResultRequestType.Text = "Semmi";
                    break;
            }

            sError = "";

            int iCycle = oClient.GetCycle(sReaderId,ref sError);

            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            edtCycle.Text = iCycle.ToString();

            sError = "";
            int iTimeoutMode = oClient.GetTimeoutMode(sReaderId, ref sError);

            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            if (iTimeoutMode == 1)
                chkTimeoutMode.Checked = true;
            else
                chkTimeoutMode.Checked = false;
            
            oClient.Close();

        }

        private void btnSetCycle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();
            int iCycle = 0;
            int.TryParse(edtCycle.Text, out iCycle);

            string sError = "";

            oClient.SetCycle(sReaderId, iCycle, ref sError);

            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            int iTimeoutMode = 0;
            if (chkTimeoutMode.Checked == true)
            {
                iTimeoutMode = 1;
            }

            sError = "";
            oClient.SetTimeoutMode(sReaderId, iTimeoutMode, ref sError);

            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            oClient.Close();

        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();
            int iTimeout = 10;
            int.TryParse(edtReadTimeout.Text, out iTimeout);
            oClient.Read(sReaderId, iTimeout,60);
            oClient.Close();

        }

        System.Threading.Thread ResultThread = null;

        private void thrResultThread()
        {
            List<VRHWCFAppInterface.clsReadResult> colReadResults;

            string sConectString = "";
            sConectString = edtConnectString.Text;

            while (true)
            {
                VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", sConectString);
                oClient.Open();
                string sError = "";
                colReadResults = oClient.GetResults("#MIND#", ref sError).ToList();
                if (!string.IsNullOrEmpty(sError))
                    MessageBox.Show(sError);
                oClient.Close();

                if (colReadResults != null)
                {
                    lstResults.BeginInvoke(
                            (MethodInvoker)delegate()
                            {
                                foreach (VRHWCFAppInterface.clsReadResult oResult in colReadResults)
                                {
                                    lstResults.Items.Add(oResult.oReaderThreadConfig.id + ": " + oResult.sResult + " : " + oResult.sOriginalResult);
                                }

                                if (lstResults.Items.Count > 0)
                                    lstResults.SelectedIndex = lstResults.Items.Count - 1;
                            }
                        );
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private void chkBoxResultOnThread_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBoxResultOnThread.Checked == true)
            {
                if(ResultThread == null)
                {
                    ResultThread = new System.Threading.Thread(thrResultThread);
                    ResultThread.Start();
                }
            }
            else
            {
                if(ResultThread != null)
                {
                    ResultThread.Abort();
                    ResultThread = null;
                }
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ResultThread != null)
            {
                ResultThread.Abort();
                ResultThread = null;
            }
        }

        private void btnExecuteCommandSet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();
            oClient.ExecuteReaderCommands(sReaderId,edtConfigFile.Text,edtCommandSet.Text, 60);
            oClient.Close();

        }

        private void btnExecuteCommand_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();
            oClient.ExecuteReaderCommand(sReaderId, edtCommand.Text, 60);
            oClient.Close();


        }

        private string GetSubResultValue(VRHWCFAppInterface.clsReadResult oReadResult, string item)
        {
            string subresultvalue = "";

            if (oReadResult.colSubResults != null)
            {
                foreach (VRHWCFAppInterface.clsReadSubResult oSubResult in oReadResult.colSubResults)
                {
                    if (string.Compare(oSubResult.name, item, true) == 0)
                    {
                        subresultvalue = oSubResult.value;
                        break;
                    }

                }
            }

            return subresultvalue;
        }

        void StartRead()
        {
            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();

            string sError = "";

            if (oClient.GetTimeoutMode(sReaderId, ref sError) != 1)
                oClient.SetTimeoutMode(sReaderId, 1, ref sError); //Timeout üzemmód beállítása

            if (oClient.GetCycle(sReaderId, ref sError) != -1)
                oClient.SetCycle(sReaderId, -1, ref sError); //Folyamatos olvasás

            if (oClient.GetResultRequestType(sReaderId, ref sError) != VRHWCFAppInterface.eControllerResultRequestType.Unfiltered)
                oClient.SetResultRequestType(sReaderId, VRHWCFAppInterface.eControllerResultRequestType.Unfiltered, ref sError); //Filterezés előtt jöjjön át minden adat

            sError = "";
            oClient.GetResults(sReaderId, ref sError); //Az esetleg menet közben beérkezett adatok kilvasása a semmibe
            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            string sCommand = "sMN MIStartIn";
            oClient.ExecuteReaderCommand(sReaderId, sCommand, 60);

            oClient.Close();
        }

        void StopRead()
        {
            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();

            string sCommand = "sMN MIStopIn";
            oClient.ExecuteReaderCommand(sReaderId, sCommand, 60);

            oClient.Close();
        }

        void GetResults()
        {
            List<VRHWCFAppInterface.clsReadResult> colReadResults;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();

            string sError = "";

            colReadResults = oClient.GetResults("#MIND#", ref sError).ToList(); //Eredmények elkérése az összes lehetséges olvasóról, vagy a #MIND# helyett a konkrét olvasó kell, ha csak egyről akarjuk
            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            if (colReadResults != null)
            {
                foreach (VRHWCFAppInterface.clsReadResult oResult in colReadResults)
                {
                    if (oResult.eResultType == VRHWCFAppInterface.eReadResultType.Data) //Csak az adat típusú eredmények érdekelnek minket ebben a pillanatban
                    {
                        string sInfo = oResult.sResult + " rssi:" + GetSubResultValue(oResult, "rssi") + " count:" + GetSubResultValue(oResult, "count");
                    }
                }
            }

            oClient.Close();

        }

        private void btnReadAndGetSample_Click(object sender, EventArgs e)
        {
            List<VRHWCFAppInterface.clsReadResult> colReadResults;

            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();

            string sError = "";

            if (oClient.GetTimeoutMode(sReaderId, ref sError) != 1)
                oClient.SetTimeoutMode(sReaderId, 1, ref sError); //Timeout üzemmód beállítása
            
            if (oClient.GetCycle(sReaderId, ref sError) != -1)
                oClient.SetCycle(sReaderId, -1, ref sError); //Folyamatos olvasás

            if (oClient.GetResultRequestType(sReaderId, ref sError) != VRHWCFAppInterface.eControllerResultRequestType.Unfiltered)
                oClient.SetResultRequestType(sReaderId, VRHWCFAppInterface.eControllerResultRequestType.Unfiltered, ref sError); //Filterezés előtt jöjjön át minden adat

            sError = "";
            oClient.GetResults(sReaderId, ref sError); //Az esetleg menet közben beérkezett adatok kilvasása a semmibe
            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            int iReadSeconds = 5; //Öt másodpercig olvassunk
            int iWaitForReadCommandExecutionSecons = 60; //Az olvasási parancs végrehajtására várjunk 60 másodpercet

            oClient.Read(sReaderId, iReadSeconds, iWaitForReadCommandExecutionSecons); //Olvasás indítása

            System.Threading.Thread.Sleep(iReadSeconds * 1000); //Várakozás

            sError = "";
            colReadResults = oClient.GetResults(sReaderId, ref sError).ToList(); //Eredmények elkérése
            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            if (colReadResults != null)
            {
                foreach (VRHWCFAppInterface.clsReadResult oResult in colReadResults)
                {
                    if (oResult.eResultType == VRHWCFAppInterface.eReadResultType.Data) //Csak az adat típusú eredmények érdekelnek minket ebben a pillanatban
                    {
                        string sInfo = oResult.sResult + " rssi:" + GetSubResultValue(oResult,"rssi") + " count:" + GetSubResultValue(oResult,"count");
                        lstResults.Items.Add(sInfo);
                        if (lstResults.Items.Count > 0)
                             lstResults.SelectedIndex = lstResults.Items.Count - 1;
                    }
                }
            }
            
            oClient.Close();

        }

        private void btnReadAndGetSample2_Click(object sender, EventArgs e)
        {
            List<VRHWCFAppInterface.clsReadResult_RID_TAGID_COUNT_RSSI> colReadResults;

            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();

            string sError = "";

            if (oClient.GetTimeoutMode(sReaderId, ref sError) != 1)
                oClient.SetTimeoutMode(sReaderId, 1, ref sError); //Timeout üzemmód beállítása

            if (oClient.GetCycle(sReaderId, ref sError) != -1)
                oClient.SetCycle(sReaderId, -1, ref sError); //Folyamatos olvasás

            if (oClient.GetResultRequestType(sReaderId, ref sError) != VRHWCFAppInterface.eControllerResultRequestType.Unfiltered)
                oClient.SetResultRequestType(sReaderId, VRHWCFAppInterface.eControllerResultRequestType.Unfiltered, ref sError); //Filterezés előtt jöjjön át minden adat

            sError = "";
            oClient.GetResults(sReaderId, ref sError); //Az esetleg menet közben beérkezett adatok kilvasása a semmibe
            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            int iReadSeconds = 5; //Öt másodpercig olvassunk
            int iWaitForReadCommandExecutionSecons = 60; //Az olvasási parancs végrehajtására várjunk 60 másodpercet

            oClient.Read(sReaderId, iReadSeconds, iWaitForReadCommandExecutionSecons); //Olvasás indítása

            System.Threading.Thread.Sleep(iReadSeconds * 1000); //Várakozás

            sError = "";
            colReadResults = oClient.GetResults_RID_TAGID_COUNT_RSSI(sReaderId, ref sError).ToList(); //Eredmények elkérése
            if (!string.IsNullOrEmpty(sError))
                MessageBox.Show(sError);

            if (colReadResults != null)
            {
                foreach (VRHWCFAppInterface.clsReadResult_RID_TAGID_COUNT_RSSI oResult in colReadResults)
                {
                    if (oResult.eResultType == VRHWCFAppInterface.eReadResultType.Data) //Csak az adat típusú eredmények érdekelnek minket ebben a pillanatban
                    {
                        string sInfo = oResult.sTagId + " reader:" + oResult.sReaderId  + " rssi:" + oResult.dRssi.ToString() + " count:" + oResult.iCount.ToString();
                        lstResults.Items.Add(sInfo);
                        if (lstResults.Items.Count > 0)
                            lstResults.SelectedIndex = lstResults.Items.Count - 1;
                    }
                }
            }

            oClient.Close();

        }

        private void bReadIF2Inputs_Click(object sender, EventArgs e)
        {
            List<VRHWCFAppInterface.clsReadResult_RID_TAGID_COUNT_RSSI> colReadResults;

            if (string.IsNullOrEmpty(lstReaders.Text))
                return;

            string sReaderId = lstReaders.Text;

            VRHWCFAppInterface.AppInterfaceClient oClient = new VRHWCFAppInterface.AppInterfaceClient("BasicHttpBinding_IAppInterface", edtConnectString.Text);
            oClient.Open();

            string sError = "";
            bool bInputA = false;
            bool bInputB = false;
            bool bInputC = false;
            bool bInputD = false;

            sError = oClient.READGPI_IF2(sReaderId, 60, ref bInputA, ref bInputB, ref bInputC, ref bInputD);

            if (string.IsNullOrEmpty (sError))
            {
                MessageBox.Show("InputA: " + bInputA.ToString() + "\nInputB: " + bInputB.ToString() + "\nInputC: " + bInputC.ToString() + "\nInputD: " + bInputD.ToString());
            }
            else
            {
                MessageBox.Show(sError);
            }

            oClient.Close();

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            StartRead();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopRead();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GetResults();
        }


    }
}
