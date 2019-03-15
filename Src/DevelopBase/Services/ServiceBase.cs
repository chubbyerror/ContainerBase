using System;
using DevelopBase.Data;
namespace DevelopBase.Services
{
    public abstract class ServiceBase
    {
        private IServiceProvider _serviceProvider;
        protected IServiceProvider ServiceProvider{get=>_serviceProvider;}
        public ServiceBase(IServiceProvider serviceProvider)
        {
            if(serviceProvider==null)
            {
                throw new ArgumentNullException();
            }
            _serviceProvider=serviceProvider;
        }
        protected T GetService<T>() where T : class,IService
        {
            return (T)_serviceProvider.GetService(typeof(T));
        }
        protected T GetDbContext<T>() where T : class, IDbContext
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }
        protected T GetObject<T>() where T : class
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }


    }
}