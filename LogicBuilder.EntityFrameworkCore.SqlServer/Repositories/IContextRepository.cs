using LogicBuilder.Data;
using LogicBuilder.Domain;
using LogicBuilder.Expressions.Utils.Expansions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Repositories
{
    public interface IContextRepository
    {
        Task<ICollection<TModel>> GetAsync<TModel, TData>(Expression<Func<TModel, bool>>? filter = null, Expression<Func<IQueryable<TModel>, IQueryable<TModel>>>? queryFunc = null, SelectExpandDefinition? selectExpandDefinition = null)
            where TModel : class, IBaseModel
            where TData : class, IBaseData;

        Task<int> CountAsync<TModel, TData>(Expression<Func<TModel, bool>>? filter = null)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        Task<TModelReturn> QueryAsync<TModel, TData, TModelReturn, TDataReturn>(Expression<Func<IQueryable<TModel>, TModelReturn>> queryFunc, SelectExpandDefinition? selectExpandDefinition = null)//NOSONAR -  in this case, reducing the number of generic parameters adds more complexity and would not be beneficial to readability.
            where TModel : class, IBaseModel
            where TData : class, IBaseData;

        Task<bool> SaveAsync<TModel, TData>(TModel entity)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        Task<bool> SaveAsync<TModel, TData>(ICollection<TModel> entities)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        Task<bool> SaveGraphAsync<TModel, TData>(TModel entity)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        Task<bool> SaveGraphsAsync<TModel, TData>(ICollection<TModel> entities)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        Task<bool> DeleteAsync<TModel, TData>(System.Linq.Expressions.Expression<Func<TModel, bool>>? filter = null)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        void AddChange<TModel, TData>(TModel entity)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        void AddChanges<TModel, TData>(ICollection<TModel> entities)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        void AddGraphChange<TModel, TData>(TModel entity)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        void AddGraphChanges<TModel, TData>(ICollection<TModel> entities)
            where TModel : IBaseModel
            where TData : class, IBaseData;

        Task<bool> SaveChangesAsync();

        void ClearChangeTracker();

        void DetachAllEntries();
    }
}
