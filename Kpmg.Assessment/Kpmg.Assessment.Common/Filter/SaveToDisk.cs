using Kpmg.Assessment.MultipartMediaFormatter;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using System.Web.Http;

namespace Kpmg.Assessment.Common.Filter
{
    public class SaveToDisk : ActionFilterAttribute
    {
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            //If its not a multipart request...don't dig deep...reject the request.
            //Exception filter will intercept and return generic server error...
            if(!actionContext.Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            object upload;
            if(actionContext.ActionArguments.TryGetValue("upload", out upload))
            {
                FileUploadModel uploadModel = (FileUploadModel)upload;
                if (uploadModel != null && uploadModel.Binary!= null)
                {
                    //Was initially peeking the file's extension, but determined it was unncessary
                    // as it needs to be saved to disk regardless...
                    string fileExtension = Path.GetExtension(uploadModel.Binary.Filename);
                    string tempLocation = HttpContext.Current.Server.MapPath("~/App_Data");

                    if (!Directory.Exists(tempLocation))
                    {
                        Directory.CreateDirectory(tempLocation);
                    }
                    File.WriteAllBytes(Path.Combine(new[] { tempLocation, uploadModel.Binary.Filename }), uploadModel.Binary.Buffer);
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Settings.Default.NoPostedFile);
                }
            }

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}
