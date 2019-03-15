using DevelopBase.Services;
using SimpleBusiness.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBusiness.Service
{
    public class Echo : ServiceBase, IEcho
    {
        public Echo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public string EchoTest(string name)
        {
            //throw new Exception("asdas");
            return $"hello {name}";
        }
    }
}
