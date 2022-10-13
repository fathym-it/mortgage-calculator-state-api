using Fathym;
using Fathym.API;
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
    public class MortgageCalculatorHub : LCUStateHub<MortgageCalculatorStateEntity>
    {
        #region Life Cycle
        [FunctionName("negotiate")]
        public virtual Task<SignalRConnectionInfo> Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestMessage req)
        {
            return negotiate(req);
        }

        [FunctionName(nameof(OnConnected))]
        public virtual Task OnConnected([SignalRTrigger] InvocationContext invocationContext,
            [DurableClient] IDurableEntityClient client, ILogger logger)
        {
            return connected(invocationContext, client, logger);
        }

        [FunctionName($"{nameof(AttachState)}")]
        public virtual async Task AttachState([SignalRTrigger] InvocationContext invocationContext,
            [DurableClient] IDurableEntityClient client, ILogger logger, string stateKey)
        {
            await attachState(invocationContext, $"{invocationContext.UserId}|{stateKey}", client, logger);
        }

        [FunctionName($"{nameof(UnattachState)}")]
        public virtual async Task UnattachState([SignalRTrigger] InvocationContext invocationContext,
            ILogger logger, string stateKey)
        {
            await unattachState(invocationContext, $"{invocationContext.UserId}|{stateKey}", logger);
        }
        #endregion

        #region API Methods
        [FunctionName($"{nameof(MortgageCalculatorHub_Calculate)}")]
        public virtual async Task<Status> MortgageCalculatorHub_Calculate(
            [SignalRTrigger] InvocationContext invocationContext, [DurableClient] IDurableEntityClient client,
            MortgageCalculatorHub_RecalculateRequest recalculateRequest, ILogger logger)
        {
            var stateKey = $"{invocationContext.UserId}|{recalculateRequest.CalculatorLookup}";

            var entityId = new EntityId(nameof(MortgageCalculatorStateEntity), stateKey);

            await client.SignalEntityAsync<IMortgageCalculatorState>(entityId, async (calc) =>
            {
                await calc.SetCalculating();
            });

            await client.SignalEntityAsync<IMortgageCalculatorState>(entityId, async stateSvc =>
            {
                await stateSvc.Calculate(recalculateRequest.Calculator);
            });
            
            return Status.Success;
        }
        #endregion

        #region Helpers
        #endregion
    }

    public class MortgageCalculatorHub_RecalculateRequest : BaseRequest
    {
        public virtual MortgageCalculatorState Calculator { get; set; }

        public virtual string CalculatorLookup { get; set; }
    }
}
