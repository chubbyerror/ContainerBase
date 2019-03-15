using System;

namespace DevelopBase.Common
{
    public class RegisterInfo
    {
        public Type From
        {
            get
            {
                if(string.IsNullOrEmpty(FromName))
                {
                    return null;
                }
                return Type.GetType(FromName);
            }
        } 
        public Type To
        {
            get
            {
                if(string.IsNullOrEmpty(ToName))
                {
                    return null;
                }
                return Type.GetType(ToName);

            }
        }
        public string FromName{get;set;}
        public string ToName{get;set;}
        public LifeScope LifeScope{get;set;}
        public object[] ConstructorParams{get;set;}
    }
}
