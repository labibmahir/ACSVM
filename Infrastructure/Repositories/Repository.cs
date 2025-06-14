using Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of IRepository interface.
    /// </summary>
    /// <typeparam name="T">T is a model class.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DataContext context;

        public Repository(DataContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Creates a row in the table.
        /// </summary>
        /// <param name="entity">Object to be saved in the table as a row.</param>
        /// <returns>Saves object.</returns>
        public T Add(T entity)
        {
            try
            {
                return context.Set<T>().Add(entity).Entity;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a row from the table as an object if primary key matches.
        /// </summary>
        /// <param name="id">Primary key.</param>
        /// <returns>Retrieves object.</returns>
        public T Get(Guid id)
        {
            try
            {
                return context.Set<T>().Find(id);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a row from the table as an object if primary key matches.
        /// </summary>
        /// <param name="id">Primary key.</param>
        /// <returns>Retrieves object.</returns>
        public T Get(int id)
        {
            try
            {
                return context.Set<T>().Find(id);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns all rows as a list of objects from the table.
        /// </summary>
        /// <returns>List of objects.</returns>
        public IEnumerable<T> GetAll()
        {
            try
            {
                return context.Set<T>()
                    .AsQueryable()
                    .AsNoTracking()
                    .ToList();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Add multi object to database
        /// </summary>
        /// <param name="entities"></param>
        public void AddRange(IEnumerable<T> entities)
        {
            context.Set<T>().AddRange(entities);
        }

        /// <summary>
        /// Returns matched rows as a list of objects.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>List of objects.</returns>
        public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await context.Set<T>()
                    .AsQueryable()
                    .AsNoTracking()
                    .Where(predicate)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns matched rows as a list of objects.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>List of objects.</returns>
        public virtual async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> obj)
        {
            try
            {
                return await context.Set<T>()
                    .AsQueryable()
                    .Where(predicate)
                    .Include(obj)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns matched rows as a list of objects.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>List of objects.</returns>
        public virtual async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> obj, Expression<Func<T, object>> next)
        {
            try
            {
                return await context.Set<T>()
                      .AsQueryable()
                      .Where(predicate)
                      .Include(obj)
                      .Include(next)
                      .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns first matched row as an object from the table.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>Retrieved object.</returns>
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await context.Set<T>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(predicate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returns first matched row as an object from the table.
        /// </summary>
        /// <param name="predicate">Custom LINQ expression.</param>
        /// <returns>Retrieved object.</returns>
        public async Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await context.Set<T>()
                    .AsNoTracking()
                    .OrderByDescending(predicate)
                    .FirstOrDefaultAsync(predicate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Updates a row in the table.
        /// </summary>
        /// <param name="entity">Object to be updated.</param>
        public void Update(T entity)
        {
            //try
            //{
            //    context.Entry(entity).State = EntityState.Modified;
            //    context.Set<T>().Update(entity);
            //}
            //catch
            //{
            //    throw;
            //}
            try
            {
                var entry = context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    var key = context.Model.FindEntityType(typeof(T)).FindPrimaryKey();
                    var keyValues = key.Properties.Select(p => p.PropertyInfo.GetValue(entity)).ToArray();

                    var trackedEntity = context.Set<T>().Local
                        .FirstOrDefault(e =>
                        {
                            var trackedKeyValues = key.Properties.Select(p => p.PropertyInfo.GetValue(e)).ToArray();
                            return trackedKeyValues.SequenceEqual(keyValues);
                        });

                    if (trackedEntity != null)
                    {
                        context.Entry(trackedEntity).State = EntityState.Detached;
                    }

                    context.Attach(entity);
                }

                context.Entry(entity).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                // You can log the exception here if needed
                throw;
            }
        }

        /// <summary>
        /// Deletes a row from the table.
        /// </summary>
        /// <param name="entity">Object to be deleted.</param>
        public void Delete(T entity)
        {
            try
            {
                context.Set<T>().Remove(entity);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Include child a row from the table.
        /// </summary>
        /// <param name="entity">Object to be deleted.</param>
        public async Task<T> LoadWithChildWithOrderByAsync<TEntity>(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] expressionList)
        {
            var query = context.Set<T>().AsQueryable();

            foreach (var expression in expressionList)
            {
                query = query.Include(expression);
            }

            if (orderBy != null)
                query = orderBy(query);

            return await query.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="expressionList"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<T> LoadWithChildAsync<TEntity>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] expressionList)
        {
            var query = context.Set<T>().AsQueryable();

            foreach (var expression in expressionList)
            {
                query = query.Include(expression);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="expressionList"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<T>> LoadListWithChildAsync<TEntity>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] expressionList)
        {
            var query = context.Set<T>().AsQueryable();

            foreach (var expression in expressionList)
            {
                query = query.Include(expression);
            }

            return await query.Where(predicate).ToListAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="orderBy"></param>
        /// <param name="expressionList"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<T>> LoadListWithChildAsync<TEntity>(Expression<Func<T, bool>> predicate, int skip, int take, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] expressionList)
        {
            var query = context.Set<T>().AsQueryable();

            foreach (var expression in expressionList)
            {
                query = query.Include(expression);
            }
            if (orderBy != null)
                query = orderBy(query);

            return await query.Where(predicate).Skip(skip).Take(take).ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="groupByKeySelector"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="orderBy"></param>
        /// <param name="expressionList"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<IGrouping<string, T>>> LoadGroupedListWithChildAsync<TEntity>(Expression<Func<T, bool>> predicate, Func<T, string> groupByKeySelector, int skip, int take, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] expressionList)
        {
            var query = context.Set<T>().AsQueryable();

            foreach (var expression in expressionList)
            {
                query = query.Include(expression);
            }

            if (orderBy != null)
                query = orderBy(query);

            var groupedQuery = query.Where(predicate).GroupBy(groupByKeySelector).AsEnumerable().Skip(skip).Take(take);

            return await Task.Run(() => groupedQuery);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="expressionList"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] expressionList)
        {
            foreach (var expression in expressionList)
            {
                context.Set<T>().Include(expression);
            }

            return await context.Set<T>().Where(filter).CountAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T GetById(Guid id)
        {
            try
            {
                var entity = context.Set<T>().Find(id);
                context.Entry(entity).State = EntityState.Detached;
                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public T GetById(int Key)
        {
            try
            {
                var entity = context.Set<T>().Find(Key);
                context.Entry(entity).State = EntityState.Detached;
                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<T?> GetByIdAsync(Guid id)
        {
            var entity = await context.Set<T>().FindAsync(id);
            if (entity == null)
            {
                return null;
            }
            context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T?> GetByIdAsync(int key)
        {
            var entity = await context.Set<T>().FindAsync(key);
            if (entity == null)
            {
                return null;
            }

            context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            return context.Entry(entity);
        }
    }
}
