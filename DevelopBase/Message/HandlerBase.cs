using System;
using System.Threading.Tasks;
using DevelopBase.Services;
namespace DevelopBase.Message
{
    public abstract class HandlerBase
    {
        private IServiceProvider _serviceProvider=null;
        protected IServiceProvider ServiceProvider{get=>_serviceProvider;}
        public HandlerBase(IServiceProvider serviceProvider)
        {
            if(serviceProvider==null)
            {
                throw new ArgumentNullException();   
            }
            _serviceProvider=serviceProvider;
        }
        public abstract ResponseBase Handler(RequestBase request);
        public async Task<ResponseBase> HandlerAsync(RequestBase request)
        {
            return await Task.Run(()=>Handler(request));
        }
        public T GetService<T>() where T : class, IService
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }
        public T GetObject<T>() where T : class
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }
    }
}
