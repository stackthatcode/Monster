namespace Monster.Middle.Processes.Sync.Workers.Orders
{
    public class ParallelWorker
    {
        private readonly AcumaticaOrderSync _acumaticaOrderSync;

        public ParallelWorker(AcumaticaOrderSync acumaticaOrderSync)
        {
            _acumaticaOrderSync = acumaticaOrderSync;
        }


    }
}
