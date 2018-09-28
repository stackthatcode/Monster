using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace Push.Foundation.Utilities.Autofac
{
    public class ServiceLocator
    {
        private readonly ILifetimeScope _scope;

        public ServiceLocator(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public T Make<T>()
        {
            return _scope.Resolve<T>();
        }
    }
}
