using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Log4Pro.IS.TRM.EventHubContract;
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;
using VRHReaderFrameworkCommon.Convert;

namespace IslandSystemProcessor
{
    public class WRCVProcessor : VRHReaderFrameworkCommon.clsProcessorBase
    {
        public override void LoadConfig(string name)
        {

        }

        public override List<VRHReaderFrameworkCommon.clsAction> Process(ref VRHReaderFrameworkCommon.clsReadResult oReadResult)
        {
            var colRet = new List<VRHReaderFrameworkCommon.clsAction>();

            try
            {
                if (oReadResult != null)
                {
                    if (oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.Data || oReadResult.eResultType == VRHReaderFrameworkCommon.eReadResultType.DataEvent)
                    {
                        var epc = VRHReaderFrameworkCommon.Convert.Convert.ConvertHexToAscii(oReadResult.sResult.Substring(4, oReadResult.sResult.Length - 4));
                        epc = epc.TrimEnd('\0');
                        VRHReaderFrameworkCommon.clsLogger.Debug(epc);

                        TrackingContract.ReceivingModule.ReceiveResponse response = null;
                        var request = new TrackingContract.ReceivingModule.ReceiveRequest()
                        {
                            ShippingUnitId = epc
                        };
                        try
                        {
                            response = EventHubCore.Call<RedisPubSubChannel,
                                TrackingContract.ReceivingModule.ReceiveRequest,
                                TrackingContract.ReceivingModule.ReceiveResponse>($"{TrackingContract.CHANNEL_PREFIX}:{TrackingContract.ReceivingModule.MODULE_PREFIX}:demo", request);
                        }
                        catch (Exception ex)
                        {
                            VRHReaderFrameworkCommon.clsLogger.Fatal(ex.Message, ex);
                            throw;
                        }
                    }

                    oReadResult.eAppProcessingStatus = VRHReaderFrameworkCommon.eReadResultProcessingStatus.Processed;
                }
            }
            catch (Exception e)
            {
                VRHReaderFrameworkCommon.clsLogger.Fatal(e.Message, e);
                throw;
            }

            return colRet;
        }
    }
}
