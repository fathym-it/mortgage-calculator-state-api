using Fathym.LCU.Services.StateAPIs.Durable;

namespace FinaTech.SensitivityModel.StateAPI.State
{
    public class MortgageCalculatorState : LCUStateEntity
    {
        #region Properties
        public virtual bool Calculating { get; set; }

        public virtual double HomeValue { get; set; }

        public virtual double InterestRate { get; set; }

        public virtual double DownPayment { get; set; }

        public virtual double LoanAmount { get; set; }

        public virtual int LoanTerm { get; set; }

        public virtual double MonthlyPayment { get; set; }
        #endregion

        #region Constructors
        public MortgageCalculatorState()
        {
            LoanTerm = 30;
        }
        #endregion
    }
}
