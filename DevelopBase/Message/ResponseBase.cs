using System;
using System.Collections.Generic;
using System.Text;

namespace DevelopBase.Message
{
    public  class ResponseBase
    {
        private int _errorCode = 0;
        private string _errorInfo = "";
        public ResponseBase(int errorCode=0, string errorInfo="")
        {
            _errorCode = errorCode;
            _errorInfo = errorInfo;
        }
        public int ErrorCode
        {
            get => _errorCode;
        }
        public string ErrorInfo
        {
            get => _errorInfo;
            protected set => _errorInfo = value;
        }
    }
}
