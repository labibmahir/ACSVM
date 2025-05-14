using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    /// <summary>
    /// Contains signatures of all generic methods.
    /// </summary>
    /// <typeparam name="T">T is a model class.</typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// The method is used to add data in the table.
        /// </summary>
        /// <param name="entity">Add data in the table.</param>
        /// <returns>Returns saved object.</returns>
        T Add(T entity);

        /// <summary>
        /// The method is used to retrieve data.
        /// </summary>
        /// <param name="id">Primary key.</param>
        /// <returns>Returns a row from the table as an object if primary key matches.</returns>
        T Get(Guid id);

        /// <summary>
        /// The method is used to retrieve data.
        /// </summary>
        /// <param name="id">Primary key.</param>
        /// <returns>Returns a row from the table as an object if primary key matches.</returns>
        T Get(int id);

        /// <summary>
        /// The method is used to get a list of data.
        /// </summary>
        /// <returns>Returns all rows as a list of data from the table.</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// add multi object to database
        /// </summary>
        /// <param name="entities"></param>
        void AddRange(IEnumerable<T> entities);

        /// <summary>
        /// The method is used to get a list of data.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>Returns matched rows as a list of data.</returns>
        Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> obj);

        /// <summary>
        /// The method is used to get a list of data.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>Returns matched rows as a list of data.</returns>
        Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> obj, Expression<Func<T, object>> next);

        /// <summary>
        /// The method is used to get a list of data.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>Returns matched rows as a list of data.</returns>
        Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// The method is used to retrieve first or default data.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>Returns first matched row as an object from the table.</returns>
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// The method is used to retrieve first or default data.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>Returns first matched row as an object from the table.</returns>
        Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// The method is used to update data in the table.
        /// </summary>
        /// <param name="id">Update data in the table.</param>

        T GetById(Guid id);

        /// <summary>
        /// The method is used to update data in the table.
        /// </summary>
        /// <param name="Key">Update data in the table.</param>
        /// 
        T GetById(int Key);

        /// <summary>
        /// Searches using primary key.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Searches using primary key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T?> GetByIdAsync(int key);

        void Update(T entity);

        /// <summary>
        /// The method is used to delete data from the table.
        /// </summary>
        /// <param name="entity">Delete data from the table.</param>
        void Delete(T entity);

        /// <summary>
        /// The method is used to load child table data.
        /// </summary>
        /// <param name="expressionList">load child table from the table.</param>
        Task<T> LoadWithChildAsync<TEntity>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] expressionList);

        Task<IEnumerable<T>> LoadListWithChildAsync<TEntity>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] expressionList);

        Task<IEnumerable<T>> LoadListWithChildAsync<TEntity>(Expression<Func<T, bool>> predicate, int skip, int take, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] expressionList);

        int Count(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] expressionList);

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}
