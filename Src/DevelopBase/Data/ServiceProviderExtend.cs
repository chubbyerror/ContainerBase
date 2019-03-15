using System;
using DevelopBase.Data;
namespace DevelopBase.Common
{
    public static partial class ServiceProviderExtend
    {
        public static T GetDbcontext<T>(this IServiceProvider serviceProvider) where T:IDbContext
        {
            var dbcontextType=typeof(T);
            return (T)serviceProvider.GetService(dbcontextType);
        }
    }
}
