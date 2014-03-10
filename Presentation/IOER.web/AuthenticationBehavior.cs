using System;
using System.Net;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ILPathways
{
    public class ContextPusher : IClientMessageInspector
    {
        public static string configUsername = "testuser";
        public static string configPassword = "testpassword";

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

            // Do nothing.

        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            
            MessageHeader userHeader= MessageHeader.CreateHeader("username", "ns", configUsername);
            MessageHeader passwordHeader = MessageHeader.CreateHeader("password", "ns", configPassword);

            request.Headers.Add(userHeader);
            request.Headers.Add(passwordHeader);

            return null;

        }

    }

    public class AuthenticationBehavior : Attribute, IOperationBehavior, IContractBehavior,IEndpointBehavior
    {

        #region IOperationBehavior Members

        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {

        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {

            clientOperation.Parent.MessageInspectors.Add(new ContextPusher());

        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {

            //throw new NotImplementedException();

        }

        void IOperationBehavior.Validate(OperationDescription operationDescription)
        {

            // throw new NotImplementedException();

        }

        #endregion

        #region IContractBehavior Members

        void IContractBehavior.AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {

            clientRuntime.MessageInspectors.Add(new ContextPusher());

        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {

            //throw new NotImplementedException();

        }

        void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {

            //throw new NotImplementedException();

        }

        #endregion

        #region IEndpointBehavior Members
        
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            //throw new NotImplementedException();
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new ContextPusher());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            //throw new NotImplementedException();
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }

}
