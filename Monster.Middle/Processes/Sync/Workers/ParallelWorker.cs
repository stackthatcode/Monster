namespace Monster.Middle.Processes.Sync.Workers.Orders
{
    public class ParallelWorker
    {
        private readonly AcumaticaOrderPut _acumaticaOrderSync;

        public ParallelWorker(AcumaticaOrderPut acumaticaOrderSync)
        {
            _acumaticaOrderSync = acumaticaOrderSync;
        }
    }
}
