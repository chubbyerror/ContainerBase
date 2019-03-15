using System;
using System.Collections.Generic;
using System.Text;

namespace DevelopBase.Message
{
    public class ResponseGeneric<T>:ResponseBase 
    {
        private T _result =default(T);
        public ResponseGeneric(int errorCode, string errorInfo, T result) : base(errorCode, errorInfo)
        {
            _result = result;
        }
        public T Result
        {
            get => _result;
        }
    }
}
