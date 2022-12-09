using Castle.Core.Logging;
using Fathym;
using Fathym.LCU.Services.StateAPIs.Durable;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FinaTech.SensitivityModel.StateAPI.State
{
    public interface IMortgageCalculatorActions
    {
        Task<Status> Calculate();

        Task SetCalculating(IMortgageCalculatorState state);
    }

    public interface IMortgageCalculatorState
    {
        bool Calculating { get; set; }

        string CurrentCalculator { get; set; }

        double HomeValue { get; set; }

        double InterestRate { get; set; }

        double DownPayment { get; set; }

        double LoanAmount { get; set; }

        int LoanTerm { get; set; }

        double MonthlyPayment { get; set; }
    }

    public class MortgageCalculatorEntityStore : StateEntityStore<MortgageCalculatorEntityStore>, IMortgageCalculatorState, IMortgageCalculatorActions
    {
        #region Fields
        protected string calcLookup
        {
            get { return keyParts[0]; }
        }
        #endregion

        #region Properties
        public virtual bool Calculating { get; set; }

        public virtual string CurrentCalculator { get; set; }

        public virtual double HomeValue { get; set; }

        public virtual double InterestRate { get; set; }

        public virtual double DownPayment { get; set; }

        public virtual double LoanAmount { get; set; }

        public virtual int LoanTerm { get; set; }

        public virtual double MonthlyPayment { get; set; }
        #endregion

        #region Constructors
        public MortgageCalculatorEntityStore(ILogger<MortgageCalculatorEntityStore> logger = null)
            : base(logger)
        {
            LoanTerm = 30;
        }
        #endregion

        #region API Methods
        public virtual async Task<Status> Calculate()
        {
            calculateMortgagePayment();

            Calculating = false;

            return Status.Success;
        }

        public virtual async Task SetCalculating(IMortgageCalculatorState state)
        {
            Calculating = true;

            if (state != null)
            {
                if (state?.CurrentCalculator != null)
                    CurrentCalculator = state.CurrentCalculator;

                if (state?.HomeValue != null)
                    HomeValue = state.HomeValue;

                if (state?.InterestRate != null)
                    InterestRate = state.InterestRate;

                if (state?.DownPayment != null)
                    DownPayment = state.DownPayment;

                if (state?.LoanAmount != null)
                    LoanAmount = state.LoanAmount;

                if (state?.LoanTerm != null)
                    LoanTerm = state.LoanTerm;

            }
        }
        #endregion

        #region Life Cycle
        [FunctionName(nameof(MortgageCalculatorEntityStore))]
        public async Task Run([EntityTrigger] IDurableEntityContext ctx, [SignalR(HubName = nameof(MortgageStateActions))] IAsyncCollector<SignalRMessage> messages)
        {
            await executeStateEntityLifeCycle(ctx, messages, loadInitialState);

        }
        #endregion

        #region Helpers
        protected virtual void calculateMortgagePayment()
        {
            //  M = P [ i(1 + i)^n ] / [ (1 + i)^n – 1].

            // M = Total monthly payment

            // P = The total amount of your loan

            // I = Your interest rate, as a monthly percentage

            // N = The total amount of months in your timeline for paying off your mortgage

            var totalMonths = LoanTerm * 12;

            var ir = (InterestRate / 12) / 100;

            var numerator = ir * Math.Pow(1 + ir, totalMonths);

            var denominator = Math.Pow(1 + ir, totalMonths) - 1;
            
            MonthlyPayment = LoanAmount * (numerator / denominator);          
        }

        protected virtual async Task<MortgageCalculatorEntityStore> loadInitialState()
        {
            var store = new MortgageCalculatorEntityStore((ILogger<MortgageCalculatorEntityStore>)logger);

            await store.SetCalculating(store);

            await store.Calculate();

            return store;
        }
        #endregion
    }
}
