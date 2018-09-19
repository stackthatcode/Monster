using Autofac;
using Monster.Acumatica.Config;
using Monster.Middle.Config;
using Monster.Middle.Processes.Payouts;
using Push.Foundation.Utilities.Autofac;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Runners
{
    public class PayoutBootstrap
    {
        public static 
                void RunPayouts(
                    PrivateAppCredentials shopifyCredentials,
                    AcumaticaCredentials acumaticaCredentials,
                    PayoutConfig payoutConfig)
        {
            using (var container =
                    MiddleAutofac.Build(
                        connStringOverride: payoutConfig.ConnectionString,
                        loggerName: "Monster.Payouts"))
            {
                container.ExecuteInLifetimeScope(
                    scope =>
                    {
                        var process = scope.Resolve<PayoutProcess>();

                        process.Execute(
                            shopifyCredentials, 
                            acumaticaCredentials,
                            payoutConfig.ScreenApiUrl);
                    });
            }
        }
    }
}
