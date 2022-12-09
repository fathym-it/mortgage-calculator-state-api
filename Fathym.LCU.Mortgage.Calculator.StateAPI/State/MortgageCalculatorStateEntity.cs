//using Fathym;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.DurableTask;
//using System;
//using System.Threading.Tasks;

//namespace FinaTech.SensitivityModel.StateAPI.State
//{
//    public interface IMortgageCalculatorState
//    {
//        Task<Status> Calculate(MortgageCalculatorState state);

//        Task SetCalculating();
//    }

//    public class MortgageCalculatorStateEntity : MortgageCalculatorState, IMortgageCalculatorState
//    {
//        #region Fields
//        protected string calcLookup
//        {
//            get { return context.EntityKey; }
//        }
//        #endregion

//        #region Properties
//        #endregion

//        #region Constructors
//        public MortgageCalculatorStateEntity()
//        {
//            LoanTerm = 30;
//        }
//        #endregion

//        #region API Methods
//        public virtual async Task<Status> Calculate(MortgageCalculatorState state)
//        {
//            if (state != null)
//            {
//                if (state?.CurrentCalculator != null)
//                    CurrentCalculator = state.CurrentCalculator;

//                if (state?.HomeValue != null)
//                    HomeValue = state.HomeValue;

//                if (state?.InterestRate != null)
//                    InterestRate = state.InterestRate;

//                if (state?.DownPayment != null)
//                    DownPayment = state.DownPayment;

//                if (state?.LoanAmount != null)
//                    LoanAmount = state.LoanAmount;

//                if (state?.LoanTerm != null)
//                    LoanTerm = state.LoanTerm;

//                calculateMortgagePayment();
//            }

//        Calculating = false;

//            return Status.Success;
//        }

//        public virtual async Task SetCalculating()
//        {
//            Calculating = true;
//        }
//        #endregion

//        #region Life Cycle
//        [FunctionName(nameof(MortgageCalculatorStateEntity))]
//        public async Task Run([EntityTrigger] IDurableEntityContext ctx)
//        {
//            if (!ctx.HasState)
//            {
//                ctx.SetState(new MortgageCalculatorStateEntity());
//            }

//            await ctx.DispatchAsync<MortgageCalculatorStateEntity>();
//        }
//        #endregion

//        #region Helpers
//        protected virtual void calculateMortgagePayment()
//        {
//            //  M = P [ i(1 + i)^n ] / [ (1 + i)^n – 1].

//            // M = Total monthly payment

//            // P = The total amount of your loan

//            // I = Your interest rate, as a monthly percentage

//            // N = The total amount of months in your timeline for paying off your mortgage

//            var totalMonths = LoanTerm * 12;

//            var ir = (InterestRate / 12) / 100;

//            var numerator = ir * Math.Pow(1 + ir, totalMonths);

//            var denominator = Math.Pow(1 + ir, totalMonths) - 1;
            
//            MonthlyPayment = LoanAmount * (numerator / denominator);          
//        }
//        #endregion
//    }
//}
