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
        public Action SrvDeleted, FindNewSrv;
        public Action<Dictionary<string, string>> SrvChanged;
        HashSet<Google.Protobuf.ByteString> klst;
        public Client(string host, int port, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            _client = new dotnet_etcd.EtcdClient(host, port, username, password, caCert, clientCert, clientKey, publicRootCa);
            klst = new HashSet<Google.Protobuf.ByteString>();
            //监听新增
            _client.WatchRange("/EtcdDiscovery/", new Action<Etcdserverpb.WatchResponse>((e) => {
                if (FindNewSrv!=null)
                {
                    List<Mvccpb.Event> newlst = null;
                    lock (klst)
                    {
                        newlst = e.Events.Where(c => !klst.Any(k => k == c.Kv.Key)).ToList();
                    }
                    if (klst.Count>0)
                    {
                        FindNewSrv?.Invoke();//可以委托的东西很多，暂时没有委托出去
                    }
                }
            }));
        }

        public Dictionary<string, string> GetServer(string ServiceName, bool needWatch = true)
        {
            Google.Protobuf.ByteString key = Google.Protobuf.ByteString.CopyFromUtf8($"/EtcdDiscovery/{ServiceName}");
            lock (klst)
            {
                klst.Add(key);
            }
            var req = new Etcdserverpb.RangeRequest() { Key = key };
            var result = _client.GetAsync(req).Result;
            if (result.Kvs != null && result.Kvs.Count > 0)
            {
                //watch
                _client.WatchRange($"/EtcdDiscovery/", new Action<Etcdserverpb.WatchResponse>((e) => {
                    if (needWatch)
                    {
                        foreach (var item in e.Events.Where(c => c.Kv.Key.ToStringUtf8() == $"/EtcdDiscovery/{ServiceName}").ToList())
                        {
                            if (item.Type == Mvccpb.Event.Types.EventType.Put)
                            {
                                Dictionary<string, string> api = KdCommon.Common.Deserialize<Dictionary<string, string>>(item.Kv.Value.ToByteArray());
                                SrvChanged?.Invoke(api);
                            }
                            else if (item.Type == Mvccpb.Event.Types.EventType.Delete)
                            {
                                SrvDeleted?.Invoke();
                            }
                        }
                    }
                }));
                return KdCommon.Common.Deserialize<Dictionary<string, string>>(result.Kvs[0].Value.ToByteArray());
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
