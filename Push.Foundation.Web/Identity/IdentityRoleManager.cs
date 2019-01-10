using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Push.Foundation.Web.Identity
{
    public class IdentityRoleManager : RoleManager<IdentityRole>
    {
        public IdentityRoleManager(RoleStore<IdentityRole> roleStore)
            : base(roleStore)
        {
        }
    }
}
