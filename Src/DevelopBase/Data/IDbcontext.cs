using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DevelopBase.Data
{
    public interface IDbContext
    {
        EntityEntry Add(object entity);
        void AddRange(IEnumerable<object> enties);
        EntityEntry Update(object entity);
        void UpdateRange(IEnumerable<object> enties);
        EntityEntry Remove(object entity);
        void RemoveRange(IEnumerable<object> enties);
        int SaveChanges();
    }
}