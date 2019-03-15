using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
namespace DevelopBase.Data
{
    public abstract class DbContextBase : DbContext
    {
        private string _connectionString = "";
        protected string ConnectionString
        {
            get => _connectionString;
        }
        public DbContextBase(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException();
            }
            _connectionString = connectionString;

        }
        /// <summary>
        /// 设置数据库驱动
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connectionString"></param>
        public abstract void SetDriver(DbContextOptionsBuilder options, string connectionString);
        protected IQueryable<T> GetEntity<T>() where T : class
        {
            return Set<T>().AsQueryable();
        }


    }
}
