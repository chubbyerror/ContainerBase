using System;
using System.Collections.Generic;
using System.Text;

namespace Kdetcd.Standard.Locker
{
    public interface IClient
    {
        LockModel Lock(string lockname);
        UnLockResult UnLock(string lockname, long Id);
        WatchLockModel WatchLock(string lockname, long timeout);
    }

    class ZkLockerWatcher
    {
        public string Name { get; set; }
        public System.Threading.EventWaitHandle Event { get; set; }
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
}
