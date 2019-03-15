namespace DevelopBase.Protocol
{
    public class BaseRequest:DevelopBase.Message.RequestBase
    {
        public string ActionKey { set; get; }
        public string RequestObject { set; get; }
    }
}