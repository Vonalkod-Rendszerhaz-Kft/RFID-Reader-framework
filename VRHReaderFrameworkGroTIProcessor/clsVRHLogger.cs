using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace VRHReaderFrameworkGroTIProcessor
{
    internal class clsVRHLoggerConfig
    {
        public string SQLConnectString;
    }

    public class clsVRHLogger : VRHReaderFrameworkCommon.clsProcessorBase
    {
        private clsVRHLoggerConfig oConfig = null;

        public override void LoadConfig(string name)
        {
            oConfig = new clsVRHLoggerConfig();

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
            }
            oXmlDoc = null;
        }

        public override List<VRHReaderFrameworkCommon.clsAction> Process(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            try
            {
                SqlConnection oConnection;
                oConnection = new SqlConnection(oConfig.SQLConnectString);
                oConnection.Open();

                try
                {
                    SqlCommand oCommand = new SqlCommand("INSERT INTO RFID_NAPLO (TIME,PC_IP,RFID_GATE_IP,RFID_GATE_ID,TYPE,DATA1,DATA2) VALUES (@TIME,@PC_IP,@RFID_GATE_IP,@RFID_GATE_ID,@TYPE,@DATA1,@DATA2)",oConnection);

                    SqlParameter oParameter = null;

                    oParameter = new SqlParameter("@TIME",System.Data.SqlDbType.DateTime);
                    oParameter.Value = oReadResult.dtRead;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@PC_IP",System.Data.SqlDbType.NVarChar);
                    oParameter.Value = Environment.MachineName;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@RFID_GATE_IP" , System.Data.SqlDbType.NVarChar);
                    oParameter.Value = oReadResult.oReaderThreadConfig.readerip;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@RFID_GATE_ID",System.Data.SqlDbType.NVarChar);
                    oParameter.Value = oReadResult.oReaderThreadConfig.id;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@TYPE",System.Data.SqlDbType.Int);
                    switch (oReadResult.eResultType)
                    {
                        case VRHReaderFrameworkCommon.eReadResultType.DataEvent:
                        case VRHReaderFrameworkCommon.eReadResultType.Data:
                            oParameter.Value = 0;
                            break;
                        case VRHReaderFrameworkCommon.eReadResultType.Event:
                            oParameter.Value = 1;
                            break;
                        case VRHReaderFrameworkCommon.eReadResultType.Failure:
                            oParameter.Value = 100;
                            break;
                        case VRHReaderFrameworkCommon.eReadResultType.Log:
                            oParameter.Value = 2;
                            break;
                    }
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@DATA1",System.Data.SqlDbType.NVarChar);
                    oParameter.Value = oReadResult.sResult;
                    oCommand.Parameters.Add(oParameter);

                    oParameter = new SqlParameter("@DATA2",System.Data.SqlDbType.NVarChar);

                    string subresults = "";
                    if (oReadResult.colSubResults != null)
                    {
                        foreach (VRHReaderFrameworkCommon.clsReadSubResult oSubResult in oReadResult.colSubResults)
                        {
                            subresults +=
                                "<" + oSubResult.name + ">"
                                + oSubResult.value
                                + "</" + oSubResult.name + ">";
                        }
                    }
                    oParameter.Value = subresults;
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

            return null;
        }
    }
}
