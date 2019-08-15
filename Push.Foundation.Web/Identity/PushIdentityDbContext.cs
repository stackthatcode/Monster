using System;
using System.Data.Common;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;


namespace Push.Foundation.Web.Identity
{
    public class PushIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public Guid UniqueIdentifier { get; private set; }

        public PushIdentityDbContext(DbConnection connection) 
                : base(connection, contextOwnsConnection: false)
        {
            // This follows the "well-established" OWIN conventions
            // Exhibit "A" as to why this OWIN stuff is hell-spawned
            // ... Utterly, utterly shameful!!
            //
            Database.SetInitializer<PushIdentityDbContext>(null);
            
            UniqueIdentifier = Guid.NewGuid();
        }
    }
}
