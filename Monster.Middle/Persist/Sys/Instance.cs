using System;

namespace Monster.Middle.Persist.Sys
{
    public class Instance
    {
        public Guid Id { get; set; }
        public string ConnectionString { get; set; }
        public string Nickname { get; set; }
        public Guid OwnerUserId { get; set; }
    }
}
