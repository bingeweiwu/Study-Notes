using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class SulpHurServiceHost : ServiceHost
    {
        public SulpHurServiceHost(RuleManager ruleManager, UIContentVerification verifyThread, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {

            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new SulpHurInstanceProvider(ruleManager,verifyThread));
            }
        }
    }

    public class SulpHurInstanceProvider : IInstanceProvider, IContractBehavior
    {
        RuleOperations ruleManager;
        VerifyOperations verifyThread;

        public SulpHurInstanceProvider(RuleOperations ruleManager, VerifyOperations verifyThread)
        {
            this.ruleManager = ruleManager;
            this.verifyThread = verifyThread;
        }
        public object GetInstance(InstanceContext instanceContext, System.ServiceModel.Channels.Message message)
        {
            return this.GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return new SulpHurService.SulpHurWCFService(this.ruleManager, this.verifyThread);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
    }
}
