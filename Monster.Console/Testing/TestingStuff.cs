using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Logging;
using BackgroundJob = Hangfire.BackgroundJob;

namespace Monster.ConsoleApp
{
    // *** Testing LifeTime Scope
    //    builder.RegisterType<ScopeTestingRiffRaff>().InstancePerLifetimeScope();
    // *** Testing LifeTime Scope


    public class AutofacScopeTester
    {
        private static int _globalIdentifier = 1;

        private IPushLogger _logger;
        private readonly ProcessPersistContext _persist;
        public int Id = _globalIdentifier++;

        public AutofacScopeTester(
                IPushLogger logger, ProcessPersistContext persist)
        {
            _logger = logger;
            _persist = persist;
        }

        // Run this after Hangfire persistence is setup
        public static void ScheduleSomeTrouble()
        {
            // *** Testing LifeTime Scope
            var jobId1 = BackgroundJob.Enqueue<AutofacScopeTester>(
                x => x.Run());
            var jobId2 = BackgroundJob.Enqueue<AutofacScopeTester>(
                x => x.Run());
            var jobId3 = BackgroundJob.Enqueue<AutofacScopeTester>(
                x => x.Run());
            // *** Testing LifeTime Scope
        }

        public void Run()
        {
            while (true)
            {
                _logger.Info(
                    $"Sound off from RiffRaff: {Id} - {_persist.Id}");
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
