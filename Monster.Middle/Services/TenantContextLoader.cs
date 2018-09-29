using System;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Sys;
using Monster.Middle.Sql.Multitenant;

namespace Monster.Middle.Services
{
    public class TenantContextLoader
    {
        private readonly AccountRepository _accountRepository;
        private readonly TenantContextRepository _tenantContextRepository;

        public TenantContextLoader(
                TenantContextRepository tenantContextRepository, 
                AccountRepository accountRepository)
        {
            _tenantContextRepository = tenantContextRepository;
            _accountRepository = accountRepository;
        }

        public void Initialize(Guid tenantId)
        {
            var tenant = _accountRepository.RetrieveTenant(tenantId);

            // Load into PersistContext

            // Load into Shopify's context...

            // Load into Acumatica's context...

            // etc.
        }
    }
}
