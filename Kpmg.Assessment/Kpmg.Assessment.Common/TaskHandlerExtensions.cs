using Kpmg.Assessment.TaskHandler;
using Kpmg.Assessment.TaskHandler.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kpmg.Assessment.Common
{
    public static class TaskHandlerExtensions
    {
        public static void TriggerConsumers<T, U>(this ICanConsume<T, U> consumer, ref BlockingCollection<T> dataQueue, ref BlockingCollection<U> errorQueue) 
            where T : class where U : class
        {
            consumer.ValidDataQueue = dataQueue;
            consumer.ValidationResultQueue = errorQueue;

            Task.Run(() => { consumer.StartConsuming(batchSize: 500); });
            Task.Run(() => { consumer.ProcessErrors(batchSize: 500); });
        }

        public static ProducerTaskResult TriggerProducer<T, U>(this ICanProduce<T, U> producer, ref BlockingCollection<T> dataQueue, ref BlockingCollection<U> errorQueue) 
            where T : class where U : class
        {
            if (producer.TemporaryDirectory.Length == 0)
            {
                producer.TemporaryDirectory = HttpContext.Current.Server.MapPath("~/App_Data"); 
            }

            producer.ValidDataQueue = dataQueue;
            producer.ValidationResultQueue = errorQueue;

            return Task.Run(() => { return producer.StartProducing(); }).Result;
        }
    }
}
