using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DevelopBase.Cache
{
    /// <summary>
    /// Cache操作定义
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        T Get<T>(string key);
        /// <summary>
        /// 异步获取值
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);
        /// <summary>
        /// 设置键值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void Set(string key, string value);
        /// <summary>
        /// 设置键值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="timeout">超时</param>
        void Set(string key, string value, TimeSpan timeout);
        /// <summary>
        /// 异步设置键值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        Task SetAsync(string key, string value);
        /// <summary>
        /// 异步设置键值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        Task SetAsync(string key, string value, TimeSpan timeout);

        /// <summary>
        /// 删除键
        /// </summary>
        /// <param name="key">键</param>
        void Remove(string key);
        /// <summary>
        /// 异步删除键
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task RemoveAsync(string key);
        

    }
}
