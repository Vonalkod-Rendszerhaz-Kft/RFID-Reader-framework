using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace VRHReaderFrameworkGroTIProcessor
{
    internal class clsGateKeeperConfig
    {
        public string SQLConnectString;
        public System.Collections.Generic.List<clsIdConverter> colIdConverters;
        public System.Collections.Generic.List<clsTargetConverter> colTargetConverters;
        public System.Collections.Generic.List<clsCommand> colSuccessCommands;
        public System.Collections.Generic.List<clsCommand> colWarningCommands;
        public System.Collections.Generic.List<clsCommand> colFailureCommands;
    }

    internal class clsIdConverter
    {
        public string id;
        public string antenna;
        public string target;
    }

    internal class clsTargetConverter
    {
        public string id;
        public string target;
    }

    internal class clsCommand
    {
        public string command;
        public int delaysec;
    }

    public class clsGateKeeper : VRHReaderFrameworkCommon.clsProcessorBase
    {
        clsGateKeeperConfig oConfig = null;

        public override void LoadConfig(string name)
        {
            oConfig = new clsGateKeeperConfig();
            oConfig.colIdConverters = new List<clsIdConverter>();
            oConfig.colTargetConverters = new List<clsTargetConverter>();
            oConfig.colSuccessCommands = new List<clsCommand>();
            oConfig.colWarningCommands = new List<clsCommand>();
            oConfig.colFailureCommands = new List<clsCommand>();

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
                if (string.Compare(node.Name, "SQLConnectString", true) == 0)
                {
                    oConfig.SQLConnectString = node.InnerText;
                }
                else if (string.Compare(node.Name, "idconverter", true) == 0)
                {
                    clsIdConverter oIdConverter = new clsIdConverter();
                    foreach (System.Xml.XmlNode subnode in node.ChildNodes)
                    {
                        if (string.Compare(subnode.Name, "id", true) == 0)
                        {
                            oIdConverter.id = subnode.InnerText;
                        }
                        else if (string.Compare(subnode.Name, "antenna", true) == 0)
                        {
                            oIdConverter.antenna = subnode.InnerText;
                        }
                        else if (string.Compare(subnode.Name, "target", true) == 0)
                        {
                            oIdConverter.target = subnode.InnerText;
                        }
                    }
                    oConfig.colIdConverters.Add(oIdConverter);
                }
                else if (string.Compare(node.Name, "targetconverter", true) == 0)
                {
                    clsTargetConverter oTargetConverter = new clsTargetConverter();
                    foreach (System.Xml.XmlNode subnode in node.ChildNodes)
                    {
                        if (string.Compare(subnode.Name, "id", true) == 0)
                        {
                            oTargetConverter.id = subnode.InnerText;
                        }
                        else if (string.Compare(subnode.Name, "target", true) == 0)
                        {
                            oTargetConverter.target = subnode.InnerText;
                        }
                    }
                    oConfig.colTargetConverters.Add(oTargetConverter);
                }
                else if (string.Compare(node.Name, "successcommand", true) == 0)
                {
                    clsCommand oCommand = new clsCommand();
                    foreach (System.Xml.XmlNode subnode in node.ChildNodes)
                    {
                        if (string.Compare(subnode.Name, "command", true) == 0)
                        {
                            oCommand.command = subnode.InnerText;
                        }
                        else if (string.Compare(subnode.Name, "delaysec", true) == 0)
                        {
                            oCommand.delaysec = int.Parse(subnode.InnerText);
                        }
                    }
                    oConfig.colSuccessCommands.Add(oCommand);
                }
                else if (string.Compare(node.Name, "warningcommand", true) == 0)
                {
                    clsCommand oCommand = new clsCommand();
                    foreach (System.Xml.XmlNode subnode in node.ChildNodes)
                    {
                        if (string.Compare(subnode.Name, "command", true) == 0)
                        {
                            oCommand.command = subnode.InnerText;
                        }
                        else if (string.Compare(subnode.Name, "delaysec", true) == 0)
                        {
                            oCommand.delaysec = int.Parse(subnode.InnerText);
                        }
                    }
                    oConfig.colWarningCommands.Add(oCommand);
                }
                else if (string.Compare(node.Name, "failurecommand", true) == 0)
                {
                    clsCommand oCommand = new clsCommand();
                    foreach (System.Xml.XmlNode subnode in node.ChildNodes)
                    {
                        if (string.Compare(subnode.Name, "command", true) == 0)
                        {
                            oCommand.command = subnode.InnerText;
                        }
                        else if (string.Compare(subnode.Name, "delaysec", true) == 0)
                        {
                            oCommand.delaysec = int.Parse(subnode.InnerText);
                        }
                    }
                    oConfig.colFailureCommands.Add(oCommand);
                }
            }
            oXmlDoc = null;
            
        }

        private string GetAntennaID(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            string sRet = "1";
            if (oReadResult.colSubResults != null)
            {
                foreach (VRHReaderFrameworkCommon.clsReadSubResult oSubResult in oReadResult.colSubResults)
                {
                    if (string.Compare(oSubResult.name,"ant",true) == 0 || string.Compare(oSubResult.name,"antenna",true) == 0)
                    {
                        if (!string.IsNullOrEmpty(oSubResult.value))
                            sRet = oSubResult.value;

                        break;
                    }
                }
            }
            return sRet;
        }

        private string GetActionTargetId(string sSourceId)
        {
            string sRet = "";

            foreach (clsTargetConverter oTargetConverter in oConfig.colTargetConverters)
            {
                if (oTargetConverter.id == sSourceId)
                {
                    sRet = oTargetConverter.target;
                    break;
                }
            }

            return sRet;
        }

        private string GetTargetGateID(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            string sRet = "";

            //Alapból a GateID megegyezik a reader id -vel
            sRet = oReadResult.oReaderThreadConfig.id;

            foreach (clsIdConverter oConverter in oConfig.colIdConverters)
            {
                if (string.Compare(oConverter.id,oReadResult.oReaderThreadConfig.id) == 0)
                {
                    if (string.IsNullOrEmpty(oConverter.antenna))
                    { // Ha nincsen kitöltve az antenna dfefiníció, akkor simán átírjut a targetre.
                        sRet = oConverter.target;
                        break;
                    }
                    else
                    {
                        string sAntennaID = GetAntennaID(ref oReadResult);
                        if (string.Compare(sAntennaID,oConverter.antenna) == 0)
                        {
                            sRet = oConverter.target;
                            break;
                        }
                    }
                }
            }

            return sRet;
        }

        private int CanPassRFIDTheGate(string sTargetGateID, ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            int iRet = 0;
            try
            {
                SqlConnection oConnection;
                oConnection = new SqlConnection(oConfig.SQLConnectString);
                oConnection.Open();

                try
                {
                    SqlCommand oCommand = new SqlCommand(
                        "SELECT [RFID_ZONA_KAPU].[RFID_GATE_ID], [RFID_ZONA_KAPU].[ZONE_ID], [RFID_ZONA_BETEG].[USER_ID], [RFID_ZONA_BETEG].[RFID_TAG] FROM [RFID_ZONA_KAPU] INNER JOIN [RFID_ZONA_BETEG] ON [RFID_ZONA_KAPU].[ZONE_ID] = [RFID_ZONA_BETEG].[ZONE_ID] where [RFID_ZONA_BETEG].[RFID_TAG]=@RFID_TAG AND [RFID_ZONA_KAPU].[RFID_GATE_ID] = @RFID_GATE_ID"
                        , oConnection);

                    SqlParameter oParameter = null;

                    oParameter = new SqlParameter("@RFID_TAG", System.Data.SqlDbType.NVarChar);

                    string sReadResult = "";
                    if (!string.IsNullOrEmpty(oReadResult.sResult))
                    {
                        sReadResult = oReadResult.sResult.Substring(1);
                    }
                    oParameter.Value = sReadResult; //oReadResult.sResult;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@RFID_GATE_ID", System.Data.SqlDbType.BigInt);
                    oParameter.Value = Int64.Parse(sTargetGateID);
                    oCommand.Parameters.Add(oParameter);

                    SqlDataAdapter oAdapter = new SqlDataAdapter(oCommand);
                    
                    DataSet oDataset = new DataSet();

                    if (oAdapter.Fill(oDataset) > 0)
                    { // Ha kaptunk vissza sort, akkor létezik ez a kapcsolat
                        iRet = 1;
                    }

                    oDataset.Dispose();
                    oAdapter.Dispose();
                    oCommand.Dispose();



                }
                catch
                {
                    oConnection.Close();
                    oConnection.Dispose();
                    oConnection = null;
                    throw;
                }

                oConnection.Dispose();
                oConnection.Close();
                oConnection = null;
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                throw;
            }

            return iRet;
        }

        private void BuildActionCommands(ref List<VRHReaderFrameworkCommon.clsAction> colRet, ref System.Collections.Generic.List<clsCommand> colCommands,ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            string sActionTargetId = GetActionTargetId(oReadResult.oReaderThreadConfig.id);

            foreach(clsCommand oCommand in colCommands)
            {
                VRHReaderFrameworkCommon.clsAction oAction = new VRHReaderFrameworkCommon.clsAction();
                oAction.dtAction = DateTime.Now;
                oAction.dtValidAfter = DateTime.Now.AddSeconds(oCommand.delaysec);
                oAction.iAction = 2;
                if (string.IsNullOrEmpty(sActionTargetId) == true)
                {
                    oAction.uidReader = oReadResult.uidReader;
                    oAction.sTargetReaderId = "";
                }
                else
                {
                    oAction.uidAction = Guid.Empty;
                    oAction.sTargetReaderId = sActionTargetId;
                }
                oAction.uidAction = Guid.Empty;
                oAction.uidProcessor = Guid.Empty;
                oAction.colActionParameters = new List<string>();
                oAction.colActionParameters.Add(oCommand.command);
                colRet.Add(oAction);
            }
        }

        private void LogPassResult(int iPassResult, string sTargetGateID , ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            try
            {
                SqlConnection oConnection;
                oConnection = new SqlConnection(oConfig.SQLConnectString);
                oConnection.Open();

                try
                {
                    SqlCommand oCommand = new SqlCommand("INSERT INTO [RFID_RIASZTAS] ([REC_ID],[REC_TIME],[RFID_GATE_ID],[REC_STATUS],[RFID_ID],[STAMP_KI],[STAMP_IDOPONT],[STAMP_HOL]) VALUES (next value for RFID_OSZTO,@REC_TIME,@RFID_GATE_ID,@REC_STATUS,@RFID_ID,@STAMP_KI,@STAMP_IDOPONT,@STAMP_HOL)", oConnection);

                    SqlParameter oParameter = null;

                    oParameter = new SqlParameter("@REC_TIME", System.Data.SqlDbType.DateTime2);
                    oParameter.Value = oReadResult.dtRead;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@RFID_GATE_ID", System.Data.SqlDbType.BigInt);
                    oParameter.Value = Int64.Parse(sTargetGateID);
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@REC_STATUS", System.Data.SqlDbType.Int);
                    if (iPassResult == 1)
                    {
                        oParameter.Value = 0;
                    }
                    else
                    {
                        oParameter.Value = 1;
                    }
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@RFID_ID", System.Data.SqlDbType.NVarChar);
                    string sReadResult = "";
                    if (!string.IsNullOrEmpty(oReadResult.sResult))
                    {
                        sReadResult = oReadResult.sResult.Substring(1);
                    }
                    oParameter.Value = sReadResult; // oReadResult.sResult;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@STAMP_KI", System.Data.SqlDbType.BigInt);
                    oParameter.Value = 0;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@STAMP_IDOPONT", System.Data.SqlDbType.DateTime2);
                    oParameter.Value = DateTime.Now;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@STAMP_HOL", System.Data.SqlDbType.BigInt);
                    oParameter.Value = 0;
                    oCommand.Parameters.Add(oParameter);

                    oCommand.ExecuteNonQuery();
                    oCommand.Dispose();
                }
                catch
                {
                    oConnection.Close();
                    oConnection.Dispose();
                    oConnection = null;
                    throw;
                }

                oConnection.Dispose();
                oConnection.Close();
                oConnection = null;
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                throw;
            }
        }

        public override List<VRHReaderFrameworkCommon.clsAction> Process(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            if ((oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data || oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent) && oReadResult.eAppProcessingStatus != VRHReaderFrameworkCommon.eReadResultProcessingStatus.Processed)
            {
                List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();

                string sTargetGateID = GetTargetGateID(ref oReadResult);

                int iPassResult = CanPassRFIDTheGate(sTargetGateID, ref oReadResult);

                if (iPassResult == 1)
                { //Átmehet
                    BuildActionCommands(ref colRet, ref oConfig.colSuccessCommands, ref oReadResult);
                }
                else
                { //Nem mehet át
                    BuildActionCommands(ref colRet, ref oConfig.colFailureCommands,ref oReadResult);
                }

                LogPassResult(iPassResult, sTargetGateID, ref oReadResult);

                oReadResult.eAppProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.Processed;

                return colRet;
            }
            else
            {
                return null;
            }
        }
    }
}
