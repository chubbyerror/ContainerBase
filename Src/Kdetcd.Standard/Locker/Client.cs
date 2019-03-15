using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Kdetcd.Standard.Locker
{
    public class Client : IClient ,IDisposable
    {
        HashSet<ZkLockerWatcher> _lockerList = new HashSet<ZkLockerWatcher>();
        HashSet<dotnet_etcd.EtcdClient> _clients = new HashSet<dotnet_etcd.EtcdClient>();
        dotnet_etcd.EtcdClient _client;
        int _locklease = 10;
        bool IsDisposing = false;
        public Client(string host, int port, int locklease=10, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            _client = new dotnet_etcd.EtcdClient(host, port, username, password, caCert, clientCert, clientKey, publicRootCa);
            _locklease = locklease;

            //获取集群中全部客户端制作客户端列表
            foreach (var item in _client.MemberList(new Etcdserverpb.MemberListRequest()).Members)
            {
                if (item.ClientURLs.Count > 0)
                {
                    string[] baseurl = item.ClientURLs[0].Substring(item.ClientURLs[0].LastIndexOf("/") + 1).Split(':');
                    string clienthost = baseurl[0] == "0.0.0.0" ? host : baseurl[0];
                    int clientport = int.Parse(baseurl[1]);
                    _clients.Add(new dotnet_etcd.EtcdClient(clienthost, clientport, username, password, caCert, clientCert, clientKey, publicRootCa));
                }
            }

            //后台自延期部分
            System.Threading.Tasks.Task.Run(() =>
            {
                while (!IsDisposing)
                {
                    //休眠 TTL/2 时间，防止执行过程导致的node丢失
                    System.Threading.Thread.Sleep(_locklease * 500);

                    //获取客户端全部node
                    Dictionary<Google.Protobuf.ByteString, Google.Protobuf.ByteString> lockers = null;
                    lock (_lockerList)
                    {
                        lockers = _lockerList
                        .ToDictionary(c => Google.Protobuf.ByteString.CopyFromUtf8(c.Name)
                        , c => Google.Protobuf.ByteString.CopyFromUtf8(c.Id.ToString()));
                    }

                    //生成lease
                    Etcdserverpb.LeaseGrantRequest request = new Etcdserverpb.LeaseGrantRequest();
                    request.TTL = _locklease;
                    var lease = _client.LeaseGrant(request);
                    //更新全部node
                    foreach (var item in lockers)
                    {
                        _client.Put(new Etcdserverpb.PutRequest()
                        {
                            Key = item.Key,
                            Value = item.Value,
                            Lease = lease.ID
                        });
                    }
                }
            });
        }

        //根据客户端列表返回可用的第一个客户端
        private dotnet_etcd.EtcdClient bestClient(HashSet<dotnet_etcd.EtcdClient> clients)
        {
            foreach (var client in clients)
            {
                try
                {
                    client.Status(new Etcdserverpb.StatusRequest());
                    return client;
                }
                catch (Exception)
                {
                    //throw;
                }
            }
            return null;
        }

        public LockModel Lock(string lockname)
        {
            LockModel @lock = new LockModel();
            Etcdserverpb.RangeResponse oldlockers = null;
            //use local cache speed up.
            lock (_lockerList)
            {
                var locallst = _lockerList.Where(c => c.Name == lockname).ToList();
                if (locallst.Count > 0)
                {
                    @lock.Result = LockResult.LockExists;
                    @lock.Id = locallst.FirstOrDefault().Id;
                    return @lock;
                }
            }
            //confirm lock not exists.
            try
            {
                oldlockers = _client.GetAsync($"/locks/{lockname}").Result;
            }
            catch (Exception ex)
            {

                @lock.Id = 0;
                @lock.Result = LockResult.Fail;
                lock (_client)
                {
                    _client = bestClient(_clients);
                }
                return @lock;
            }

            //return lockid to locker .
            if (oldlockers?.Kvs.Count > 0)
            {
                @lock.Id = long.Parse(oldlockers.Kvs.FirstOrDefault().Value.ToStringUtf8());
                @lock.Result = LockResult.LockExists;
            }
            else
            {
                //creat lock id
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                @lock.Id = (long)ts.TotalMilliseconds;
                @lock.Result = LockResult.Success;
                try
                {
                    Etcdserverpb.LeaseGrantRequest request = new Etcdserverpb.LeaseGrantRequest();
                    request.TTL = _locklease;
                    var lease= _client.LeaseGrant(request);
                    Etcdserverpb.PutRequest put = new Etcdserverpb.PutRequest();
                    put.Key =Google.Protobuf.ByteString.CopyFromUtf8(($"/locks/{lockname}"));
                    put.Value = Google.Protobuf.ByteString.CopyFromUtf8(@lock.Id.ToString());
                    put.Lease = lease.ID;
                    _client.Put(put);
                    //add to cache and srv .
                    _client.Put($"/locks/{lockname}", @lock.Id.ToString());
                    lock (_lockerList)
                    {
                        _lockerList.Add(new ZkLockerWatcher() { Event = null, Id = @lock.Id, Name = lockname });
                    }
                }
                catch (Exception ex)
                {
                    lock (_client)
                    {
                        _client = bestClient(_clients);
                    }
                    @lock.Result = LockResult.Fail;
                }
            }
            return @lock;
        }

        public UnLockResult UnLock(string lockname, long Id)
        {
            ZkLockerWatcher watcher = null;
            bool localhas = false;
            bool srvhas = false;
            lock (_lockerList)
            {
                var locallst = _lockerList.Where(c => c.Name == lockname).ToList();
                if (locallst.Count > 0)
                {
                    if (locallst.FirstOrDefault().Id != Id)
                    {
                        return UnLockResult.NotSameLockId;
                    }
                    else
                    {
                        watcher = locallst.FirstOrDefault();
                        localhas = true;
                    }
                }
            }

            if (!localhas)
            {
                Etcdserverpb.RangeResponse oldlockers = null;
                try
                {
                    oldlockers = _client.GetAsync($"/locks/{lockname}").Result;
                }
                catch (Exception ex)
                {
                    lock (_client)
                    {
                        _client = bestClient(_clients);
                    }
                    return UnLockResult.Fail;
                }
                if (oldlockers?.Kvs.Count > 0)
                {
                    if (long.Parse(oldlockers.Kvs[0].Value.ToStringUtf8()) != Id)
                    {
                        return UnLockResult.NotSameLockId;
                    }
                    else
                    {
                        srvhas = true;
                    }
                }
            }

            if (localhas || srvhas)
            {

                long deled = 0;
                try
                {
                    deled = _client.DeleteRangeAsync($"/locks/{lockname}").Result.Deleted;
                    if (localhas)
                    {
                        lock (_lockerList)
                        {
                            _lockerList.RemoveWhere(c=>c.Id==Id &&c.Name==lockname);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (_client)
                    {
                        _client = bestClient(_clients);
                    }
                    return UnLockResult.Fail;
                }

                if (deled > 0)
                {
                    return UnLockResult.Success;
                }
                else
                {
                    return UnLockResult.NotExists;
                }
            }
            else
            {
                return UnLockResult.NotExists;
            }
        }

        public WatchLockModel WatchLock(string lockname, long timeout)
        {
            var Event = new System.Threading.ManualResetEvent(false);

            //get id and confirm lock exists .
            var node = _client.GetValAsync($"/locks/{lockname}").Result;
            long Id = 0;
            if (!long.TryParse(node,out Id))
            {
                return new WatchLockModel() { Id= 0, Result = WatcherResult.NotExists };
            }

            //at etcd use single watch on lock.
            _client.Watch($"/locks/{lockname}", new Action<dotnet_etcd.WatchEvent[]>((e) => {
                foreach (var item in e)
                {
                    if (item.Type== Mvccpb.Event.Types.EventType.Delete && item.Key== $"/locks/{lockname}")
                    {
                        Event.Set();
                    }
                }
            }));

            //timeout or unlock
            var action = System.Threading.WaitHandle.WaitAny(new System.Threading.WaitHandle[] { Event }, TimeSpan.FromMilliseconds(timeout));
            if (action == System.Threading.WaitHandle.WaitTimeout)
            {
                return new WatchLockModel() { Id = Id, Result = WatcherResult.TimeOuit };
            }
            else
            {
                return new WatchLockModel() { Id = Id, Result = WatcherResult.UnLocked };
            }
        }

        public void Dispose()
        {
            if (!IsDisposing)
            {
                IsDisposing = true;
                _client.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
