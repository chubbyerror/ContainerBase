using System;
using DevelopBase.Message;
using System.Threading.Tasks;
namespace DevelopBase.Common
{
    public static partial class ServiceProviderExtend
    {
        public static HandlerBase GetHandler<T>(this IServiceProvider serviceProvider) where T:RequestBase
        {
            Type handlerType=typeof(HandlerGeneric<>).MakeGenericType(typeof(T));
            return (HandlerBase)serviceProvider.GetService(handlerType);
        }
        public static HandlerBase GetHandler(this IServiceProvider serviceProvider,Type requestType)
        {
            Type handlerType=typeof(HandlerGeneric<>).MakeGenericType(requestType);
            return (HandlerBase)serviceProvider.GetService(handlerType);
        }
        public static async Task<ResponseBase> HandlerAsync(this IServiceProvider serviceProvider, RequestBase request)
        {
            return await serviceProvider.GetHandler(request.GetType()).HandlerAsync(request);            
        }
    }
}
