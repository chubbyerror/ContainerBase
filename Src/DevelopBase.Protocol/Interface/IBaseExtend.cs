using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DevelopBase.Protocol
{
    public interface IBaseExtend:DevelopBase.Services.IService
    {
        Message.ResponseBase Handler(BaseRequest request);
        Task<Message.ResponseBase> HandlerAsync(BaseRequest request);
    }
}
