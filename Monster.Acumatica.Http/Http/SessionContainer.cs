using System;
using System.Net;

namespace Monster.Acumatica.Http
{
    // This is the Lifetime-scoped wrapper object that retains
    // ... the CookieContainer
    public class SessionContainer
    {
        public CookieContainer CookieContainer { get; private set; }
        public Guid Identifier { get; private set; }

        public SessionContainer()
        {
            Identifier = Guid.NewGuid();
            CookieContainer = new CookieContainer();
        }
    }
}
