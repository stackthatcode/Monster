using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Identity
{
    public class IdentityContext
    {
        // ASP.NET Identity of the logged in User
        public string AspNetUserId { get; set; }
        public string Email { get; set; }
        public List<string> AspNetRoles { get; set; }

        // Monster -> System Instance
        public Guid InstanceId { get; set; }
        public string InstanceNickName { get; set; }

        // Monster -> Instance -> State
        public SystemState SystemState { get; set; }

        // Computed properties
        public string RolesFlattened => AspNetRoles.ToCommaDelimited();
        public bool IsAdmin => AspNetRoles.Contains(SecurityConfig.AdminRole);
        public bool IsAuthenticated =>
                AspNetRoles.Contains(SecurityConfig.AdminRole)
                || AspNetRoles.Contains(SecurityConfig.UserRole);


        public IdentityContext()
        {
            AspNetRoles = new List<string>();
        }

        public void LoadState(SystemState state)
        {
            this.SystemState = state;
        }

        public static IdentityContext AnonymousIdentity()
        {
            return new IdentityContext();
        }
    }
}
