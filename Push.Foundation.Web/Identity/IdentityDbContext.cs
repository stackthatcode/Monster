using System;
using System.Data.Common;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;


namespace Push.Foundation.Web.Identity
{
    public class IdentityDbContext : IdentityDbContext<IdentityUser>
    {
        public Guid UniqueIdentifier { get; private set; }

        public IdentityDbContext(DbConnection connection) 
                : base(connection, contextOwnsConnection: false)
        {
            // This follows the "well-established" OWIN conventions
            // Exhibit "A" as to why this OWIN stuff is hell-spawned
            // ... Utterly, utterly shameful!!
            //
            Database.SetInitializer<IdentityDbContext>(null);
            
            UniqueIdentifier = Guid.NewGuid();
        }
    }
}
