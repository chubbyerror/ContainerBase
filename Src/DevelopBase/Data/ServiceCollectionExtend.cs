using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using DevelopBase.Data;
namespace DevelopBase.Common
{
    public static partial class ServiceCollectionExtend
    {
        public static void AddDbcontext(this IServiceCollection serviceCollection,IEnumerable<RegisterInfo> dbcontexts)
        {
            foreach(var item in dbcontexts)
            {
                if(!item.From.GetInterfaces().Contains(typeof(IDbContext)))
                {
                    throw new ArgumentException();
                }
                if(!item.To.GetInterfaces().Contains(item.From))
                {
                    throw new ArgumentException();
                }
                if(!item.To.ContainBaseType(typeof(DbContextBase)))
                {
                    throw new ArgumentException();
                }
                switch(item.LifeScope)
                {
                    case LifeScope.Default:
                    {
                        serviceCollection.AddTransient(item.From,provider=>{
                            return Activator.CreateInstance(item.To,item.ConstructorParams);
                        });
                        break;
                    }
                    case LifeScope.Thread:
                    {
                        serviceCollection.AddScoped(item.From,providerFactory=>{
                            return Activator.CreateInstance(item.To,item.ConstructorParams);
                        });
                        break;
                    }
                    case LifeScope.Singleton:
                    {
                        
                        serviceCollection.AddSingleton(item.From,providerFactory=>{
                            return Activator.CreateInstance(item.To,item.ConstructorParams);
                        
                        });
                        break;
                    }
                }
            }
        }
    }
}
