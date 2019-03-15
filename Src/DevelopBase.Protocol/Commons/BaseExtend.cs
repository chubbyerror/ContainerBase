using DevelopBase.Message;
using DevelopBase.Common;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using DevelopBase.DPBase;
using static DevelopBase.DPBase.DPBase;

namespace DevelopBase.Protocol
{

    public class BaseExtend :DevelopBase.Services.ServiceBase,  IBaseExtend
    {
        public BaseExtend(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public ResponseBase Handler(BaseRequest request)
        {
            try
            {
                var typeInfos = ServiceProvider.GetService<HashSet<Appinfo>>();
                var t = typeInfos.Where(c => c.AppName == request.ActionKey).FirstOrDefault().AppType;
                var req = Newtonsoft.Json.JsonConvert.DeserializeObject(request.RequestObject, t);
                var handler = (HandlerBase)ServiceProvider.GetHandler(t);
                return handler.Handler((RequestBase)req);
            }
            catch (Exception ex)
            {
                return new ResponseBase(-1, ex.Message);
            }
        }

        public async Task<ResponseBase> HandlerAsync(BaseRequest request)
        {
            try
            {
                var typeInfos = ServiceProvider.GetService<HashSet<Appinfo>>();
                var t = typeInfos.Where(c => c.AppName == request.ActionKey).FirstOrDefault().AppType;
                var req = Newtonsoft.Json.JsonConvert.DeserializeObject(request.RequestObject,t);
                var handler = (HandlerBase)ServiceProvider.GetHandler(t);
                return await handler.HandlerAsync((RequestBase)req);
            }
            catch (Exception ex)
            {
                return new ResponseBase(-1, ex.Message);
            }
        }

    }

}
