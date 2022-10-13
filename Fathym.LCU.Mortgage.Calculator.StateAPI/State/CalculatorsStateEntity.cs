using Fathym;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinaTech.SensitivityModel.StateAPI.State
{
    public interface ICalculatorsState
    {
        Task CreateCalculator(CalculatorState calculator);

        Task SetCurrentCalculator(string calcLookup);

        Task RemoveCalculator(string calcLookup);
    }

    public class CalculatorsStateEntity : CalculatorsState, ICalculatorsState
    {
        #region Fields
        protected string entCtxtUsername
        {
            get { return context.EntityKey; }
        }
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public CalculatorsStateEntity()
        { }
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
            CurrentCalculator = calcLookup;

            if (!CurrentCalculator.IsNullOrEmpty())
            {
                var calcId = new EntityId(nameof(MortgageCalculatorStateEntity), CurrentCalculator);

                context.SignalEntity<IMortgageCalculatorState>(calcId, calc => calc.SetCalculating());

                context.SignalEntity<IMortgageCalculatorState>(calcId, calc => calc.Calculate(null));
            }
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
        [FunctionName(nameof(CalculatorsStateEntity))]
        public async Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            if (!ctx.HasState)
            {
                ctx.SetState(new CalculatorsStateEntity());
            }

            await ctx.DispatchAsync<CalculatorsStateEntity>();
        }
        #endregion

        #region Helpers
        #endregion
    }
}
