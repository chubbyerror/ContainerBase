using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using DevelopBase.Services;
using System.Linq;
namespace DevelopBase.Common
{

    public static partial class ServiceCollectionExtend
    {
        public static void AddServices(this IServiceCollection serviceCollection,IEnumerable<RegisterInfo> services)
        {
            foreach(var item in services)
            {
                if(!item.To.GetInterfaces().Contains(typeof(IService)))
                {
                    throw new ArgumentException();
                }
                
                switch(item.LifeScope)
                {
                    case LifeScope.Default:
                    {
                        serviceCollection.AddTransient(item.From,DefaultServiceProviderFactory=>{
                            return Activator.CreateInstance(item.To,new object[]{DefaultServiceProviderFactory});
                        });
                        break;
                    }
                    case LifeScope.Thread:
                    {
                        serviceCollection.AddScoped(item.From,DefaultServiceProviderFactory=>Activator.CreateInstance(item.To,new object[]{DefaultServiceProviderFactory}));
                        break;
                    }
                    case LifeScope.Singleton:
                    {
                        serviceCollection.AddSingleton(item.From,DefaultServiceProviderFactory=>Activator.CreateInstance(item.To,new object[]{DefaultServiceProviderFactory}));
                        break;
                    }
                }
            }
        }
    }
}
