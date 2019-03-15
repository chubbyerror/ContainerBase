using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcTest
{
    public class ResponesModel
    {
        public class SimpleEchoResponse
        {
            public string ErrorCode { get; set; }
            public string ErrorMsg { get; set; }
            public string ResultObjects { get; set; }
        }
    }
}
