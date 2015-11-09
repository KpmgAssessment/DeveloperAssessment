using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.UIClient.Models
{
    public class ServerHelper
    {
        public static ICollection<T> GetAllFromServer<T>(string endpoint)
        {
            ICollection<T> returnData;
            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri(endpoint);
                HttpResponseMessage result = Task.Run(async () => { return await client.GetAsync(uri); }).Result;

                string asJson = result.Content.ReadAsStringAsync().Result;
                returnData =  JsonConvert.DeserializeObject<ICollection<T>>(asJson);
            }

            return returnData;
        }

        public static T GetOneFromServer<T>(string endpoint)
        {
            T returnData;
            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri(endpoint);
                HttpResponseMessage result = Task.Run(async () => { return await client.GetAsync(endpoint); }).Result;

                string asJson = result.Content.ReadAsStringAsync().Result;
                returnData = JsonConvert.DeserializeObject<T>(asJson);
            }
            return returnData;
        }

        public static FeedbackToClient PostUploads(string endpoint, HttpContent uploads)
        {
            FeedbackToClient failedUploads;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage result = Task.Run(async () => { return await client.PostAsync(endpoint, uploads); }).Result;
                MultipartMemoryStreamProvider resultContent = result.Content.ReadAsMultipartAsync().Result;

                string json = resultContent.Contents[0].ReadAsStringAsync().Result;
                failedUploads = JsonConvert.DeserializeObject<FeedbackToClient>(json);
            }

            return failedUploads;
        }

        public static HttpResponseMessage Put<T>(string endpoint, T data)
        {
            HttpResponseMessage result;

            using (HttpClient client = new HttpClient())
            {
                result = Task.Run(async () => { return await client.PutAsJsonAsync(endpoint, data); }).Result;
            }

            return result;
        }

        public static HttpResponseMessage Delete(string endpoint)
        {
            HttpResponseMessage result;

            using (HttpClient client = new HttpClient())
            {
                result = Task.Run(async () => { return await client.DeleteAsync(endpoint); }).Result;
            }

            return result;
        }
    }
}
