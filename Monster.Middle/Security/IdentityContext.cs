using System;
using System.Collections.Generic;

namespace Monster.Middle.Security
{
    public class IdentityContext
    {
        // ASP.NET Identity of the actual logged in User
        public string AspNetUserId { get; set; }
        public string Email { get; set; }
        public List<string> AspNetRoles { get; set; } 
        
        // Monster -> Installation mapped from the ASP.NET User Identity
        public Guid InstallationId { get; set; }
        public string NickName { get; set; }

        public string RolesFlattened => String.Join(",", AspNetRoles ?? new List<string>());
        public bool IsAdmin => AspNetRoles.Contains(SecurityConfig.AdminRole);


        public IdentityContext()
        {
            AspNetRoles = new List<string>();
        }

        //public static IdentityContext Make(
        //        ApplicationUser aspNetUser, EF.User user, UserPersonal personal)
        //{
        //    var output = new IdentityContext();
        //    output.AspNetUserId = aspNetUser.Id;
        //    output.Email = aspNetUser.Email;
        //    output.FirstName = personal.FirstName;
        //    output.LastName = personal.LastName;
        //    output.IsProfileComplete = user.IsProfileComplete;
        //    output.LocalTime = DateTime.UtcNow;
        //    return output;
        //}
    }
}
