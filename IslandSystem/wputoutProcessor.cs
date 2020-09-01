using Log4Pro.IS.TRM.EventHubContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;

namespace IslandSystemProcessor
{
    class WPUTOUTProcessor : VRHReaderFrameworkCommon.clsProcessorBase
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
						Console.WriteLine(epc);
                        VRHReaderFrameworkCommon.clsLogger.Debug(epc);

                        TrackingContract.Response response = null;
                        var request = new TrackingContract.PutOutModule.PutOutRequest()
                        {
                            PackagingUnitId = epc
                        };
                        try
                        {
                            response = EventHubCore.Call<RedisPubSubChannel,
                                TrackingContract.PutOutModule.PutOutRequest,
                                TrackingContract.Response>($"{TrackingContract.CHANNEL_PREFIX}:{TrackingContract.PutOutModule.MODULE_PREFIX}:demo", request);
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
