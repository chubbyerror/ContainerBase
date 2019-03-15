using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kdetcd.Standard.EtcdDiscovery
{
    public class Server : IDisposable
    {
        dotnet_etcd.EtcdClient _client;
        public Server(string host, int port, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            _client = new dotnet_etcd.EtcdClient(host, port, username, password, caCert, clientCert, clientKey, publicRootCa);
        }

        public string Register(string ServiceName, Dictionary<string, string> SrvInfo)
        {
            try
            {
                _client.PutAsync(new Etcdserverpb.PutRequest() { Key=Google.Protobuf.ByteString.CopyFromUtf8( $"/EtcdDiscovery/{ServiceName}"),
                     Value= Google.Protobuf.ByteString.CopyFrom(KdCommon.Common.Serialize(SrvInfo)) }).Wait();
                return $"/EtcdDiscovery/{ServiceName}";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(string ServiceName, Dictionary<string, string> SrvInfo)
        {
            try
            {
                long oldver = -1;
                try
                {
                    oldver = _client.GetAsync($"/EtcdDiscovery/{ServiceName}").Result?.Kvs[0].Version ?? 0;
                }
                catch (Exception ex)
                {
                    if (ex.HResult != -2146233086)
                    {
                        throw ex;
                    }
                    else
                    {
                        throw new Exception("lock object not exisits .");
                    }
                }
                //set new data
                _client.PutAsync(new Etcdserverpb.PutRequest()
                {
                    Key = Google.Protobuf.ByteString.CopyFromUtf8($"/EtcdDiscovery/{ServiceName}"),
                    Value = Google.Protobuf.ByteString.CopyFrom(KdCommon.Common.Serialize(SrvInfo))
                }).Wait();
                var newver = _client.GetAsync($"/EtcdDiscovery/{ServiceName}").Result?.Kvs[0].Version;
                return oldver != newver;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(string ServiceName)
        {
            try
            {
                _client.DeleteRangeAsync($"/EtcdDiscovery/{ServiceName}").Wait();
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2146233086)
                {
                    throw ex;
                }
            }
        }

        public async void Dispose() => await Task.Run(new Action(() => { _client.Dispose(); }));
    }
}
