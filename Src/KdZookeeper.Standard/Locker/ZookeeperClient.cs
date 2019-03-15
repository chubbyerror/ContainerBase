using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using org.apache.zookeeper;
using org.apache.zookeeper.data;

namespace KdZookeeper.Standard
{

    class ZkLockerWatcher
    {
        public string Name { get; set; }
        public EventWaitHandle Event { get; set; }
        public WatcherResult ZooEvent { get; set; } = WatcherResult.TimeOuit;
        public long Id = 0;
    }

    public enum WatcherResult
    {
        /// <summary>
        /// 已解锁
        /// </summary>
        UnLocked,
        /// <summary>
        /// 锁不存在
        /// </summary>
        NotExists,
        /// <summary>
        /// 超时
        /// </summary>
        TimeOuit,
        /// <summary>
        /// 与服务器断开连接
        /// </summary>
        Disconnected,
    }

    public enum UnLockResult
    {
        /// <summary>
        /// 解锁成功
        /// </summary>
        Success,
        /// <summary>
        /// 锁不存在
        /// </summary>
        NotExists,
        /// <summary>
        /// 解锁失败
        /// </summary>
        Fail,
        /// <summary>
        /// 解锁失败，不是请求解锁的LockId
        /// </summary>
        NotSameLockId,
    }

    public enum LockResult
    {
        /// <summary>
        /// 锁成功
        /// </summary>
        Success,
        /// <summary>
        /// 锁已存在
        /// </summary>
        LockExists,
        /// <summary>
        /// 锁失败
        /// </summary>
        Fail,
    }

    public class WatchLockModel
    {
        /// <summary>
        /// watcher结果
        /// </summary>
        public WatcherResult Result { get; set; }
        /// <summary>
        /// watcher锁对象id
        /// </summary>
        public long Id { get; set; }
    }

    public class LockModel
    {
        /// <summary>
        /// 是否锁成功
        /// </summary>
        public LockResult Result { get; set; }
        /// <summary>
        /// 锁成功返回锁id，否则返回0或已有锁id
        /// </summary>
        public long Id { get; set; }
    }

    public class ZookeeperClient
    {
        private SingleWatcher Watcher;
        private HashSet<ZkLockerWatcher> _lockerList = new HashSet<ZkLockerWatcher>();
        protected ZooKeeper _client = null;

        public ZookeeperClient(string Connstring, int SessionTimeout = -1)
        {

            var watcher = new SingleWatcher();
            Watcher = watcher;
            Watcher.RunProcess = new Action<WatchedEvent>((e) =>
            {
                if (e.getState()== org.apache.zookeeper.Watcher.Event.KeeperState.Disconnected)
                {
                    lock (_lockerList)
                    {
                        foreach (var item in _lockerList)
                        {
                            item.ZooEvent = WatcherResult.Disconnected;
                            item.Event.Set();
                        }
                        _lockerList.Clear();
                    }
                }
                else if (e.get_Type() == org.apache.zookeeper.Watcher.Event.EventType.NodeDeleted)
                {
                    var path = e.getPath();
                    lock (_lockerList)
                    {
                        var node = _lockerList.FirstOrDefault(w => $"/locks/{w.Name}" == path);
                        if (!string.IsNullOrWhiteSpace(node.Name))
                        {
                            node.ZooEvent = WatcherResult.UnLocked;
                            node.Event?.Set();
                            _lockerList.Remove(node);
                        }
                    }
                }
            });
            //初始化客户端
            _client = new ZooKeeper(Connstring, SessionTimeout, watcher, false);
            //初始化root节点
        }

        /// <summary>
        /// 加锁方法
        /// </summary>
        /// <param name="lockname">锁名称</param>
        /// <returns>{加锁结果 Result ,锁id Id}</returns>
        public LockModel Lock(string lockname)
        {
            lock (_lockerList)
            {
                var node = _lockerList.FirstOrDefault(c => c.Name == lockname);
                if (node!=null)
                {
                    return new LockModel() { Result = LockResult.LockExists, Id = node.Id };
                }
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_client.createAsync($"/locks/{lockname}", new byte[0], ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL).Result))
                {
                    //增加监听
                    var watherResult = _client.existsAsync($"/locks/{lockname}", true).Result;

                    //增加到本地缓存
                    lock (_lockerList)
                    {
                        _lockerList.Add(new ZkLockerWatcher() { Name = lockname, Event = null, Id = watherResult?.getCtime() ?? 0 });
                    }

                    return new LockModel() { Result = LockResult.Success, Id = watherResult.getCtime() }; 
                }
                else
                {
                    return new LockModel() { Result = LockResult.Fail, Id = 0 };
                }
            }
            catch (Exception)
            {
                return new LockModel() { Result = LockResult.Fail, Id = 0 };
            }
        }
        
        /// <summary>
        /// 注意这个解锁方法会不进行id比对直接解锁，将会强制解锁所有锁，如果您不了解请使用比对id的解锁方法。
        /// </summary>
        /// <param name="lockname">锁名称</param>
        /// <returns>解锁结果枚举</returns>
        [Obsolete]
        public UnLockResult UnLock(string lockname)
        {
            lock (_lockerList)
            {
                if (_lockerList.Count(c => lockname == c.Name) == 0)
                {
                    return UnLockResult.NotExists;
                }
            }

            try
            {
                _client.deleteAsync($"/locks/{lockname}").Wait();
                return UnLockResult.Success;
            }
            catch (Exception)
            {
                //throw;
            }

            return UnLockResult.Fail;
        }

        /// <summary>
        /// 解锁方法，在比对锁名和id都相等后尝试解锁，返回解锁结果
        /// </summary>
        /// <param name="lockname">锁名称</param>
        /// <param name="Id">锁id</param>
        /// <returns>解锁结果枚举</returns>
        public UnLockResult UnLock(string lockname, long Id)
        {
            //锁id必须大于0
            if (Id==0)
            {
                throw new Exception("can't unlock any lock when id = 0 ");
            }
            long nodeid = 0;
            bool localexists = false;

            //查看本地缓存是否有锁，如果有从缓存取id
            lock (_lockerList)
            {
                var localnode = _lockerList.FirstOrDefault(c => lockname == c.Name);
                localexists = localnode != null;
                if (localexists)
                {
                    nodeid = localnode.Id;
                }
            }

            //缓存无锁从zookeeper尝试获取锁id
            if (!localexists)
            {
                var node = _client.existsAsync($"/locks/{lockname}").Result;
                nodeid = node.getCtime();
                if (node == null)
                {
                    //本地库和zookeeper都不存在，返回锁不存在
                    return UnLockResult.NotExists;
                }
            }

            //比对id是否相等
            if (nodeid != Id)
            {
                //返回id不匹配解锁失败
                return UnLockResult.NotSameLockId;
            }

            try
            {
                if (nodeid == Id)
                {
                    _client.deleteAsync($"/locks/{lockname}").Wait();
                    //成功解锁
                    return UnLockResult.Success;
                }
            }
            catch (Exception)
            {
                //throw;
            }
            //屏蔽可能的catch，返回解锁失败
            return UnLockResult.Fail;
        }

        /// <summary>
        /// 阻塞监视锁方法
        /// </summary>
        /// <param name="lockname">锁名称</param>
        /// <param name="timeout">阻塞超时时间</param>
        /// <returns>{监视结果 Result ,锁id Id}</returns>
        public WatchLockModel WatchLock(string lockname,long timeout)
        {
            EventWaitHandle handle = null;
            WatcherResult zooevent = WatcherResult.TimeOuit;
            long Id = 0;
            lock (_lockerList)
            {
                var node = _lockerList.FirstOrDefault(c => c.Name == lockname);
                if (node == null)
                {
                    return new WatchLockModel() { Id = 0, Result = WatcherResult.NotExists };
                }
                node.Event = node.Event?? new ManualResetEvent(false);
                handle = node.Event;
                zooevent = node.ZooEvent;
                Id = node.Id;
            }

            var action = WaitHandle.WaitAny(new WaitHandle[] { handle }, TimeSpan.FromMilliseconds(timeout));
            if (action == WaitHandle.WaitTimeout)
            {
                return new WatchLockModel() { Id = Id, Result = WatcherResult.TimeOuit };
            }
            else
            {
                if (zooevent == WatcherResult.Disconnected)
                {
                    return new WatchLockModel() { Id = Id, Result = WatcherResult.Disconnected };
                }
                else if (zooevent == WatcherResult.UnLocked)
                {
                    return new WatchLockModel() { Id = Id, Result = WatcherResult.UnLocked };
                }
                else
                {
                    return new WatchLockModel() { Id = Id, Result = zooevent };
                }
            }
        }
    }
}
