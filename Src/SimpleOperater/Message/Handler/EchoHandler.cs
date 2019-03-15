using DevelopBase.Message;
using SimpleBusiness.Message.Request;
using SimpleBusiness.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBusiness.Message.Handler
{
    public class EchoHandler : HandlerGeneric<EchoRequest>
    {
        public EchoHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override ResponseBase Handler(EchoRequest request)
        {
            try
            {
                string echo = GetService<IEcho>().EchoTest(request.Name);
                return new ResponseGeneric<string>(1, "", echo);
            }
            catch (Exception ex)
            {
                return new ResponseGeneric<string>(ex.HResult, ex.Message, null);
            }

        }
    }
}
