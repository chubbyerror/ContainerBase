using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Text;

namespace KdZookeeper.Standard.ZooDiscovery
{
    public class Server: IDisposable
    {
        protected ZooKeeper _client = null;
        public bool Conneted = false;
        public Server(string Connstring, int SessionTimeout = -1)
        {

            var watcher = new SingleWatcher();
            watcher.RunProcess = new Action<WatchedEvent>((e) =>
            {
                if (e.getState() == Watcher.Event.KeeperState.Disconnected)
                {
                    Conneted = false;
                    //zookeeper断开处理
                }
            });
            //初始化客户端
            _client = new ZooKeeper(Connstring, SessionTimeout, watcher, false);
            Conneted = true;
            //初始化root节点
            OpenNode("/ZooDiscovery");
        }

        private string OpenNode(string nodename)
        {
            string result = "";
            Stat NodeExists = null;
            while (NodeExists == null)
            {
                try
                {
                    NodeExists = _client.existsAsync(nodename).Result;
                    if (NodeExists == null)
                    {
                        result = _client.createAsync(nodename, new byte[0], ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT).Result;
                    }
                    else
                    {
                        result = nodename;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.HResult!= (int)KeeperException.Code.NODEEXISTS)
                    {
                        throw ex;
                    }
                }
            }
            return result;
        }

        public string Register(string ServiceName,string SrvInfo)
        {
            try
            {
                var fullid = _client.createAsync($"/ZooDiscovery/{ServiceName}", Encoding.UTF8.GetBytes(SrvInfo), ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL).Result;
                return fullid;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(string ServiceName, string SrvInfo)
        {
            try
            {
                //get old Version
                int ver = _client.getDataAsync($"/ZooDiscovery/{ServiceName}").Result.Stat.getVersion();
                //set data
                Stat stat= _client.setDataAsync($"/ZooDiscovery/{ServiceName}", Encoding.UTF8.GetBytes(SrvInfo)).Result;
                //compara Version
                return ver !=stat.getVersion();
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
                _client.deleteAsync($"/ZooDiscovery/{ServiceName}").Wait();
            }
            catch (Exception ex )
            {
                if (ex.HResult!= (int)KeeperException.Code.NONODE)
                {
                    throw ex;
                }
            }
        }

        public async void Dispose() => await _client.closeAsync();
    }
}