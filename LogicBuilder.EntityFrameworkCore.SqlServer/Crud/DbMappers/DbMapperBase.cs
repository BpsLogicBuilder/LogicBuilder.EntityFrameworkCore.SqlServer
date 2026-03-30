using LogicBuilder.Data;
using System.Collections.Generic;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Crud.DbMappers
{
    internal class DbMapperBase<T>(IUnitOfWork unitOfWork) : IDbMapper<T> where T : class, IBaseData
    {
        #region Variables
        private readonly IUnitOfWork unitOfWork = unitOfWork;
        #endregion Variables

        #region Properties
        protected virtual GenericRepository<T> Repository { get { return this.unitOfWork.GetRepository<T>(); } }
        #endregion Properties

        #region Methods
        public virtual void AddChanges(ICollection<T> entities)
        {
            foreach (T entity in entities)
            {
                switch (entity.EntityState)
                {
                    case EntityStateType.Deleted:
                        this.Repository.Delete(entity);
                        break;
                    case EntityStateType.Modified:
                        this.Repository.Update(entity);
                        break;
                    case EntityStateType.Added:
                        this.Repository.Insert(entity);
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void AddGraphChanges(ICollection<T> entities)
        {
            foreach (T entity in entities)
            {
                switch (entity.EntityState)
                {
                    case EntityStateType.Deleted:
                        this.Repository.DeleteGraph(entity);
                        break;
                    case EntityStateType.Added:
                        this.Repository.InsertGraph(entity);
                        break;
                    case EntityStateType.Modified:
                        this.Repository.UpdateGraph(entity);
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion Methods
    }
}
