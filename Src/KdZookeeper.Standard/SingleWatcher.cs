using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KdZookeeper.Standard
{
    public class SingleWatcher : Watcher
    {
        public override Task process(WatchedEvent @event)
        {
            return Task.Run(()=> RunProcess?.Invoke(@event));
        }
        public Action<WatchedEvent> RunProcess;
    }
}
