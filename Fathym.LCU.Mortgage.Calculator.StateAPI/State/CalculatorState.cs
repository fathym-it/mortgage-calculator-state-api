using Fathym.LCU.Services.StateAPIs.Durable;

namespace FinaTech.SensitivityModel.StateAPI.State
{
    public class CalculatorState : LCUStateEntity
    {
        #region Properties
        public virtual string Lookup { get; set; }

        public virtual string Name { get; set; }
        #endregion
    }
}
