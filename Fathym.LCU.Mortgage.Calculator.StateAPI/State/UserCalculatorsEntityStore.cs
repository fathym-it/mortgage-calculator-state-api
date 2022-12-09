using Fathym;
using Fathym.LCU.Services.StateAPIs.Durable;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinaTech.SensitivityModel.StateAPI.State
{
    public interface IUserCalculatorsActions
    {
        Task CreateCalculator(CalculatorState calculator);
        
        Task RemoveCalculator(string calcLookup);

        Task SetCurrentCalculator(string calcLookup);

        Task SetLoading();
    }

    public interface IUserCalculatorsState
    {
        bool Loading { get; set; }

        List<CalculatorState> Calculators { get; set; }

        CalculatorState CurrentCalculator { get; set; }
    }

    public class UserCalculatorsEntityStore : StateEntityStore<UserCalculatorsEntityStore>, IUserCalculatorsState, IUserCalculatorsActions
    {
        #region Fields
        protected string username
        {
            get { return keyParts[0]; }
        }
        #endregion

        #region Properties
        public virtual bool Loading { get; set; }

        public virtual List<CalculatorState> Calculators { get; set; }

        public virtual CalculatorState CurrentCalculator { get; set; }
        #endregion

        #region Constructors
        public UserCalculatorsEntityStore(ILogger<UserCalculatorsEntityStore> logger = null)
            : base(logger)
        {
            Calculators = new List<CalculatorState>();
        }
        #endregion

        #region API Methods
        public virtual async Task CreateCalculator(CalculatorState calculator)
        {
            if (!Calculators.Any(c => c.Lookup == calculator.Lookup))
            {
                Calculators.Add(calculator);
            }
            else
            {
                //  TODO:  What to do when already exists?
            }

            await SetCurrentCalculator(calculator.Lookup);
        }

        public virtual async Task SetCurrentCalculator(string calcLookup)
        {
            CurrentCalculator = Calculators?.FirstOrDefault(calc => calc.Lookup == calcLookup);

            if (CurrentCalculator != null)
            {
                var calcId = new EntityId(nameof(MortgageCalculatorEntityStore), CurrentCalculator.Lookup);

                context.SignalEntity<IMortgageCalculatorActions>(calcId, calc => calc.SetCalculating(null));

                context.SignalEntity<IMortgageCalculatorActions>(calcId, calc => calc.Calculate());
            }
        }

        public virtual async Task SetLoading()
        {
            Loading = true;
        }

        public virtual async Task RemoveCalculator(string calcLookup)
        {
            var curCalc = Calculators.FirstOrDefault(c => c.Lookup == calcLookup);

            if (curCalc != null)
            {
                Calculators.Remove(curCalc);
            }
            else
            {
                //  TODO:  What to do when doesn't exists?
            }

            await SetCurrentCalculator(null);
        }
        #endregion

        #region Life Cycle
        [FunctionName(nameof(UserCalculatorsEntityStore))]
        public async Task Run([EntityTrigger] IDurableEntityContext ctx, [SignalR(HubName = nameof(MortgageStateActions))] IAsyncCollector<SignalRMessage> messages)
        {
            await executeStateEntityLifeCycle(ctx, messages, loadInitialState);

        }
        #endregion

        #region Helpers
        protected virtual async Task<UserCalculatorsEntityStore> loadInitialState()
        {
            var store = new UserCalculatorsEntityStore((ILogger<UserCalculatorsEntityStore>)logger);

            return store;
        }
        #endregion
    }
}
