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
}
