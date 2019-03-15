using DevelopBase.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBusiness.Message.Request
{
    public class EchoRequest:RequestBase
    {
        public string Name { get; set; }
    }
}
