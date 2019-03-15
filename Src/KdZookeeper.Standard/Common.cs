using System;
using System.Collections.Generic;
using System.Text;

namespace KdZookeeper.Standard
{
    public static class Common
    {
        public static byte[] Serialize<T>(T data)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                var ser = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
                using (var binaryDictionaryWriter = System.Xml.XmlDictionaryWriter.CreateBinaryWriter(memory))
                {
                    ser.WriteObject(binaryDictionaryWriter, data);
                    binaryDictionaryWriter.Flush();
                }
                return memory.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] data) where T : class
        {
            var ser = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            System.Xml.XmlDictionaryReaderQuotas quotas = new System.Xml.XmlDictionaryReaderQuotas();
            using (var binaryDictionaryReader = System.Xml.XmlDictionaryReader.CreateBinaryReader(data, quotas))
            {
                return ser.ReadObject(binaryDictionaryReader, false) as T;
            }
        }
    }
}
