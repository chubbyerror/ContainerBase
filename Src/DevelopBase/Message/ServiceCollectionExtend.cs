using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using DevelopBase.Common;
using System.Reflection;
using DevelopBase.Message;
namespace DevelopBase.Common
{
    public static partial class ServiceCollectionExtend
    {
        public static void AddHandlers(this IServiceCollection serviceCollection,IEnumerable<RegisterInfo> handlers)
        {
            foreach(var item in handlers)
            {
                //生成泛型
                var handlerType=typeof(HandlerGeneric<>).MakeGenericType(item.From);
                switch(item.LifeScope)
                {
                    case LifeScope.Default:
                    {
                        serviceCollection.AddTransient(handlerType,factory=>{
                            return Activator.CreateInstance(item.To,new object[]{factory});
                        });
                        break;
                    }
                    case LifeScope.Thread:
                    {
                        serviceCollection.AddScoped(handlerType,factory=>{
                            return Activator.CreateInstance(item.To,new object[]{factory});
                        });
                        break;

                    }
                    case LifeScope.Singleton:
                    {
                            serviceCollection.AddSingleton(handlerType,factory=>{
                            return Activator.CreateInstance(item.To,new object[]{factory});
                        });
                        break;

                    }
                }
            }
        }
    }
}
