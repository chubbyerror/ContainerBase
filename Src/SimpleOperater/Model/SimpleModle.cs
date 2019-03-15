using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBusiness.Model
{
    public class SimpleModle
    {
        public class SimpleEchoResquest
        {
            public string name { get; set; }
        }
        public class SimpleEchoResponse
        {
            public string Echo { get; set; }
        }
    }
}
