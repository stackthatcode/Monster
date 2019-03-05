using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Identity
{
    public class IdentityContext
    {
        // ASP.NET Identity of the actual logged in User
        public string AspNetUserId { get; set; }
        public string Email { get; set; }
        public List<string> AspNetRoles { get; set; }
        public string RolesFlattened => AspNetRoles.ToCommaDelimited();
        public bool IsAdmin => AspNetRoles.Contains(SecurityConfig.AdminRole);
        
        // Monster -> Instance
        public Guid InstanceId { get; set; }
        public string InstanceNickName { get; set; }
        public bool IsRealTimeMode { get; set; }


        public IdentityContext()
        {
            AspNetRoles = new List<string>();
        }        
    }
}
