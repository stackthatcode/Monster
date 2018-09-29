using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.Middle.Sql.Multitenant;
using Monster.Middle.Sql.System;

namespace Monster.Middle.Services
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
