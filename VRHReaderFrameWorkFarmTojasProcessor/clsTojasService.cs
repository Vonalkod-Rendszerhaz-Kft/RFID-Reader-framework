using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameWorkFarmTojasProcessor
{
    public class clsTojasService : VRHReaderFrameworkCommon.clsProcessorBase
    {
        public override void LoadConfig(string name)
        {
        }

        public override List<VRHReaderFrameworkCommon.clsAction> Process(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            List<VRHReaderFrameworkCommon.clsAction> colRet = new List<VRHReaderFrameworkCommon.clsAction>();

            try
            {
                if (oReadResult != null)
                {
                    if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data || oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent)
                    {
                        TojasServiceWCF.RfidGatewayServiceClient oTojasService = new TojasServiceWCF.RfidGatewayServiceClient();
                        oTojasService.Open();
                        try
                        {
                            oTojasService.TermelesbeAd(oReadResult.sResult/*, oReadResult.dtRead*/);
                            VRHReaderFrameworkCommon.clsLogger.Info("Termelésbe adva: " + oReadResult.sResult);
                        }
                        catch (System.ServiceModel.FaultException fe)
                        {
                            VRHReaderFrameworkCommon.clsLogger.Info("Nincsen termelésbe adva: " + oReadResult.sResult + " Oka: " + fe.Message);
                        }
                        oTojasService.Close();
                    }
                }
                oReadResult.eAppProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.Processed;
            }
            catch(Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                throw;
            }

            return colRet;
        }
    }
}
