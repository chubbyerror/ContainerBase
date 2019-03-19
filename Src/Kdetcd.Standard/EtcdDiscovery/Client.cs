using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Kdetcd.Standard.EtcdDiscovery
{
    public class Client : IDisposable
    {
        dotnet_etcd.EtcdClient _client;
        public Action<string> SrvDeleted;
        public Action FindNewSrv;
        public Action<string, string> SrvChanged;
        HashSet<Google.Protobuf.ByteString> klst;
        public Client(string host, int port, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            _client = new dotnet_etcd.EtcdClient(host, port, username, password, caCert, clientCert, clientKey, publicRootCa);
            klst = new HashSet<Google.Protobuf.ByteString>();
            //监听新增
            _client.WatchRange("/EtcdDiscovery/", new Action<Etcdserverpb.WatchResponse>((e) => {
                bool isklistContains = false;
                foreach (var item in e.Events)
                {
                    lock (klst)
                    {
                        isklistContains = klst.Contains(item.Kv.Key);
                    }
                    if (isklistContains)
                    {
                        switch (item.Type)
                        {
                            case Mvccpb.Event.Types.EventType.Put:
                               SrvChanged?.Invoke(item.Kv.Key.ToStringUtf8(),item.Kv.Value.ToStringUtf8());
                                break;
                            case Mvccpb.Event.Types.EventType.Delete:
                                SrvDeleted?.Invoke(item.Kv.Key.ToStringUtf8());
                                lock (klst)
                                {
                                    klst.RemoveWhere(c => c == item.Kv.Key);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                    else if (item.Type == Mvccpb.Event.Types.EventType.Put)
                    {
                        FindNewSrv?.Invoke();
                    }
                }
            }));
        }

        public string GetServer(string ServiceName, bool needWatch = true)
        {
            Google.Protobuf.ByteString key = Google.Protobuf.ByteString.CopyFromUtf8($"/EtcdDiscovery/{ServiceName}");

            var req = new Etcdserverpb.RangeRequest() { Key = key };
            var result = _client.GetAsync(req).Result;
            if (result.Kvs != null && result.Kvs.Count > 0)
            {
                lock (klst)
                {
                    if (needWatch && klst.Contains(key))
                    {

                        klst.Add(key);
                    }
                }
                return result.Kvs[0].Value.ToStringUtf8();
            }
            else
            {
                return null;
            }
        }

        public List<string> GetServerList()
        {
            return _client.GetRangeAsync($"/EtcdDiscovery/").Result.Kvs.Select(c => c.Key.ToStringUtf8()).ToList();
        }

        public async void Dispose() => await Task.Run(new Action(() => { _client.Dispose(); }));
    }
}
