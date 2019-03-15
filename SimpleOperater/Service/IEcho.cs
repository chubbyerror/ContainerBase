using DevelopBase.Services;
using System;
using System.Collections.Generic;
using System.Text;
using static SimpleBusiness.Model.SimpleModle;

namespace SimpleBusiness.Service
{
    public interface IEcho:IService
    {
        string EchoTest(string name);
    }
}
