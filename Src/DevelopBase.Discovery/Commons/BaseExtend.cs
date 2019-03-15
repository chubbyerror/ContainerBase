using System;
using System.Collections.Generic;
using System.Text;

namespace DevelopBase.Discovery.Commons
{
    public class BaseExtend : DevelopBase.Services.ServiceBase,IBaseExtend
    {
        public BaseExtend(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
