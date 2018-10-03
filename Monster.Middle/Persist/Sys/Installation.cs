using System;

namespace Monster.Middle.Persist.Sys
{
    public class Installation
    {
        public Guid InstallationId { get; set; }
        public string ConnectionString { get; set; }
        public long CompanyId { get; set; }
        public string Nickname { get; set; }
    }
}