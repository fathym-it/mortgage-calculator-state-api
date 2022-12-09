using Fathym;
using Fathym.LCU.Services.StateAPIs.Durable;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FinaTech.SensitivityModel.StateAPI.State
{
    public class MortgageStateActions : StateActions
    {
        #region Life Cycle
        [FunctionName(nameof(Negotiate))]
        public virtual Task<SignalRConnectionInfo> Negotiate(ILogger logger, [HttpTrigger(AuthorizationLevel.Anonymous, Route = "negotiate")] HttpRequestMessage req)
        {
            return negotiate(logger, req);
        }

        [FunctionName(nameof(OnConnected))]
        public virtual Task OnConnected(ILogger logger, [SignalRTrigger] InvocationContext invocationContext, [DurableClient] IDurableEntityClient client)
        {
            return connected(logger, invocationContext, client);
        }

        [FunctionName(nameof(OnDisconnected))]
        public virtual Task OnDisconnected(ILogger logger, [SignalRTrigger] InvocationContext invocationContext,
            [DurableClient] IDurableEntityClient client)
        {
            return disconnected(logger, invocationContext, client);
        }

        [FunctionName($"{nameof(AttachState)}")]
        public virtual async Task AttachState(ILogger logger, [SignalRTrigger] InvocationContext invocationContext, [DurableClient] IDurableEntityClient client, string stateType, string stateKey)
        {
            if (stateType == "MortgageCalculatorEntityStore")
                await attachState<MortgageCalculatorEntityStore>(logger, invocationContext, client, stateKey);
            else if (stateType == "UserCalculatorsEntityStore")
                await attachState<UserCalculatorsEntityStore>(logger, invocationContext, client, stateKey);
        }

        [FunctionName($"{nameof(UnattachState)}")]
        public virtual async Task UnattachState(ILogger logger, [SignalRTrigger] InvocationContext invocationContext, [DurableClient] IDurableEntityClient client, string stateType, string stateKey)
        {
            if (stateType == "MortgageCalculatorEntityStore")
                await unattachState<MortgageCalculatorEntityStore>(logger, invocationContext, client, stateKey);
            else if (stateType == "UserCalculatorsEntityStore")
                await unattachState<UserCalculatorsEntityStore>(logger, invocationContext, client, stateKey);
        }
        #endregion

        #region State Actions
        #region Create Calculator
        [FunctionName($"{nameof(CreateCalculator)}")]
        public virtual async Task CreateCalculator(ILogger logger, [SignalRTrigger] InvocationContext invocationContext, [DurableClient] IDurableEntityClient client, [DurableClient] IDurableOrchestrationClient orchClient, CalculatorState state)
        {
            await startOrchestration(logger, orchClient, nameof(CreateCalculatorFlow), instanceId: invocationContext.UserId, input: state, terminateTimeoutSeconds: 10);
        }

        [FunctionName($"{nameof(CreateCalculatorFlow)}")]
        public virtual async Task CreateCalculatorFlow(ILogger logger, [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var username = context.InstanceId;

            var state = context.GetInput<CalculatorState>();

            var userCalcsClient = context.CreateEntityProxy<UserCalculatorsEntityStore, IUserCalculatorsActions>(username);

            await userCalcsClient.SetLoading();

            if (state.Lookup.IsNullOrEmpty())
                state.Lookup = Guid.NewGuid().ToString();

            await userCalcsClient.CreateCalculator(state);
        }
        #endregion

        #region Recalculate
        [FunctionName($"{nameof(Calculate)}")]
        public virtual async Task Calculate(ILogger logger, [SignalRTrigger] InvocationContext invocationContext, [DurableClient] IDurableEntityClient client, [DurableClient] IDurableOrchestrationClient orchClient, MortgageCalculatorEntityStore state)
        {
            var usersCalc = await client.LoadEntityFromStore<UserCalculatorsEntityStore>(invocationContext.UserId);

            if (usersCalc.CurrentCalculator != null)
                await startOrchestration(logger, orchClient, nameof(CalculateFlow), instanceId: usersCalc.CurrentCalculator.Lookup, input: state);
        }

        [FunctionName($"{nameof(CalculateFlow)}")]
        public virtual async Task CalculateFlow(ILogger logger, [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var calcLookup = context.InstanceId;

            var state = context.GetInput<MortgageCalculatorEntityStore>();

            var calcClient = context.CreateEntityProxy<MortgageCalculatorEntityStore, IMortgageCalculatorActions>(calcLookup);

            await calcClient.SetCalculating(state);

            await calcClient.Calculate();
        }
        #endregion

        #region Remove Calculator
        [FunctionName($"{nameof(RemoveCalculator)}")]
        public virtual async Task RemoveCalculator(ILogger logger, [SignalRTrigger] InvocationContext invocationContext, [DurableClient] IDurableEntityClient client, [DurableClient] IDurableOrchestrationClient orchClient, string calcLookup)
        {
            await startOrchestration(logger, orchClient, nameof(RemoveCalculatorFlow), instanceId: invocationContext.UserId, input: calcLookup, terminateTimeoutSeconds: 10);
        }

        [FunctionName($"{nameof(RemoveCalculatorFlow)}")]
        public virtual async Task RemoveCalculatorFlow(ILogger logger, [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var username = context.InstanceId;

            var calcLookup = context.GetInput<string>();

            var userCalcsClient = context.CreateEntityProxy<UserCalculatorsEntityStore, IUserCalculatorsActions>(username);

            await userCalcsClient.SetLoading();

            await userCalcsClient.RemoveCalculator(calcLookup);
        }
        #endregion

        #region Set Current Calculator
        [FunctionName($"{nameof(SetCurrentCalculator)}")]
        public virtual async Task SetCurrentCalculator(ILogger logger, [SignalRTrigger] InvocationContext invocationContext, [DurableClient] IDurableEntityClient client, [DurableClient] IDurableOrchestrationClient orchClient, string calcLookup)
        {
            await startOrchestration(logger, orchClient, nameof(SetCurrentCalculatorFlow), instanceId: invocationContext.UserId, input: calcLookup);
        }

        [FunctionName($"{nameof(SetCurrentCalculatorFlow)}")]
        public virtual async Task SetCurrentCalculatorFlow(ILogger logger, [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var username = context.InstanceId;

            var calcLookup = context.GetInput<string>();

            var userCalcsClient = context.CreateEntityProxy<UserCalculatorsEntityStore, IUserCalculatorsActions>(username);

            await userCalcsClient.SetLoading();

            await userCalcsClient.SetCurrentCalculator(calcLookup);
        }
        #endregion
        #endregion
    }
}
