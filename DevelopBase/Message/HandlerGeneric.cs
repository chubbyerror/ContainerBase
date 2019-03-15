using System;

namespace DevelopBase.Message
{
    public abstract class HandlerGeneric<T> : HandlerBase where T:RequestBase
    {
        public HandlerGeneric(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public abstract ResponseBase Handler(T request);
        public override ResponseBase Handler(RequestBase request)
        {
            return Handler((T)request);
        }
    }
}
