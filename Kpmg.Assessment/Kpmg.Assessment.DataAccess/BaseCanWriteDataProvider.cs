using EntityFramework.BulkInsert.Extensions;
using Kpmg.Assessment.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment
{
    public abstract class BaseCanWriteDataProvider<T> : BaseCanReadDataProvider<T>, ICanWriteDataProvider<T>
            where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal new KpmgAssessmentEntities Database { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual new KpmgAssessmentEntities DatabaseProvider
        {
            get
            {
                if (Database == null)
                {
                    Database = GetDatabaseInstance<KpmgAssessmentEntities>();
                }
                return Database;
            }

            set { Database = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="KpmgAssessmentEntities"></typeparam>
        /// <returns></returns>
        protected new KpmgAssessmentEntities GetDatabaseInstance<KpmgAssessmentEntities>()
        {
            return Activator.CreateInstance<KpmgAssessmentEntities>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        public virtual int Create(ICollection<T> entities)
        {
            List<T> result = new List<T>();
            BulkInsertOptions insertOptions = new BulkInsertOptions { BatchSize = 500, EnableStreaming = true };
            DatabaseProvider.BulkInsert(entities, insertOptions);

            int saved = Task.Run(async () => { return await DatabaseProvider.SaveChangesAsync(); }).Result;

            return saved;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public virtual int Create(T entity)
        {
            return Create(new T[] { entity });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int Delete(ICollection<T> entities)
        {
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.ForEach(entities, options, entity => { EntitySet.Remove(entity); });

            return Task.Run(async () => { return await DatabaseProvider.SaveChangesAsync(); }).Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual int Delete(T entity)
        {
            return Delete(new T[] { entity });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int Update(ICollection<T> entities)
        {
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.ForEach(entities, options, entity => 
            {
                DbEntityEntry dbEntityEntry = DatabaseProvider.Entry(entity);
                if(dbEntityEntry.State == System.Data.Entity.EntityState.Detached)
                {
                    dbEntityEntry.State = System.Data.Entity.EntityState.Modified;
                    Database.SaveChangesAsync();

                    //int id = Convert.ToInt32(dbEntityEntry.Property("Id").CurrentValue);
                    //List<T> itemCollection = DatabaseProvider.Set<T>().Where(item => true).ToList();
                    //T dbEntity = itemCollection.FirstOrDefault(item => MatchItemById(item, id));

                    //if(dbEntity != null)
                    //{
                    //    dbEntityEntry.State = System.Data.Entity.EntityState.Modified;
                    //    DatabaseProvider.SaveChanges();
                    //}
                }
            });

            return 0;
        }

        protected bool MatchItemById<T>(T item, int neededId)
        {
            bool result = false;

            if (item != null)
            {
                PropertyInfo idProperty = typeof(T).GetProperty("Id");

                if (idProperty != null)
                {
                    try
                    {
                        int idVal = Convert.ToInt32(idProperty.GetValue(item));
                        result = idVal == neededId;
                    }
                    catch (Exception ex)
                    {
                        //TODO => Log ex
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual int Update(T entity)
        {
            return Update(new T[] { entity });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (DatabaseProvider != null)
                    {
                        DatabaseProvider.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseDataProvider() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
