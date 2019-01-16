using System;
using System.Collections.Concurrent;


namespace Monster.Middle.Hangfire
{
    public class NamedLock
    {
        private static readonly
                ConcurrentDictionary<string, string>
                    _processing = new ConcurrentDictionary<string, string>();

        private static readonly object _lock = new object();

        private readonly string _methodName;

        public NamedLock(string methodName)
        {
            _methodName = methodName;
        }

        private string BuildKey(string userId)
        {
            return _methodName + ":" + userId;
        }

        public bool Acquire(string userId)
        {
            var key = BuildKey(userId);

            lock (_lock)
            {
                if (_processing.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    _processing[key] = key;
                    return true;
                }
            }
        }

        public bool Free(string userId)
        {
            lock (_lock)
            {
                try
                {
                    var key = BuildKey(userId);
                    string keyOut;
                    _processing.TryRemove(key, out keyOut);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }
    }
}

