using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Net;
using System.Net.Http;

namespace Kpmg.Assessment.Common.Filter
{
    public class TransformExceptionFilter : ExceptionFilterAttribute
    {
        public override async Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            await base.OnExceptionAsync(actionExecutedContext, cancellationToken);

            //Log exception details...

            //Send cleaned up error msg
            actionExecutedContext.ActionContext.Response = actionExecutedContext
                .ActionContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, Settings.Default.ServerError);
        }
    }
}
