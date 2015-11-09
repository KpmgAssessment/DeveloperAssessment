using Kpmg.Assessment.UIClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Kpmg.Assessment.UIClient.Controllers
{
    public class UploadController : Controller
    {
        static object _lock = new object();

        // GET: TransactionData
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DoUpload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DoUpload(List<HttpPostedFileBase> fileUploads)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = tokenSource.Token };
            MultipartFormDataContent uploads = new MultipartFormDataContent();

            Parallel.ForEach(fileUploads, options, file =>
            {
                Stream fileStream = file.InputStream;

                //Essential to ensure thread safety especially as parallel.foreach will be spinning
                // concurrent threads based on the processor strength of the server
                lock(_lock)
                {
                    uploads.Add(new StreamContent(file.InputStream), "Binary", file.FileName);
                }
            });

            string endpoint = ConfigurationManager.AppSettings["UploadEndpoint"];
            FeedbackToClient serverResponse = ServerHelper.PostUploads(endpoint, uploads);

            ViewBag.UploadResponse = null;
            ViewBag.UploadResponse = serverResponse;

            return View();
        }

        public ActionResult UpdateTransactionData()
        {
            TransactionData data = TempData["TransactionData"] as TransactionData;
            return View(data);
        }

        public ActionResult UpdateResult()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UpdateTransactionData(TransactionData model)
        {
            if(ModelState.IsValid)
            {
                string endpoint = ConfigurationManager.AppSettings["BaseEndpoint"] + "TransactionData/Update";
                HttpResponseMessage result = ServerHelper.Put<TransactionData>(endpoint, model);

                result.EnsureSuccessStatusCode();

                //Update was a success, tempdata key for transaction data has 
                // no reason to live beyond this scope.
                TempData.Remove("TransactionData");
            }

            return RedirectToAction("UpdateResult", "Upload");
        }

        public ActionResult Delete(int id)
        { 
            string endpoint = ConfigurationManager.AppSettings["BaseEndpoint"] + "TransactionData/Delete?id=" + id;
            HttpResponseMessage result = ServerHelper.Delete(endpoint);
            result.EnsureSuccessStatusCode();

            return RedirectToAction("Index", "Home");
        }
    }
}