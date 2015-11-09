using Kpmg.Assessment.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Kpmg.Assessment.Common
{
    public class BaseCanWriteController<T, V> : BaseReadOnlyController<T, V>
        where V : ICanWriteDataProvider<T>
    {
        protected internal new V Database { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual V ICanWriteDataProvider
        {
            get
            {
                if (Database == null)
                {
                    Database = GetDatabaseInstance<V>();
                }
                return Database;
            }

            set { Database = value; }
        }

        protected new DV GetDatabaseInstance<DV>()
        {
            return Activator.CreateInstance<DV>();
        }

        //The intention here is to hide the base GetProvider (in BaseCanReadController)
        //simply because DT in both impl are of different types. Hence the new keyword
        protected W GetProvider<W, DT>() where W : ICanWriteDataProvider<DT>
        {
            return Activator.CreateInstance<W>();
        }

        [HttpPost]
        public virtual IHttpActionResult Create(ICollection<T> entities)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public virtual IHttpActionResult Create(T entity)
        {
            int result = Task.Run(() => { return ICanWriteDataProvider.Create(entity); }).Result;

            return Ok();
        }

        [HttpDelete]
        public IHttpActionResult DeleteAsync(ICollection<int> ids)
        {
            int result = Task.Run(() => { return ICanWriteDataProvider.Delete(ids); }).Result;

            return Ok(result);
        }

        [HttpDelete]
        public IHttpActionResult DeleteAsync(int id)
        {
            int result = ICanWriteDataProvider.Delete(id);
            return Ok(result);
        }

        [HttpPut]
        public IHttpActionResult UpdateAsync(ICollection<T> entities)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        public IHttpActionResult UpdateAsync(T entity)
        {
            int result = Task.Run(() => { return ICanWriteDataProvider.Update(entity); }).Result;

            return Ok(result);
        }

        protected override void Dispose(bool disposing)
        {
            if (ICanWriteDataProvider != null)
            {
                ICanWriteDataProvider.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
