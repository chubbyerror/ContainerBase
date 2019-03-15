using System;

namespace DevelopBase.Common
{
    public static class ReflectionExtend
    {
        public static bool ContainBaseType(this Type type,Type baseType)
        {
            var childType=type;
            while(childType.BaseType!=typeof(object))
            {
                if(childType.BaseType==baseType)
                {
                    return true;
                }
                childType=childType.BaseType;
            }
            return false;
        }
    }
}
