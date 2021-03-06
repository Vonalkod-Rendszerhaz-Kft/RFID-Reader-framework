//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VRHReaderFrameWorkSzapportaProcessor.SzapportaWS {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://szapporta.hu/RfidEventManagement", ConfigurationName="SzapportaWS.RfidReaderServices")]
    public interface RfidReaderServices {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://szapporta.hu/RfidEventManagement/RfidReaderServices/RegisterTagRead", ReplyAction="http://szapporta.hu/RfidEventManagement/RfidReaderServices/RegisterTagReadRespons" +
            "e")]
        void RegisterTagRead(string tagID, byte movementDirection, string readTime, string readerIpAddress);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://szapporta.hu/RfidEventManagement/RfidReaderServices/RegisterTagRead", ReplyAction="http://szapporta.hu/RfidEventManagement/RfidReaderServices/RegisterTagReadRespons" +
            "e")]
        System.Threading.Tasks.Task RegisterTagReadAsync(string tagID, byte movementDirection, string readTime, string readerIpAddress);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface RfidReaderServicesChannel : VRHReaderFrameWorkSzapportaProcessor.SzapportaWS.RfidReaderServices, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class RfidReaderServicesClient : System.ServiceModel.ClientBase<VRHReaderFrameWorkSzapportaProcessor.SzapportaWS.RfidReaderServices>, VRHReaderFrameWorkSzapportaProcessor.SzapportaWS.RfidReaderServices {
        
        public RfidReaderServicesClient() {
        }
        
        public RfidReaderServicesClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public RfidReaderServicesClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RfidReaderServicesClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RfidReaderServicesClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void RegisterTagRead(string tagID, byte movementDirection, string readTime, string readerIpAddress) {
            base.Channel.RegisterTagRead(tagID, movementDirection, readTime, readerIpAddress);
        }
        
        public System.Threading.Tasks.Task RegisterTagReadAsync(string tagID, byte movementDirection, string readTime, string readerIpAddress) {
            return base.Channel.RegisterTagReadAsync(tagID, movementDirection, readTime, readerIpAddress);
        }
    }
}
