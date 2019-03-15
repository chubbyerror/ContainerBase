using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Text;

namespace KdZookeeper.Standard.ZooDiscovery
{
    public class Client: IDisposable
    {
        protected ZooKeeper _client = null;
        public bool Conneted = false;
        private const string Root= "/ZooDiscovery";
        public Action FindNewSrv;
        public Action<string> SrvDeleted;
        public Action<string,string> SrvChanged;
        public System.Threading.CancellationTokenSource Cancellation = new System.Threading.CancellationTokenSource();
        public Client(string Connstring, int SessionTimeout = -1)
        {

            var watcher = new SingleWatcher();
            watcher.RunProcess = new Action<WatchedEvent>((e) => {
                if (e.getState() == Watcher.Event.KeeperState.Disconnected)
                {
                    Conneted = false;
                    //zookeeper断开处理
                }
                var path = e.getPath();
                //监听处理
                switch (e.get_Type())
                {
                    case Watcher.Event.EventType.None:
                        break;
                    case Watcher.Event.EventType.NodeCreated:
                        break;
                    case Watcher.Event.EventType.NodeDeleted:
                        SrvDeleted?.Invoke(path.Substring(path.LastIndexOf("/") + 1));
                        break;
                    case Watcher.Event.EventType.NodeDataChanged:
                        var api = GetServer(path.Substring(path.LastIndexOf("/") + 1), false);
                        SrvChanged?.Invoke(path.Substring(path.LastIndexOf("/") + 1),api);
                        break;
                    case Watcher.Event.EventType.NodeChildrenChanged:
                        FindNewSrv?.Invoke();
                        break;
                    default:
                        break;
                }
            });
            //初始化客户端
            _client = new ZooKeeper(Connstring, SessionTimeout, watcher, false);
            Conneted = true;
            //初始化root节点
            OpenNode(Root);
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
                    if (ex.Message.IndexOf("NodeExistsException") == -1)
                    {
                        throw ex;
                    }
                }
            }
            _client.existsAsync(nodename, true);
            return result;
        }

        public string GetServer(string ServiceName,bool needWatch=true)
        {
            try
            {
                var fullid = _client.getDataAsync($"{Root}/{ServiceName}",needWatch).Result.Data;
                return Encoding.UTF8.GetString(fullid);
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("NoNodeException") == -1)
                {
                    throw ex;
                }
                else
                {
                    return "";
                }
            }
        }

        public List<string> GetServerList()
        {
            try
            {
                //get old Version
                return _client.getChildrenAsync($"{Root}",true).Result.Children;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async void Dispose() => await _client.closeAsync();
    }
}