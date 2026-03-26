using LogicBuilder.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Crud.DataStores
{
    public interface IStore
    {
        Task<ICollection<T>> GetAsync<T>(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IQueryable<T>>? queryFunc = null) where T : class, IBaseData;
        Task<IQueryable<T>> GetQueryableAsync<T>(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IQueryable<T>>? queryableFunc = null) where T : class, IBaseData;
        Task<int> CountAsync<T>(Expression<Func<T, bool>>? filter = null) where T : class, IBaseData;
        Task<bool> SaveAsync<T>(ICollection<T> entities) where T : class, IBaseData;
        Task<bool> SaveGraphsAsync<T>(ICollection<T> entities) where T : class, IBaseData;
        Task<TReturn> QueryAsync<T, TReturn>(Func<IQueryable<T>, TReturn> queryableFunc) where T : class, IBaseData;
        void AddChanges<T>(ICollection<T> entities) where T : class, IBaseData;
        void AddGraphChanges<T>(ICollection<T> entities) where T : class, IBaseData;
        Task<bool> SaveChangesAsync();
        void ClearChangeTracker();
        void DetachAllEntries();
    }
}
