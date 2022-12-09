using Fathym;
using Fathym.API;
using Fathym.LCU.Services.StateAPIs.Durable;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FinaTech.SensitivityModel.StateAPI.State
{
    public class MortgageCalculatorAPIs : LCUStateAPIs<MortgageCalculatorStateEntity>
    {
        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public MortgageCalculatorAPIs(ILogger<MortgageCalculatorAPIs> logger)
            : base(logger)
        { }
        #endregion

        #region API Methods
        #region Calculators State API Methods
        #region Routes
        private const string createCalcStateRoute = nameof(UserCalculatorsEntityStore);

        private const string getCStateRoute = nameof(UserCalculatorsEntityStore);

        private const string removeCalculatorRoute = nameof(UserCalculatorsEntityStore) + "/{calcLookup}";

        private const string setCurrentCalculatorRoute = nameof(UserCalculatorsEntityStore) + "/current";
        #endregion

        [FunctionName(nameof(CalculatorsStateEntity_CreateCalculator))]
        public virtual async Task<HttpResponseMessage> CalculatorsStateEntity_CreateCalculator(ILogger log,
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = createCalcStateRoute)] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            [SignalR(HubName = "SensitivityModelHub")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return await withAPIBoundary<CalculatorState, BaseResponse<UserCalculatorsEntityStore>>(req, async (request, response) =>
            {
                var entLookup = "test";// req.GetEnterpriseLookup();

                var username = "test";//req.LoadUsername();

                var key = $"{entLookup}|{username}";

                if (request.Lookup.IsNullOrEmpty())
                    request.Lookup = Guid.NewGuid().ToString();

                var entityId = new EntityId(nameof(UserCalculatorsEntityStore), key);

                await client.SignalEntityAsync<ICalculatorsState>(entityId, async (calc) =>
                {
                    await calc.CreateCalculator(request);
                });

                response.Status = Status.Success;

                return response;
            }).Run();
        }

        [FunctionName(nameof(CalculatorsStateEntity_GetState))]
        public virtual async Task<HttpResponseMessage> CalculatorsStateEntity_GetState(ILogger log,
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = getCStateRoute)] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            [SignalR(HubName = "SensitivityModelHub")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return await withAPIBoundary<BaseResponse<UserCalculatorsEntityStore>>(req, async (response) =>
            {
                var entLookup = "test";// req.GetEnterpriseLookup();

                var username = "test";//req.LoadUsername();

                var key = $"{entLookup}|{username}";

                var entityId = new EntityId(nameof(UserCalculatorsEntityStore), key);

                var state = await client.ReadEntityStateAsync<UserCalculatorsEntityStore>(entityId);

                response.Model = state.EntityState;

                response.Status = response.Model != null ? Status.Success : Status.NotLocated;

                await signalStateUpdate(client, signalRMessages, key);

                return response;
            }).Run();
        }

        [FunctionName(nameof(CalculatorsStateEntity_RemoveCalculator))]
        public virtual async Task<HttpResponseMessage> CalculatorsStateEntity_RemoveCalculator(ILogger log,
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = removeCalculatorRoute)] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            [SignalR(HubName = "SensitivityModelHub")] IAsyncCollector<SignalRMessage> signalRMessages, string calcLookup)
        {
            return await withAPIBoundary<BaseResponse<UserCalculatorsEntityStore>>(req, async (response) =>
            {
                var entLookup = "test";// req.GetEnterpriseLookup();

                var username = "test";//req.LoadUsername();

                var key = $"{entLookup}|{username}";

                var entityId = new EntityId(nameof(UserCalculatorsEntityStore), key);

                await client.SignalEntityAsync<ICalculatorsState>(entityId, async (calc) =>
                {
                    await calc.RemoveCalculator(calcLookup);
                });

                response.Status = Status.Success;

                return response;
            }).Run();
        }

        [FunctionName(nameof(CalculatorsStateEntity_SetCurrentCalculator))]
        public virtual async Task<HttpResponseMessage> CalculatorsStateEntity_SetCurrentCalculator(ILogger log,
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = setCurrentCalculatorRoute)] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            [SignalR(HubName = "SensitivityModelHub")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return await withAPIBoundary<MetadataModel, BaseResponse<UserCalculatorsEntityStore>>(req, async (request, response) =>
            {
                var entLookup = "test";// req.GetEnterpriseLookup();

                var username = "test";//req.LoadUsername();

                var key = $"{entLookup}|{username}";

                var entityId = new EntityId(nameof(UserCalculatorsEntityStore), key);

                await client.SignalEntityAsync<ICalculatorsState>(entityId, async (calc) =>
                {
                    await calc.SetCurrentCalculator(request.Metadata["CalculatorLookup"].ToString());
                });

                response.Status = Status.Success;

                return response;
            }).Run();
        }
        #endregion

        #region Sensitivity Model Calculator State API Methods
        #region Routes
        private const string getSMCStateRoute = nameof(MortgageCalculatorStateEntity) + "/{calcLookup}";

        private const string recalculateRoute = nameof(MortgageCalculatorStateEntity) + "/{calcLookup}";
        #endregion

        [FunctionName(nameof(MortgageCalculatorStateEntity_GetState))]
        public virtual async Task<HttpResponseMessage> MortgageCalculatorStateEntity_GetState(ILogger log,
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = getSMCStateRoute)] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            [SignalR(HubName = "SensitivityModelHub")] IAsyncCollector<SignalRMessage> signalRMessages, string calcLookup)
        {
            return await withAPIBoundary<BaseResponse<MortgageCalculatorStateEntity>>(req, async (response) =>
            {
                var entityId = new EntityId(nameof(MortgageCalculatorStateEntity), calcLookup);

                var state = await client.ReadEntityStateAsync<MortgageCalculatorStateEntity>(entityId);

                response.Model = state.EntityState;

                response.Status = response.Model != null ? Status.Success : Status.NotLocated;

                //await signalStateUpdate(client, signalRMessages, calcLookup);

                return response;
            }).Run();
        }

        [FunctionName(nameof(MortgageCalculatorStateEntity_Recalculate))]
        public virtual async Task<HttpResponseMessage> MortgageCalculatorStateEntity_Recalculate(ILogger log,
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = recalculateRoute)] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            [SignalR(HubName = "SensitivityModelHub")] IAsyncCollector<SignalRMessage> signalRMessages, string calcLookup)
        {
            return await withAPIBoundary<MortgageCalculatorState, BaseResponse<MortgageCalculatorStateEntity>>(req, async (request, response) =>
            {
                var entityId = new EntityId(nameof(MortgageCalculatorStateEntity), calcLookup);

                await client.SignalEntityAsync<IMortgageCalculatorState>(entityId, async (calc) =>
                {
                    await calc.SetCalculating();
                });

                await client.SignalEntityAsync<IMortgageCalculatorState>(entityId, async (calc) =>
                {
                    await calc.Calculate(request);
                });

                response.Status = Status.Success;

                return response;
            }).Run();
        } 
        #endregion
        #endregion
    }
}
