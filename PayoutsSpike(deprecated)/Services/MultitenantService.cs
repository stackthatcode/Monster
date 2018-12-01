using System;

namespace Monster.MiddleTier.Services
{
    public class MultitenantService
    {
        private readonly AccountRepository _accountService;
        private readonly TenantContextRepository _tenantContextRepository;

        public MultitenantService(AccountRepository accountService)
        {
            _accountService = accountService;
        }

        public void Initialize(Guid tenantId)
        {
            var tenant = _accountService.RetrieveTenant(tenantId);

        }
    }
}
