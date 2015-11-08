using Kpmg.Assessment.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment
{
    public abstract class BaseCanReadDataProvider<T> : IReadOnlyDataProvider<T>
            where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal KpmgAssessmentEntities Database { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual KpmgAssessmentEntities DatabaseProvider
        {
            get
            {
                if(Database == null)
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
        protected KpmgAssessmentEntities GetDatabaseInstance<KpmgAssessmentEntities>()
        {
            return Activator.CreateInstance<KpmgAssessmentEntities>();
        }

        protected virtual IDbSet<T> EntitySet
        {
            get { return DatabaseProvider.Set<T>(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T GetById(int id)
        {
            DatabaseProvider.Configuration.LazyLoadingEnabled = false;
            return EntitySet.Find(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual ICollection<T> ListAll()
        {
            return EntitySet.AsEnumerable().ToList();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    DatabaseProvider.Dispose();
                }

                // Ideally i'd free unmanaged resources (unmanaged objects) by setting larges fields to null etc 
                // and override a finalizer below but as i haven't got any, so i'd pass :)


                disposedValue = true;
            }
        }

        // i'd override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // but as i haven't had to deal with unmanaged resources, i'd comment this out.. :)
        // ~BaseCanReadDataProvider() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // Added to correctly implement the disposable pattern
        public void Dispose()
        {
            Dispose(true);

            // I'd need to instruct the GC to suppress finalize only if if the finalizer is overridden above.
            // but as it isn't...i leave this commented out.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
