using Kpmg.Assessment.Common.Filter;
using Kpmg.Assessment.MultipartMediaFormatter;
using Kpmg.Assessment.TaskHandler;
using Kpmg.Assessment.TaskHandler.ClassAdditions;
using Kpmg.Assessment.TaskHandler.Interfaces;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Kpmg.Assessment.Common
{
    public class TransactionDataController : BaseCanWriteController<TransactionData, TransactionDataProvider>
    {
        static int batchSize = int.Parse(Settings.Default.BatchSize);
        static int queueLimit = int.Parse(Settings.Default.QueueLimit);
        string tempDirectory = HttpContext.Current.Server.MapPath("~/App_Data");

        BlockingCollection<TransactionDataModel> _dataQueue = new BlockingCollection<TransactionDataModel>(queueLimit);
        BlockingCollection<ValidationResultModel> _validationResultQueue = new BlockingCollection<ValidationResultModel>(queueLimit);

        [Dependency]
        public ICanProduce<TransactionDataModel, ValidationResultModel> Producer { get; set; }

        [Dependency]
        public ICanConsume<TransactionDataModel, ValidationResultModel> Consumer { get; set; }

        public new TransactionDataProvider ICanWriteDataProvider { get; set; }

        public TransactionDataController() : base()
        {
            ICanWriteDataProvider = new TransactionDataProvider();
        }

        /// <summary>
        /// The SaveToDisk filter intercepts the request and saves the file to disk.
        /// If the filter can't detect the uploaded file, it returns an 400 response back to client
        /// and this action won't get executed.
        /// </summary>
        /// <param name="upload"></param>
        /// <returns></returns>
        [HttpPost]
        [SaveToDisk]
        public IHttpActionResult HandleFiles(FileUploadModel upload)
        {
            upload = null;
            Producer.TemporaryDirectory = tempDirectory;
            ProducerTaskResult producerResult = Producer.TriggerProducer(ref _dataQueue, ref _validationResultQueue);

            ICollection<ValidationResult> uploadFailures = Consumer.TriggerConsumers(ref _dataQueue, ref _validationResultQueue);
            FeedbackToClient feedback = new FeedbackToClient { SuccessUploadCount = producerResult.ValidCount, UploadValidationFailures = uploadFailures };

            string feedbackAsJson = JsonConvert.SerializeObject(feedback);

            return Ok(feedbackAsJson);
        }

        [HttpPut]
        public IHttpActionResult Update(TransactionData model)
        {
            return UpdateAsync(model);
        }

        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            return DeleteAsync(id);
        }
    }
}
