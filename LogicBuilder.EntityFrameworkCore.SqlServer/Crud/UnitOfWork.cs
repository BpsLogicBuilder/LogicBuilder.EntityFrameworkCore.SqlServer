using LogicBuilder.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.Crud.DbMappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Crud
{
    internal class UnitOfWork : IUnitOfWork
    {
        internal UnitOfWork(DbContext context)
        {
            this.context = context;
        }

        #region Variables
        private bool disposed;
        private readonly DbContext context;
        #endregion Variables

        #region Properties
        public virtual DbContext Context
        {
            get { return this.context; }
        }

        Dictionary<Type, object>? repositoryDictionary;
        public virtual Dictionary<Type, object> RepositoryDictionary
        {
            get
            {
                this.repositoryDictionary ??= [];
                return this.repositoryDictionary;
            }
        }

        Dictionary<Type, object>? mapperDictionary;
        public virtual Dictionary<Type, object> MapperDictionary
        {
            get
            {
                this.mapperDictionary ??= [];
                return this.mapperDictionary;
            }
        }
        #endregion Properties

        #region Methods
        public virtual async Task<bool> SaveChangesAsync()
        {
            return (await this.Context.SaveChangesAsync()) > 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
                this.Context.Dispose();

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual GenericRepository<T> GetRepository<T>() where T : class, IBaseData
        {
            if (!RepositoryDictionary.ContainsKey(typeof(T)))
                RepositoryDictionary.Add(typeof(T), new GenericRepository<T>(this.Context));

            return (GenericRepository<T>)RepositoryDictionary[typeof(T)];
        }

        public virtual DbMapperBase<T> GetMapper<T>() where T : class, IBaseData
        {
            if (!MapperDictionary.ContainsKey(typeof(T)))
                MapperDictionary.Add(typeof(T), new DbMapperBase<T>(this));

            return (DbMapperBase<T>)MapperDictionary[typeof(T)];
        }
        #endregion Methods
    }
}
