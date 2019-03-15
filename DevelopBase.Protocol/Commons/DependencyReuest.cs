using System;
using System.Collections.Generic;
using System.Text;

namespace DevelopBase.Protocol
{
    public abstract class DependencyReuest
    {
        private IServiceProvider _dependencyContainer = null;
        protected IServiceProvider DependencyContainer
        {
            get => _dependencyContainer;
        }
        public DependencyReuest(IServiceProvider dependencyContainer)
        {
            if (dependencyContainer == null)
            {
                throw new ArgumentNullException();
            }
            _dependencyContainer = dependencyContainer;
        }
    }
}
