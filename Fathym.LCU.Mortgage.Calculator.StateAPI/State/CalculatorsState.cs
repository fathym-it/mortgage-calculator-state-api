using Fathym.LCU.Services.StateAPIs.Durable;
using System.Collections.Generic;

namespace FinaTech.SensitivityModel.StateAPI.State
{
    public class CalculatorsState : LCUStateEntity
    {
        #region Properties
        public virtual List<CalculatorState> Calculators { get; set; }

        public virtual string CurrentCalculator { get; set; }
        #endregion

        #region Constructors
        public CalculatorsState()
        {
            Calculators = new List<CalculatorState>();
        }
        #endregion
    }
}
