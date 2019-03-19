using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Kdetcd.Standard.EtcdDiscovery
{
    public class Server : IDisposable
    {
        dotnet_etcd.EtcdClient _client;
        HashSet<zkDiscoveryWatcher> watchers = new HashSet<zkDiscoveryWatcher>();
        private bool IsDisposing = false;
        int _locklease = 10;

        public Server(string host, int port, int locklease = 10, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            _locklease = locklease;
            _client = new dotnet_etcd.EtcdClient(host, port, username, password, caCert, clientCert, clientKey, publicRootCa);
            //过期延期部分
            Task.Run(() =>
            {
                while (!IsDisposing)
                {
                    Dictionary<Google.Protobuf.ByteString, Google.Protobuf.ByteString> lockers = null;
                    lock (watchers)
                    {
                        lockers = watchers.ToDictionary(c=>c.key,c=>c.value);
                    }
                    if (IsDisposing)
                    {
                        return;
                    }
                    Etcdserverpb.LeaseGrantRequest request = new Etcdserverpb.LeaseGrantRequest();
                    request.TTL = _locklease;
                    var lease = _client.LeaseGrant(request);
                    foreach (var item in lockers)
                    {
                        _client.Put(new Etcdserverpb.PutRequest()
                        {
                            Key = item.Key,
                            Value = item.Value,
                            Lease = lease.ID
                        });
                    }
                    //休眠 TTL/2 时间，防止执行过程导致的node丢失
                    System.Threading.Thread.Sleep(_locklease * 500);
                }
            });
        }

        public string Register(string ServiceName,  string SrvInfo)
        {
            try
            {
                Etcdserverpb.LeaseGrantRequest request = new Etcdserverpb.LeaseGrantRequest();
                request.TTL = _locklease;
                var lease = _client.LeaseGrant(request);
                var newkey = Google.Protobuf.ByteString.CopyFromUtf8($"/EtcdDiscovery/{ServiceName}");
                var newval = Google.Protobuf.ByteString.CopyFromUtf8(SrvInfo);

                _client.PutAsync(new Etcdserverpb.PutRequest()
                {
                    Key = newkey,
                    Value = newval,
                    Lease=lease.ID
                }).Wait();
                lock (watchers)
                {
                    watchers.Add(new zkDiscoveryWatcher() {
                        key=newkey,
                        value=newval
                    });
                }
                return $"/EtcdDiscovery/{ServiceName}";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(string ServiceName,  string SrvInfo)
        {
            try
            {
                var haskey = false;
                var newkey = Google.Protobuf.ByteString.CopyFromUtf8($"/EtcdDiscovery/{ServiceName}");
                lock (watchers)
                {
                    haskey = watchers.Any(c => c.key == newkey);
                }

                if (!haskey)
                {
                    return false;
                }

                Etcdserverpb.LeaseGrantRequest request = new Etcdserverpb.LeaseGrantRequest();
                request.TTL = _locklease;
                var lease = _client.LeaseGrant(request);

                var newval = Google.Protobuf.ByteString.CopyFromUtf8(SrvInfo);
                //set new data
                _client.PutAsync(new Etcdserverpb.PutRequest()
                {
                    Key =newkey ,
                    Value = newval,
                    Lease=lease.ID
                }).Wait();
                lock (watchers)
                {
                    watchers.Add(new zkDiscoveryWatcher()
                    {
                        key=newkey,
                        value=newval
                    });
                }
                var newver = _client.GetAsync($"/EtcdDiscovery/{ServiceName}").Result?.Kvs[0].Version;
                return true;
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
                lock (watchers)
                {
                    watchers.RemoveWhere(c => c.key == Google.Protobuf.ByteString.CopyFromUtf8($"/EtcdDiscovery/{ServiceName}"));
                }
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
