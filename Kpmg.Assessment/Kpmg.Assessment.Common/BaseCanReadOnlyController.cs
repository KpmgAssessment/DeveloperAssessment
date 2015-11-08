using Kpmg.Assessment.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Kpmg.Assessment.Common
{
    public abstract class BaseReadOnlyController<T, U> : ApiController where U : IReadOnlyDataProvider<T>
    {
        protected internal U Database { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual U DatabaseProvider
        {
            get
            {
                if (Database == null)
                {
                    Database = GetDatabaseInstance<U>();
                }
                return Database;
            }

            set { Database = value; }
        }

        protected DU GetDatabaseInstance<DU>()
        {
            return Activator.CreateInstance<DU>();
        }

        [HttpGet]
        public virtual IHttpActionResult ListAll()
        {
            ICollection<T> result = Task.Run(() => { return DatabaseProvider.ListAll(); }).Result;

            return Ok(result);
        }

        //[HttpGet]
        //public virtual IHttpActionResult ListAllWithPaging([FromUri] JToken pagingData)
        //{
        //    int pageSize = 0, pageNumber = 1;

        //    ICollection<T> result = Task.Run(async () => { return await DataProvider.ListAllAsync(); }).Result;

        //    int totalCount = result.Count;
        //    double totalPages = Math.Ceiling((double)totalCount / pageSize);

        //    UrlHelper urlHelper = new UrlHelper(Request);
        //    string prevLink = pageNumber > 0 ? urlHelper.Link((typeof(T)).Name, new { pageNumber = pageNumber - 1, pageSize = pageSize }) : "";
        //    string nextLink = pageNumber < totalPages - 1 ? urlHelper.Link((typeof(T)).Name, new { pageNumber = pageNumber + 1, pageSize = pageSize }) : "";

        //    var paginationHeader = new
        //    {
        //        TotalCount = totalCount,
        //        TotalPages = totalPages,
        //        PrevPageLink = prevLink,
        //        NextPageLink = nextLink
        //    };

        //    HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

        //    result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        //    return Ok(result);

        //}

        [HttpGet]
        public virtual IHttpActionResult GetById(int id)
        {
            T result = Task.Run(() => { return DatabaseProvider.GetById(id); }).Result;

            return Ok(result);
        }

        protected override void Dispose(bool disposing)
        {
            if (DatabaseProvider != null)
            {
                DatabaseProvider.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
