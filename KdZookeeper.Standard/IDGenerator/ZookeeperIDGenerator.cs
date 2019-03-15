using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Text;

namespace KdZookeeper.Standard
{
    public class ZookeeperIDGenerator:IDisposable
    {
        protected ZooKeeper _client = null;

        public ZookeeperIDGenerator(string Connstring, int SessionTimeout = -1)
        {

            var watcher = new SingleWatcher();
            //初始化客户端
            _client = new ZooKeeper(Connstring, SessionTimeout, watcher, false);
            //初始化root节点
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
            return result;
        }

        string path = "";
        string root = "";
        string idname = "";
        /// <summary>
        /// 设置id名，不同的id名分别计数，因此您可用通过多个id名实现多组id生成。
        /// <para>如果可用确认id名存在可直接使用GetID(string IdName)获取id，无需设置。</para>
        /// </summary>
        /// <param name="IdName"></param>
        public void SetName(string IdName)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                root = OpenNode("/IDGenerator");
            }
            path = OpenNode($"/IDGenerator/{IdName}");
            idname = IdName;
        }

        /// <summary>
        /// 获取id，恒常返回10位顺序数字，最大2147483648。
        /// <para>如未使用SetName(string IdName)设置默认id名将返回空。</para>
        /// </summary>
        /// <returns></returns>
        public string GetID()
        {
            return GetID(idname);
        }

        /// <summary>
        /// 获取id，恒常返回10位顺序数字，最大2147483648。
        /// <para>请自行确认已经设置过该idname。</para>
        /// </summary>
        /// <param name="IdName"></param>
        /// <returns></returns>
        public string GetID(string IdName)
        {
            try
            {
                var fullid = _client.createAsync($"{root}/{IdName}/id_", new byte[0], ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL_SEQUENTIAL).Result;
                return fullid.Substring(fullid.LastIndexOf("_") + 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async void Dispose() => await _client.closeAsync();
    }
}